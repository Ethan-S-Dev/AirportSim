using AirportSim.Application.Interfaces;
using AirportSim.Domain.Interfaces;
using AirportSim.Domain.Models;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AirportSim.Application.Services
{
    public class ControlTower : IControlTower
    {
        private readonly IHubService hubService;
        private readonly IAirportRepository airportRepository;
        private Airport airport;      

        public ControlTower(IHubService hubService, IAirportRepository airportRepository)
        {
            this.hubService = hubService;
            this.airportRepository = airportRepository;           
            airport.StationEventStarted += Station_EventStarted;
            airport.StationEventEnded += Station_EventEnded;
        }

        public async Task<bool> TryLandAsync(Airplane plane)
        {
            if(!airport.Airplanes.TryAdd(plane.Id, plane))
                return false;

            await airportRepository.AddPlaneAsync(plane);

            plane.MovingStation += Plane_MovingStation;
            plane.StartLanding(airport.LandingStations);
            return true;
        }

        public async Task<bool> TryDepartureAsync(Airplane plane)
        {
            if (!airport.Airplanes.TryAdd(plane.Id, plane))
                return false;

            await airportRepository.AddPlaneAsync(plane);

            plane.MovingStation += Plane_MovingStation;
            plane.StartDeparture(airport.DepartureStations);
            return true;
        }

        public async Task<bool> TryStartFireAsync(string stationName,TimeSpan time)
        {
            if (!airport.Stations.TryGetValue(stationName,out var station))
                return false;

            var eventId = Guid.NewGuid();
            await airportRepository.AddStationEventAsync(station,eventId, StationEvents.Fire, time);

            _ = station.DoStationEventAsync(eventId, StationEvents.Fire, time);
            return true;
        }

        public async Task<bool> TryStartCracksAsync(string stationName, TimeSpan time)
        {
            if (!airport.Stations.TryGetValue(stationName,out var station))
                return false;

            var eventId = Guid.NewGuid();
            await airportRepository.AddStationEventAsync(station,eventId, StationEvents.Cracks, time);

            _ = station.DoStationEventAsync(eventId, StationEvents.Cracks, time);
            return true;
        }

        private async Task Plane_MovingStation(Airplane sender, MovingStationEventArgs args)
        {
            if (args.IsExiting)
                airport.Airplanes.TryRemove(sender.Id, out _);        

            await airportRepository.MovePlaneToStationAsync(sender, args.Station);

            // TODO: hubService.SendUpdate();     
        }

        private async Task Station_EventEnded(Station sender, StationEventArgs args)
        {
            await airportRepository.RemoveStationEventAsync(sender, args.EventId);

            // TODO: hubService.SendUpdate();
        }

        private async Task Station_EventStarted(Station sender, StationEventArgs args)
        {
            await airportRepository.StartStationEventAsync(sender, args.EventId);

            // TODO: hubService.SendUpdate();
        }

        public void LoadAirportState()
        {
            var stations = airportRepository.LoadStations();

            var airplanes = airportRepository.LoadPlanes();

            airport = new Airport(stations, airplanes);
        }
    }
}
