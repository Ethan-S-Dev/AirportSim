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
        private readonly Airport airport;      

        public ControlTower(IHubService hubService)
        {
            this.hubService = hubService;
            airport = new Airport();
            airport.StationEventStarted += Airport_StationEventStarted;
            airport.StationEventEnded += Airport_StationEventEnded;
        }

        public bool TryLand(Airplane plane)
        {
            if(!airport.Airplanes.TryAdd(plane.Id, plane))
                return false;
            plane.MovingStation += Plane_MovingStation;
            plane.StartLanding(airport.LandingStations);
            return true;
        }

        public bool TryDeparture(Airplane plane)
        {
            if (!airport.Airplanes.TryAdd(plane.Id, plane))
                return false;
            plane.MovingStation += Plane_MovingStation;
            plane.StartDeparture(airport.DepartureStations);
            return true;
        }

        public bool TryStartFire(string stationName,TimeSpan time)
        {
            if (!airport.Stations.ContainsKey(stationName))
                return false;
            _ = airport.Stations[stationName].DoStationEventAsync(StationEvents.Fire, time);
            return true;
        }

        public bool TryStartCracks(string stationName, TimeSpan time)
        {
            if (!airport.Stations.ContainsKey(stationName))
                return false;
            _ = airport.Stations[stationName].DoStationEventAsync(StationEvents.Cracks, time);
            return true;
        }

        private async Task Plane_MovingStation(Airplane sender, MovingStationEventArgs args)
        {
            Airplane toRemoved = null;
            if (args.IsOver)            
                airport.Airplanes.TryRemove(sender.Id, out toRemoved);


            // TODO: hubService.SendUpdate();
            // TODO: Save in database          
        }

        private Task Airport_StationEventEnded(Station sender, StationEventArgs args)
        {
            // TODO: hubService.SendUpdate();
            // TODO: Save in database  
        }

        private Task Airport_StationEventStarted(Station sender, StationEventArgs args)
        {
            // TODO: hubService.SendUpdate();
            // TODO: Save in database  
        }
    }
}
