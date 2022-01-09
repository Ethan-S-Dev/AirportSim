using AirportSim.Domain.Dtos;
using AirportSim.Domain.Interfaces;
using AirportSim.Domain.Models;
using AirportSim.Infra.Data.Entities;
using AirportSim.Infra.Data.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirportSim.Infra.Data
{
    public class StateObject
    {
        public IAirportContext Context { get; set; }
    }
    public class AirportRepository : IAirportRepository
    {
        private readonly IAirportContext airportContext;
        private readonly ILogger<IAirportRepository> logger;
        private readonly ConcurrentQueue<Task<Task<IDto>>> dbCommandQueue;
        private readonly SemaphoreSlim @lock = new(1);

        public AirportRepository(IAirportContext airportContext, ILogger<AirportRepository> logger)
        {
            this.airportContext = airportContext;
            this.logger = logger;
            dbCommandQueue = new ConcurrentQueue<Task<Task<IDto>>>();
        }
        public async Task<IAirport> CreateAirportWithStateAsync()
        {
            await @lock.WaitAsync();
            var stations = airportContext.Stations
                .Select(se => new Station(TimeSpan.FromSeconds(se.WaitTimeInSeconds), se.Name, se.DisplayName, se.IsEventable, se.IsLandable, se.IsDepartable) as IStation)
                .ToDictionary(s => s.Name);

            var planes = airportContext.Airplanes
                .Select(x => new Airplane(x.Id, x.Type,(Objectives)x.Objective, x.IsOutside) as IAirplane)
                .ToDictionary(p => p.Id);

            foreach (var station in stations.Values)
            {
                var stationEntity = await airportContext.FindStationAsync(station.Name);
                foreach (var landingStationName in stationEntity.LandStationNames)
                {
                    var landingStation = stations[landingStationName];
                    station.LandStations.Add(landingStation);
                }

                foreach (var departureStationName in stationEntity.DepartureStationNames)
                {
                    var departureStation = stations[departureStationName];
                    station.DepartureStations.Add(departureStation);
                }
            }

            // start current events and planes in station

            var departableStations = stations.Values.Where(s => s.IsDepartable).ToList();
            var landbleStations = stations.Values.Where(s => s.IsLandable).ToList();

            var planeEntitiesInside = airportContext.Airplanes
                .Where(p => !p.IsOutside)
                .Select(p => new Tuple<AirplaneEntity, IAirplane>(p, planes[p.Id]))
                .ToList();

            var startedStationEventEntities = airportContext.Events.Where(s => s.IsStarted).ToList();

            var dateableEvents = airportContext.Events.Where(s => !s.IsStarted).Select(s => (IDateable)s);
            var dateablePlanes = airportContext.Airplanes.Where(p => p.IsOutside).Select(s => (IDateable)s);
            var planesAndEventsAsDateable = dateableEvents.Concat(dateablePlanes).OrderBy(e => e.Date).ToList();

            foreach (var (entity, plane) in planeEntitiesInside)
            {

                var path = (Objectives)entity.Objective;
                var currentStaion = stations[entity.CurrentStationName];
                IStation privewStation = null;
                if(entity.PreviousStationName != null)
                    privewStation = stations[entity.PreviousStationName];

                var entering = (path == Objectives.Departing && currentStaion.IsDepartable) || (path == Objectives.Landing && currentStaion.IsLandable);
                plane.CurrentStation = currentStaion;
                plane.PreviousStation = privewStation;

                _ = (plane as ILoadPlane).EnterStation(currentStaion, path, entering);
            }

            foreach (var stationEventEntity in startedStationEventEntities)
            {
                var station = stations[stationEventEntity.StationName] as ILoadStation;
                var eventType = stationEventEntity.EventType;
                var time = TimeSpan.FromSeconds(stationEventEntity.EventTimeInSeconds);

                _ = station.ContinueStationEventAsync(stationEventEntity.Id, eventType, time);
            }

            foreach (var entity in planesAndEventsAsDateable)
            {
                if (entity is AirplaneEntity)
                {
                    var planeEntity = (AirplaneEntity)entity;

                    var objective = (Objectives)planeEntity.Objective;
                    var plane = planes[planeEntity.Id];
                    plane.Start(landbleStations, objective);
                    continue;
                }

                if (entity is StationEventEntity)
                {
                    var stationEvent = (StationEventEntity)entity;

                    var station = stations[stationEvent.StationName];
                    var eventType = stationEvent.EventType;
                    var time = TimeSpan.FromSeconds(stationEvent.EventTimeInSeconds);

                    _ = station.StartStationEventAsync(stationEvent.Id, eventType, time);
                }
            }
            @lock.Release();
            return new Airport(stations.Values, planes.Values);
        }
        public async Task<AirportDto> GetAirportStateAsync()
        {
            await @lock.WaitAsync();
            var airportDto = new AirportDto()
            {
                Airplanes = airportContext.Airplanes.Select(x => new AirplaneDto()
                {
                    Id = x.Id,
                    CurrentStationName = x.CurrentStationName,
                    PreviousStationName = x.PreviousStationName,
                    EnteredAt = x.EnteredAt,
                    IsOutside = x.IsOutside,
                    Objective = Enum.GetName(typeof(Objectives), x.Objective),
                    Type = x.Type
                }).ToList(),
                Events = airportContext.Events.Select(x => new EventDto()
                {
                    Id = x.Id,
                    EventTimeInSeconds = x.EventTimeInSeconds,
                    EventType = x.EventType,
                    IsStarted = x.IsStarted,
                    ReceivedAt = x.RecivedAt,
                    StationName = x.StationName,
                }).ToList(),
                Stations = airportContext.Stations.Select(x => new StationDto()
                {
                    Name = x.Name,
                    DisplayName = x.DisplayName,
                    CurrentPlaneId = x.CurrentPlaneId,
                    IsEventable = x.IsEventable,
                    WaitTimeInSeconds = x.WaitTimeInSeconds,
                }).ToList()
            };

            @lock.Release();
            return airportDto;
        }

        public async Task<AirplaneDto> AddPlaneAsync(IAirplane plane, Objectives objective)
        {
            var task = new Task<Task<IDto>>(() => AddPlane(plane, objective));

            logger.LogDebug($"Adding AddPlane task id: {task.Id}");
            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop(task.Id);

            await task;

            return await task.Result as AirplaneDto;

            //await @lock.WaitAsync();
            //var result = await AddPlane(plane, objective);
            //@lock.Release();
            //return result;
        }
        public async Task<EventDto> AddStationEventAsync(IStation station, Guid eventId, string type, TimeSpan time)
        {
            var task = new Task<Task<IDto>>(() => AddEvent(station, eventId, type, time));

            logger.LogDebug($"Adding AddStationEvent task id: {task.Id}");
            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop(task.Id);

            await task;

            return await task.Result as EventDto;

            //await @lock.WaitAsync();
            //var result = await AddEvent(station, eventId, type, time);
            //@lock.Release();
            //return result;
        }
        public async Task<AirplaneDto> MovePlaneStationsAsync(IAirplane sender, IStation priviewsStation, IStation nextStation)
        {
            var task = new Task<Task<IDto>>(() => MovePlane(sender, priviewsStation, nextStation));

            logger.LogDebug($"Adding MovePlane task id: {task.Id}");
            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop(task.Id);

            await task;

            return await task.Result as AirplaneDto;

            //await @lock.WaitAsync();
            //var result = await MovePlane(sender, priviewsStation, nextStation);
            //@lock.Release();
            //return result;
        }
        public async Task<AirplaneDto> EnterPlaneToStationAsync(IAirplane sender, IStation nextStation)
        {
            var task = new Task<Task<IDto>>(() => EnterPlane(sender, nextStation));

            logger.LogDebug($"Adding EnterPlane task id: {task.Id}");
            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop(task.Id);

            await task;

            return await task.Result as AirplaneDto;

            //await @lock.WaitAsync();
            //var result = await EnterPlane(sender, nextStation);
            //@lock.Release();
            //return result;
        }
        public async Task<AirplaneDto> RemovePlaneFromStationAsync(IAirplane sender, IStation priviewsStation)
        {
            var task = new Task<Task<IDto>>(() => RemovePlane(sender, priviewsStation));

            logger.LogDebug($"Adding ReomvePlane task id: {task.Id}");
            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop(task.Id);

            await task;

            return await task.Result as AirplaneDto;
        }
        public async Task<EventDto> RemoveStationEventAsync(IStation sender, Guid eventId)
        {
            var task = new Task<Task<IDto>>(() => RemoveEvent(eventId));

            logger.LogDebug($"Adding RemoveStationEvent task id: {task.Id}");
            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop(task.Id);

            await task;

            return await task.Result as EventDto;

            //await @lock.WaitAsync();
            //var result = await RemoveEvent(eventId);
            //@lock.Release();
            //return result;
        }
        public async Task<EventDto> StartStationEventAsync(IStation sender, Guid eventId)
        {
            var task = new Task<Task<IDto>>(() => StartEvent(eventId));

            logger.LogDebug($"Adding StartStationEvent task id: {task.Id}");
            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop(task.Id);

            await task;

            return await task.Result as EventDto;

            //await @lock.WaitAsync();
            //var result = await StartEvent(eventId);
            //@lock.Release();
            //return result;
        }

        private async Task AsyncExecutionLoop(int forTask)
        {
            logger.LogDebug($"Asking db for task {forTask}");
            await @lock.WaitAsync();
            logger.LogDebug($"Getting db for task {forTask}");
                while (dbCommandQueue.TryDequeue(out var task))
                {
                    try
                    {
                        logger.LogDebug($"Starting task id: {task.Id}");
                        task.Start();
                        await task;
                        await task.Result;
                        logger.LogDebug($"Finished task id: {task.Id}");
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug($"Failed task id: {task.Id}, Because {ex.Message}");
                    }
                }        
            logger.LogDebug($"Releasing db from task {forTask}");
            @lock.Release();
        }

        private async Task<IDto> AddPlane(IAirplane plane, Objectives objective)
        {
            
            try
            {
                var entity = new AirplaneEntity
                {
                    Id = plane.Id,
                    Type = plane.Type,
                    Objective = (int)objective,
                    EnteredAt = DateTimeOffset.UtcNow,
                    IsOutside = true
                };
                await airportContext.AddPlaneAsync(entity);
                await airportContext.SaveChangesAsync();

                return new AirplaneDto
                {
                    Id = entity.Id,
                    EnteredAt = entity.EnteredAt,
                    IsOutside = entity.IsOutside,
                    Objective = Enum.GetName(typeof(Objectives), objective),
                    Type = plane.Type,
                    CurrentStationName = entity.CurrentStationName,
                    PreviousStationName = entity.PreviousStationName                
                };
            }
            catch (InvalidProgramException ex)
            {
                logger.LogDebug($"Failed to AddPlane, {ex.Message}");
            }
            return null;
        }
        private async Task<IDto> EnterPlane(IAirplane sender, IStation nextStation)
        {
            try
            {
                var planeEntity = await airportContext.FindAirplaneAsync(sender.Id);
                var enteredStationEntity = await airportContext.FindStationAsync(nextStation.Name);

                enteredStationEntity.CurrentPlaneId = planeEntity.Id;
                planeEntity.IsOutside = sender.IsOutside;
                planeEntity.CurrentStationName = enteredStationEntity.Name;

                await airportContext.SaveChangesAsync();

                return new AirplaneDto
                {
                    Id = planeEntity.Id,
                    CurrentStationName = planeEntity.CurrentStationName,
                    EnteredAt = planeEntity.EnteredAt,
                    IsOutside = planeEntity.IsOutside,
                    Objective = Enum.GetName(typeof(Objectives), planeEntity.Objective),
                    Type = planeEntity.Type
                };
            }
            catch (InvalidProgramException ex)
            {
                logger.LogDebug($"Failed to EnterPlane, {ex.Message}");
            }

            return null;
        }
        private async Task<IDto> MovePlane(IAirplane sender, IStation priviewsStation, IStation nextStation)
        {
            try
            {
                var planeEntity = await airportContext.FindAirplaneAsync(sender.Id);
                var oldStationEntity = await airportContext.FindStationAsync(priviewsStation.Name);
                var newStationEntity = await airportContext.FindStationAsync(nextStation.Name);

                oldStationEntity.CurrentPlaneId = Guid.Empty;
                planeEntity.CurrentStationName = newStationEntity.Name;
                planeEntity.PreviousStationName = priviewsStation.Name;
                planeEntity.IsOutside = sender.IsOutside;
                newStationEntity.CurrentPlaneId = planeEntity.Id;

                await airportContext.SaveChangesAsync();

                return new AirplaneDto
                {
                    Id = planeEntity.Id,
                    CurrentStationName = planeEntity.CurrentStationName,
                    PreviousStationName = planeEntity.PreviousStationName,
                    EnteredAt = planeEntity.EnteredAt,
                    IsOutside = planeEntity.IsOutside,
                    Objective = Enum.GetName(typeof(Objectives), planeEntity.Objective),
                    Type = planeEntity.Type
                };
            }
            catch (InvalidOperationException ex)
            {
                logger.LogDebug($"Failed to MovePlane, {ex.Message}");
            }
            return null;
        }
        private async Task<IDto> RemovePlane(IAirplane sender, IStation priviewsStation)
        {
            try
            {
                var oldStationEntity = await airportContext.FindStationAsync(priviewsStation.Name);
                var planeEntity = await airportContext.FindAirplaneAsync(sender.Id);

                oldStationEntity.CurrentPlaneId = null;
                planeEntity.CurrentStationName = null;
                planeEntity.PreviousStationName = priviewsStation.Name;
                airportContext.RemoveAirplane(planeEntity);
                await airportContext.SaveChangesAsync();

                return new AirplaneDto
                {
                    Id = planeEntity.Id,
                    CurrentStationName = planeEntity.CurrentStationName,
                    PreviousStationName= planeEntity.PreviousStationName,
                    EnteredAt = planeEntity.EnteredAt,
                    IsOutside = planeEntity.IsOutside,
                    Objective = Enum.GetName(typeof(Objectives), planeEntity.Objective),
                    Type = planeEntity.Type
                };
            }
            catch (InvalidOperationException ex)
            {
                logger.LogDebug($"Failed to RemovePlane, {ex.Message}");
            }
            return null;
        }
        private async Task<IDto> AddEvent(IStation station, Guid eventId, string type, TimeSpan time)
        {
            try
            {
                var entity = new StationEventEntity
                {
                    Id = eventId,
                    EventTimeInSeconds = time.TotalSeconds,
                    StationName = station.Name,
                    EventType = type,
                    IsStarted = false,
                    RecivedAt = DateTimeOffset.UtcNow
                };
                await airportContext.AddEventAsync(entity);
                await airportContext.SaveChangesAsync();

                return new EventDto
                {
                    Id = entity.Id,
                    EventTimeInSeconds = entity.EventTimeInSeconds,
                    EventType = entity.EventType,
                    IsStarted = entity.IsStarted,
                    ReceivedAt = entity.RecivedAt,
                    StationName = entity.StationName
                };
            }
            catch (InvalidOperationException ex)
            {
                logger.LogDebug($"Failed to AddEvent, {ex.Message}");
            }
            return null;
        }
        private async Task<IDto> StartEvent(Guid eventId)
        {
            try
            {
                var eventEntity = await airportContext.FindEventAsync(eventId);

                eventEntity.IsStarted = true;
                await airportContext.SaveChangesAsync();

                return new EventDto
                {
                    Id = eventEntity.Id,
                    EventTimeInSeconds = eventEntity.EventTimeInSeconds,
                    EventType = eventEntity.EventType,
                    IsStarted = eventEntity.IsStarted,
                    ReceivedAt = eventEntity.RecivedAt,
                    StationName = eventEntity.StationName,
                };
            }
            catch (InvalidOperationException ex)
            {
                logger.LogDebug($"Failed to StartEvent, {ex.Message}");
            }
            return null;
        }
        private async Task<IDto> RemoveEvent(Guid eventId)
        {
            try
            {
                var eventEntity = await airportContext.FindEventAsync(eventId);

                airportContext.RemoveEvent(eventEntity);
                await airportContext.SaveChangesAsync();

                return new EventDto
                {
                    Id = eventEntity.Id,
                    EventTimeInSeconds = eventEntity.EventTimeInSeconds,
                    EventType = eventEntity.EventType,
                    IsStarted = eventEntity.IsStarted,
                    ReceivedAt = eventEntity.RecivedAt,
                    StationName = eventEntity.StationName,
                };
            }
            catch (InvalidOperationException ex)
            {
                logger.LogDebug($"Failed to RemoveEvent, {ex.Message}");
            }
            return null;
        }
    }
}
