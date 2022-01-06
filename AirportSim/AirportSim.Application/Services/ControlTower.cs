using AirportSim.Application.Interfaces;
using AirportSim.Domain.Dtos;
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

            var dto = await airportRepository.AddPlaneAsync(plane,Path.Landing);

            await hubService.SendAirplaneEnteredAsync(dto);

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

            var dto = await airportRepository.AddPlaneAsync(plane, Path.Departing);

            await hubService.SendAirplaneEnteredAsync(dto);

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
            var dto = await airportRepository.AddStationEventAsync(station,eventId, StationEvents.Fire, time);

            await hubService.SendEventEnteredAsync(dto);

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
            var dto = await airportRepository.AddStationEventAsync(station,eventId, StationEvents.Cracks, time);

            await hubService.SendEventEnteredAsync(dto);

            _ = station.StartStationEventAsync(eventId, StationEvents.Cracks, time);

            return true;
        }

        private async Task Plane_MovingStation(Airplane sender, MovingStationEventArgs args)
        {
            AirplaneDto dto;
            if (args.IsExiting)
            {
                airport.RemoveAirplane(sender.Id);
                dto = await airportRepository.RemovePlaneFromStationAsync(sender,sender.CurrentStation);

                await hubService.SendAiplaneRemovedAsync(dto);
                return;
            }

            if (args.IsEntering)
            {
                sender.IsOutside = false;
                sender.CurrentStation = args.Station;
                dto = await airportRepository.EnterPlaneToStationAsync(sender, args.Station);

                await hubService.SendAirplaneLandedAsync(dto);
                return;
            }


            dto = await airportRepository.MovePlaneStationsAsync(sender, sender.CurrentStation, args.Station);
            sender.CurrentStation = args.Station;

            await hubService.SendAirplaneMovedAsync(dto);
            return;                       
        }

        private async Task Station_EventEnded(Station sender, StationEventArgs args)
        {
            var dto = await airportRepository.RemoveStationEventAsync(sender, args.EventId);

            await hubService.SendEventRemovedAsync(dto);
        }

        private async Task Station_EventStarted(Station sender, StationEventArgs args)
        {
            var dto = await airportRepository.StartStationEventAsync(sender, args.EventId);

            await hubService.SendEventStartedAsync(dto);
        }

        public async Task LoadAirportStateAsync()
        {
           airport = await airportRepository.CreateAirportWithStateAsync();          
        }

        public Task<AirportDto> GetAirportStateAsync() => airportRepository.GetAirportStateAsync();
    }
}
