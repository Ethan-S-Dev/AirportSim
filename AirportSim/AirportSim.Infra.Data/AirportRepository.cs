using AirportSim.Domain.Interfaces;
using AirportSim.Domain.Models;
using AirportSim.Infra.Data.Entities;
using AirportSim.Infra.Data.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportSim.Infra.Data
{
    public class AirportRepository : IAirportRepository
    {
        private readonly IAirportContext airportContext;

        private readonly ConcurrentQueue<Task> dbCommandQueue;

        public AirportRepository(IAirportContext airportContext)
        {
            this.airportContext = airportContext;
            dbCommandQueue = new ConcurrentQueue<Task>();           
        }

        public async Task AddPlaneAsync(Airplane plane, Path objective)
        {
            var task = new Task(async () =>
            {
                await airportContext.AddPlaneAsync(new AirplaneEntity
                {
                    Id = plane.Id,
                    Type = plane.Type,
                    Objective = (int)objective,
                    EnteredAt = DateTimeOffset.UtcNow,
                    IsOutside = true
                });
                await airportContext.SaveChangesAsync();
            });

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;
        }

        public async Task AddStationEventAsync(Station station, Guid eventId, StationEvents type, TimeSpan time)
        {
            var task = new Task(async () =>
            {
                await airportContext.AddEventAsync(new StationEventEntity
                {
                    Id = eventId,
                    EventTime = time,
                    StationName = station.Name,
                    EventType = (int)type,
                    IsStarted = false,
                    RecivedAt = DateTimeOffset.UtcNow
                });
                await airportContext.SaveChangesAsync();
            });

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;
        }

        public async Task<IAirport> CreateAirportWithStatreAsync()
        {
            var stations = airportContext.Stations
                .Select(se => new Station(se.WaitTime, se.Name, se.DisplayName, se.IsEventable, se.IsLandable, se.IsDepartable))
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

                foreach (var departureStationName in stationEntity.DepartureStations)
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
                var eventType = (StationEvents)stationEventEntity.EventType;
                var time = stationEventEntity.EventTime;

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
                    var eventType = (StationEvents)stationEvent.EventType;
                    var time = stationEvent.EventTime;

                    _ = station.StartStationEventAsync(stationEvent.Id, eventType, time);
                }
            }

            return new Airport(stations.Values, planes.Values);
        }

        public async Task MovePlaneStationsAsync(Airplane sender, Station priviewsStation, Station nextStation)
        {
            var task = new Task(async () =>
            {
                var planeEntity = await airportContext.FindAirplaneAsync(sender.Id);
                var oldStationEntity = await airportContext.FindStationAsync(priviewsStation.Name);
                var newStationEntity = await airportContext.FindStationAsync(nextStation.Name);

                oldStationEntity.CurrentPlaneId = Guid.Empty;
                planeEntity.StationName = newStationEntity.Name;
                planeEntity.IsOutside = sender.IsOutside;
                newStationEntity.CurrentPlaneId = planeEntity.Id;

                await airportContext.SaveChangesAsync();
            });

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;
        }

        public async Task EnterPlaneToStationAsync(Airplane sender, Station nextStation)
        {
            var task = new Task(async () =>
            {

                var planeEntity = await airportContext.FindAirplaneAsync(sender.Id);
                var enteredStationEntity = await airportContext.FindStationAsync(nextStation.Name);

                enteredStationEntity.CurrentPlaneId = planeEntity.Id;
                planeEntity.IsOutside = sender.IsOutside;
                planeEntity.StationName = enteredStationEntity.Name;

                await airportContext.SaveChangesAsync();
                return;

            });

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;
        }

        public async Task RemovePlaneFromStationAsync(Airplane sender, Station priviewsStation)
        {
            var task = new Task(async () =>
            {
                var oldStationEntity = await airportContext.FindStationAsync(priviewsStation.Name);
                var planeEntity = await airportContext.FindAirplaneAsync(sender.Id);

                oldStationEntity.CurrentPlaneId = Guid.Empty;
                airportContext.RemoveAirplane(planeEntity);
                await airportContext.SaveChangesAsync();
            });

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;
        }

        public async Task RemoveStationEventAsync(Station sender, Guid eventId)
        {
            var task = new Task(async () =>
            {
                var eventEntity = await airportContext.FindEventAsync(eventId);

                airportContext.RemoveEvent(eventEntity);
                await airportContext.SaveChangesAsync();
            });

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;
        }

        public async Task StartStationEventAsync(Station sender, Guid eventId)
        {
            var task = new Task(async () =>
            {
                var eventEntity = await airportContext.FindEventAsync(eventId);

                eventEntity.IsStarted = true;
                await airportContext.SaveChangesAsync();
            }); 

            dbCommandQueue.Enqueue(task);

            _ = AsyncExecutionLoop();

            await task;
        }

        private async Task AsyncExecutionLoop()
        {
            while (dbCommandQueue.TryDequeue(out var task))
            {
                await task;
            }
        }
    }
}
