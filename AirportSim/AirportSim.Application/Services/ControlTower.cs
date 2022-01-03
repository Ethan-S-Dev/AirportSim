using AirportSim.Application.Interfaces;
using AirportSim.Domain.Interfaces;
using AirportSim.Domain.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace AirportSim.Application.Services
{
    public class ControlTower : IControlTower
    {
        private readonly IHubService hubService;
        private readonly Airport airport;
        private readonly ConcurrentDictionary<Airplane, Airplane> airplanes;

        public ControlTower(IHubService hubService)
        {
            this.hubService = hubService;
            airport = new Airport();
            airplanes = new ConcurrentDictionary<Airplane,Airplane>();
        }

        public bool TryLand(Airplane plane)
        {
            if(!airplanes.TryAdd(plane, plane))
                return false;
            plane.MovingStation += Plane_MovingStation;
            plane.StartLanding(airport.LandingStations);
            return true;
        }

        public bool TryDeparture(Airplane plane)
        {
            if (!airplanes.TryAdd(plane, plane))
                return false;
            plane.MovingStation += Plane_MovingStation;
            plane.StartDeparture(airport.DepartureStations);
            return true;
        }

        private async Task Plane_MovingStation(Airplane sender, MovingStationEventArgs args)
        {
            if(args.IsOver)
                airplanes.TryRemove(sender, out _);

            // hubService.SendUpdate();
            // Save in database
        }
    }
}
