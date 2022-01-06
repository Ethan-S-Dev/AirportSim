using AirportSim.Domain.Dtos;
using AirportSim.Domain.Interfaces;
using AirportSim.Domain.Models;
using AirportSim.Infra.Data.Entities;
using AirportSim.Infra.Data.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirportSim.Infra.Data
{
    public class AirportRepository : IAirportRepository
    {
        private readonly IAirportContext airportContext;
        private readonly ConcurrentQueue<Task> dbCommandQueue;
        private readonly SemaphoreSlim loopLock;

        public AirportRepository(IAirportContext airportContext)
        {
            this.airportContext = airportContext;
            dbCommandQueue = new ConcurrentQueue<Task>();
            loopLock = new SemaphoreSlim(1);
        }

        public async Task<AirplaneDto> AddPlaneAsync(Airplane plane, Path objective)
        {
            var task = new Task<Task<AirplaneDto>>(async () =>
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
            }); 
            
            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;

            return await task.Result;
        }
        public async Task<EventDto> AddStationEventAsync(Station station, Guid eventId, string type, TimeSpan time)
        {
            var task = new Task<Task<EventDto>>(async () =>
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
            });

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;

            return await task.Result;
        }
        public async Task<IAirport> CreateAirportWithStateAsync()
        {
            await loopLock.WaitAsync();
            var stations = airportContext.Stations
                .Select(se => new Station(TimeSpan.FromSeconds(se.WaitTimeInSeconds), se.Name, se.DisplayName, se.IsEventable, se.IsLandable, se.IsDepartable))
                .ToDictionary(s => s.Name);

            var planes = airportContext.Airplanes
                .Select(x => new Airplane()
                {
                    Id = x.Id,
                    Type = x.Type,
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
                .Select(p => new Tuple<AirplaneEntity,Airplane>(p, planes[p.Id]))
                .ToList();

            var startedStationEventEntities = airportContext.Events.Where(s => s.IsStarted).ToList();

            var dateableEvents = airportContext.Events.Where(s => !s.IsStarted).Select(s => (IDateable)s);
            var dateablePlanes = airportContext.Airplanes.Where(p => p.IsOutside).Select(s => (IDateable)s);
            var planesAndEventsAsDateable = dateableEvents.Concat(dateablePlanes).OrderBy(e => e.Date).ToList();

            foreach (var (entity,plane) in planeEntitiesInside)
            {

                var path = (Path)entity.Objective;
                var station = stations[entity.StationName];

                var entering = (path == Path.Departing && station.IsDepartable) || (path == Path.Landing && station.IsLandable);
                plane.CurrentStation = station;

                _  = (plane as IPlane).EnterStation(station, path, entering);
            }

            foreach (var stationEventEntity in startedStationEventEntities)
            {
                var station = stations[stationEventEntity.StationName] as IStation;
                var eventType = stationEventEntity.EventType;
                var time = TimeSpan.FromSeconds(stationEventEntity.EventTimeInSeconds);

                _ =  station.ContinueStationEventAsync(stationEventEntity.Id, eventType, time);
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
            loopLock.Release();
            return new Airport(stations.Values, planes.Values);
        }
        public async Task<AirplaneDto> MovePlaneStationsAsync(Airplane sender, Station priviewsStation, Station nextStation)
        {
            var task = new Task<Task<AirplaneDto>>(async () =>
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
            });

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;

            return await task.Result;
        }
        public async Task<AirplaneDto> EnterPlaneToStationAsync(Airplane sender, Station nextStation)
        {
            var task = new Task<Task<AirplaneDto>>(async () =>
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
            });

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;

            return await task.Result;
        }
        public async Task<AirplaneDto> RemovePlaneFromStationAsync(Airplane sender, Station priviewsStation)
        {
            var task = new Task<Task<AirplaneDto>>(async () =>
            {
                var oldStationEntity = await airportContext.FindStationAsync(priviewsStation.Name);
                var planeEntity = await airportContext.FindAirplaneAsync(sender.Id);

                oldStationEntity.CurrentPlaneId = Guid.Empty;
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
            });

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;

            return await task.Result;
        }
        public async Task<EventDto> RemoveStationEventAsync(Station sender, Guid eventId)
        {
            var task = new Task<Task<EventDto>>(async () =>
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
            });

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;

            return await task.Result;
        }
        public async Task<EventDto> StartStationEventAsync(Station sender, Guid eventId)
        {
            var task = new Task<Task<EventDto>>(async () =>
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
            });

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;

            return await task.Result;
        }
        public async Task<AirportDto> GetAirportStateAsync()
        {
            await loopLock.WaitAsync();

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

            loopLock.Release();
            return airportDto;
        }
        private async Task AsyncExecutionLoop()
        {
            await loopLock.WaitAsync();
            while (dbCommandQueue.TryDequeue(out var task))
            {
                task.Start();
                await task;
            }
            loopLock.Release();
        }            
    }
}
