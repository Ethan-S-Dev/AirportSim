using AirportSim.Application.Interfaces;
using AirportSim.Domain.Interfaces;
using AirportSim.Domain.Models;
using System;
using System.Threading.Tasks;

namespace AirportSim.Application.Services
{
    public class ControlTower : IControlTower
    {
        private readonly IHubService hubService;
        private readonly IAirportRepository airportRepository;
        private IAirport airport;      

        public ControlTower(IHubService hubService, IAirportRepository airportRepository)
        {
            this.hubService = hubService;
            this.airportRepository = airportRepository;           
            airport.StationEventStarted += Station_EventStarted;
            airport.StationEventEnded += Station_EventEnded;
        }

        public async Task<bool> TryLandAsync(Airplane plane)
        {
            if (airport == null)
                return false;

            if(!airport.TryAddAirplan(plane.Id, plane))
                return false;

            await airportRepository.AddPlaneAsync(plane,Path.Landing);

            // TODO: hubService.Update();

            plane.MovingStation += Plane_MovingStation;
            plane.StartLanding(airport.LandingStations);
       
            return true;
        }

        public async Task<bool> TryDepartureAsync(Airplane plane)
        {
            if (airport == null)
                return false;

            if (!airport.TryAddAirplan(plane.Id, plane))
                return false;

            await airportRepository.AddPlaneAsync(plane, Path.Departing);

            // TODO: hubService.Update();

            plane.MovingStation += Plane_MovingStation;
            plane.StartDeparture(airport.DepartureStations);

            return true;
        }

        public async Task<bool> TryStartFireAsync(string stationName,TimeSpan time)
        {
            if (airport == null)
                return false;

            if (!airport.TryGetStation(stationName,out var station))
                return false;

            var eventId = Guid.NewGuid();
            await airportRepository.AddStationEventAsync(station,eventId, StationEvents.Fire, time);

            // TODO: hubService.Update();

            _ = station.StartStationEventAsync(eventId, StationEvents.Fire, time);

            return true;
        }

        public async Task<bool> TryStartCracksAsync(string stationName, TimeSpan time)
        {
            if (airport == null)
                return false;

            if (!airport.TryGetStation(stationName,out var station))
                return false;

            var eventId = Guid.NewGuid();
            await airportRepository.AddStationEventAsync(station,eventId, StationEvents.Cracks, time);

            // TODO: hubService.Update();

            _ = station.StartStationEventAsync(eventId, StationEvents.Cracks, time);

            return true;
        }

        private async Task Plane_MovingStation(Airplane sender, MovingStationEventArgs args)
        {

            if (args.IsExiting)
            {
                airport.RemoveAirplane(sender.Id);
                await airportRepository.RemovePlaneFromStationAsync(sender,sender.CurrentStation);

                // TODO: hubService.SendUpdate();
                return;
            }

            if (args.IsEntering)
            {
                sender.IsOutside = false;
                sender.CurrentStation = args.Station;
                await airportRepository.EnterPlaneToStationAsync(sender, args.Station);

                // TODO: hubService.SendUpdate();
                return;
            }


            await airportRepository.MovePlaneStationsAsync(sender, sender.CurrentStation, args.Station);
            sender.CurrentStation = args.Station;

             // TODO: hubService.SendUpdate();
            return;                       
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

        public async Task LoadAirportStateAsync()
        {
           airport = await airportRepository.CreateAirportWithStatreAsync();          
        }
    }
}
