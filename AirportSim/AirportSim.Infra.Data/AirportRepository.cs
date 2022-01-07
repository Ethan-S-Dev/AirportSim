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
        private readonly IAirportContextFactory airportContextFactory;
        private readonly ILogger<IAirportRepository> logger;
        private readonly ConcurrentQueue<Task<Task<IDto>>> dbCommandQueue;
        private readonly SemaphoreSlim @lock = new(1);

        public AirportRepository(IAirportContextFactory airportContextFactory, ILogger<AirportRepository> logger)
        {
            this.airportContextFactory = airportContextFactory;
            this.logger = logger;
            dbCommandQueue = new ConcurrentQueue<Task<Task<IDto>>>();
        }
        public async Task<IAirport> CreateAirportWithStateAsync()
        {
            await @lock.WaitAsync();
            using var airportContext = airportContextFactory.CreateAirportContext();

            var stations = airportContext.Stations
                .Select(se => new Station(TimeSpan.FromSeconds(se.WaitTimeInSeconds), se.Name, se.DisplayName, se.IsEventable, se.IsLandable, se.IsDepartable))
                .ToDictionary(s => s.Name);

            var planes = airportContext.Airplanes
                .Select(x => new Airplane(x.Id, x.Type)
                {
                    IsOutside = x.IsOutside
                }).ToDictionary(p => p.Id);

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
                .Select(p => new Tuple<AirplaneEntity, Airplane>(p, planes[p.Id]))
                .ToList();

            var startedStationEventEntities = airportContext.Events.Where(s => s.IsStarted).ToList();

            var dateableEvents = airportContext.Events.Where(s => !s.IsStarted).Select(s => (IDateable)s);
            var dateablePlanes = airportContext.Airplanes.Where(p => p.IsOutside).Select(s => (IDateable)s);
            var planesAndEventsAsDateable = dateableEvents.Concat(dateablePlanes).OrderBy(e => e.Date).ToList();

            foreach (var (entity, plane) in planeEntitiesInside)
            {

                var path = (Path)entity.Objective;
                var station = stations[entity.StationName];

                var entering = (path == Path.Departing && station.IsDepartable) || (path == Path.Landing && station.IsLandable);
                plane.CurrentStation = station;

                _ = (plane as IPlane).EnterStation(station, path, entering);
            }

            foreach (var stationEventEntity in startedStationEventEntities)
            {
                var station = stations[stationEventEntity.StationName] as IStation;
                var eventType = stationEventEntity.EventType;
                var time = TimeSpan.FromSeconds(stationEventEntity.EventTimeInSeconds);

                _ = station.ContinueStationEventAsync(stationEventEntity.Id, eventType, time);
            }

            foreach (var entity in planesAndEventsAsDateable)
            {
                if (entity is AirplaneEntity)
                {
                    var planeEntity = (AirplaneEntity)entity;

                    var objective = (Path)planeEntity.Objective;
                    var plane = planes[planeEntity.Id];
                    if (objective == Path.Departing)
                    {
                        plane.StartDeparture(departableStations);
                        continue;
                    }

                    if (objective == Path.Departing)
                    {
                        plane.StartLanding(landbleStations);
                    }
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
            using var airportContext = airportContextFactory.CreateAirportContext();

            var airportDto = new AirportDto()
            {
                Airplanes = airportContext.Airplanes.Select(x => new AirplaneDto()
                {
                    Id = x.Id,
                    CurrentStationName = x.StationName,
                    EnteredAt = x.EnteredAt,
                    IsOutside = x.IsOutside,
                    Objective = Enum.GetName(typeof(Path), x.Objective),
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

        public async Task<AirplaneDto> AddPlaneAsync(Airplane plane, Path objective)
        {
            var state = new StateObject();
            var task = new Task<Task<IDto>>((state) => AddPlane(plane, objective, (state as StateObject).Context), state);

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
        public async Task<EventDto> AddStationEventAsync(Station station, Guid eventId, string type, TimeSpan time)
        {
            var state = new StateObject();
            var task = new Task<Task<IDto>>((state) => AddEvent(station, eventId, type, time, (state as StateObject).Context), state);

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
        public async Task<AirplaneDto> MovePlaneStationsAsync(Airplane sender, Station priviewsStation, Station nextStation)
        {
            var state = new StateObject();
            var task = new Task<Task<IDto>>((state) => MovePlane(sender, priviewsStation, nextStation,(state as StateObject).Context), state);

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
        public async Task<AirplaneDto> EnterPlaneToStationAsync(Airplane sender, Station nextStation)
        {
            var state = new StateObject();
            var task = new Task<Task<IDto>>((state) => EnterPlane(sender, nextStation,(state as StateObject).Context), state);

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
        public async Task<AirplaneDto> RemovePlaneFromStationAsync(Airplane sender, Station priviewsStation)
        {
            var state = new StateObject();
            var task = new Task<Task<IDto>>((state) => RemovePlane(sender, priviewsStation,(state as StateObject).Context), state);

            logger.LogDebug($"Adding ReomvePlane task id: {task.Id}");
            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop(task.Id);

            await task;

            return await task.Result as AirplaneDto;

            //await @lock.WaitAsync();
            //var result = await RemovePlane(sender, priviewsStation);
            //@lock.Release();
            //return result;
        }
        public async Task<EventDto> RemoveStationEventAsync(Station sender, Guid eventId)
        {
            var state = new StateObject();
            var task = new Task<Task<IDto>>((state) => RemoveEvent(eventId,(state as StateObject).Context),state);

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
        public async Task<EventDto> StartStationEventAsync(Station sender, Guid eventId)
        {
            var state = new StateObject();
            var task = new Task<Task<IDto>>((state) => StartEvent(eventId,(state as StateObject).Context), state);

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
            using (var airportContext = airportContextFactory.CreateAirportContext())
            {
                while (dbCommandQueue.TryDequeue(out var task))
                {
                    try
                    {
                        logger.LogDebug($"Starting task id: {task.Id}");
                        (task.AsyncState as StateObject).Context = airportContext;
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
            }
            logger.LogDebug($"Releasing db from task {forTask}");
            @lock.Release();
        }

        private async Task<IDto> AddPlane(Airplane plane, Path objective,IAirportContext airportContext)
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
                    Objective = Enum.GetName(typeof(Path), objective),
                    Type = plane.Type,
                    CurrentStationName = entity.StationName
                };
            }
            catch (InvalidProgramException ex)
            {
                logger.LogDebug($"Failed to AddPlane, {ex.Message}");
            }
            return null;
        }
        private async Task<IDto> EnterPlane(Airplane sender, Station nextStation, IAirportContext airportContext)
        {
            try
            {
                var planeEntity = await airportContext.FindAirplaneAsync(sender.Id);
                var enteredStationEntity = await airportContext.FindStationAsync(nextStation.Name);

                enteredStationEntity.CurrentPlaneId = planeEntity.Id;
                planeEntity.IsOutside = sender.IsOutside;
                planeEntity.StationName = enteredStationEntity.Name;

                await airportContext.SaveChangesAsync();

                return new AirplaneDto
                {
                    Id = planeEntity.Id,
                    CurrentStationName = planeEntity.StationName,
                    EnteredAt = planeEntity.EnteredAt,
                    IsOutside = planeEntity.IsOutside,
                    Objective = Enum.GetName(typeof(Path), planeEntity.Objective),
                    Type = planeEntity.Type
                };
            }
            catch (InvalidProgramException ex)
            {
                logger.LogDebug($"Failed to EnterPlane, {ex.Message}");
            }

            return null;
        }
        private async Task<IDto> MovePlane(Airplane sender, Station priviewsStation, Station nextStation, IAirportContext airportContext)
        {
            try
            {
                var planeEntity = await airportContext.FindAirplaneAsync(sender.Id);
                var oldStationEntity = await airportContext.FindStationAsync(priviewsStation.Name);
                var newStationEntity = await airportContext.FindStationAsync(nextStation.Name);

                oldStationEntity.CurrentPlaneId = Guid.Empty;
                planeEntity.StationName = newStationEntity.Name;
                planeEntity.IsOutside = sender.IsOutside;
                newStationEntity.CurrentPlaneId = planeEntity.Id;

                await airportContext.SaveChangesAsync();

                return new AirplaneDto
                {
                    Id = planeEntity.Id,
                    CurrentStationName = planeEntity.StationName,
                    EnteredAt = planeEntity.EnteredAt,
                    IsOutside = planeEntity.IsOutside,
                    Objective = Enum.GetName(typeof(Path), planeEntity.Objective),
                    Type = planeEntity.Type
                };
            }
            catch (InvalidOperationException ex)
            {
                logger.LogDebug($"Failed to MovePlane, {ex.Message}");
            }
            return null;
        }
        private async Task<IDto> RemovePlane(Airplane sender, Station priviewsStation, IAirportContext airportContext)
        {
            try
            {
                var oldStationEntity = await airportContext.FindStationAsync(priviewsStation.Name);
                var planeEntity = await airportContext.FindAirplaneAsync(sender.Id);

                oldStationEntity.CurrentPlaneId = null;
                planeEntity.StationName = null;
                airportContext.RemoveAirplane(planeEntity);
                await airportContext.SaveChangesAsync();

                return new AirplaneDto
                {
                    Id = planeEntity.Id,
                    CurrentStationName = planeEntity.StationName,
                    EnteredAt = planeEntity.EnteredAt,
                    IsOutside = planeEntity.IsOutside,
                    Objective = Enum.GetName(typeof(Path), planeEntity.Objective),
                    Type = planeEntity.Type
                };
            }
            catch (InvalidOperationException ex)
            {
                logger.LogDebug($"Failed to RemovePlane, {ex.Message}");
            }
            return null;
        }
        private async Task<IDto> AddEvent(Station station, Guid eventId, string type, TimeSpan time, IAirportContext airportContext)
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
        private async Task<IDto> StartEvent(Guid eventId, IAirportContext airportContext)
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
        private async Task<IDto> RemoveEvent(Guid eventId, IAirportContext airportContext)
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
