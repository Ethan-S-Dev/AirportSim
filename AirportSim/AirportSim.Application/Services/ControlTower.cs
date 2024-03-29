﻿using AirportSim.Application.Interfaces;
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

        public ControlTower(IHubService hubService,IAirportRepository airportRepository)
        {
            this.hubService = hubService;
            this.airportRepository = airportRepository;
        }

        public async Task<(bool IsSuccess,string Message)> TryLandAsync(Guid id,string type)
        {
            if (airport == null)
                return (false, "Initializing airport");

            if (id == Guid.Empty)
                return (false, "Invalid id");

            if (string.IsNullOrEmpty(type))
                return (false, "Invalid type");

            if (!airport.CanLand())
                return (false, "Landing path to busy");


           var plane = new Airplane(id, type, Objectives.Landing,true);

            if(!airport.TryAddAirplan(plane))
                return (false, $"Airplane with Id {plane.Id} all ready exist in the airport");

            var dto = await airportRepository.AddPlaneAsync(plane,Objectives.Landing);

            await hubService.SendAirplaneEnteredAsync(dto);

            plane.MovingStation += Plane_MovingStation;
            plane.Start(airport.LandingStations,Objectives.Landing);
       
            return (true, $"Airplane with Id {plane.Id} successfully entered airport airspace");
        }
        public async Task<(bool IsSuccess, string Message)> TryDepartureAsync(Guid id, string type)
        {
            if (airport == null)
                return (false, "Initializing airport"); ;

            if (airport == null)
                return (false, "Initializing airport");

            if (id == Guid.Empty)
                return (false, "Invalid id");

            if (!airport.CanDeparture())
                return (false, "Departure path to busy");

            var plane = new Airplane(id, type,Objectives.Departing,true);

            if (!airport.TryAddAirplan(plane))
                return (false, $"Airplane with Id {plane.Id} all ready exist in the airport");

            var dto = await airportRepository.AddPlaneAsync(plane, Objectives.Departing);

            await hubService.SendAirplaneEnteredAsync(dto);

            plane.MovingStation += Plane_MovingStation;
            plane.Start(airport.DepartureStations,Objectives.Departing);

            return (true, $"Airplane with Id {plane.Id} successfully entered departure queue");
        }       
        public async Task<(bool IsSuccess, string Message)> TryStartEventAsync(string eventType, string stationName, TimeSpan timeSpan)
        {
            if (airport == null)
                return (false, "Initializing airport"); ;

            if (!airport.TryGetStation(stationName, out var station))
                return (false, $"Station with name: {stationName} doesn't exist in the airport");

            var eventId = Guid.NewGuid();
            var dto = await airportRepository.AddStationEventAsync(station, eventId, eventType, timeSpan);

            await hubService.SendEventEnteredAsync(dto);

            _ = station.StartStationEventAsync(eventId, eventType, timeSpan);

            return (true, $"Cracks event on station: {station.DisplayName} successfully entered event queue");
        }
        public async Task LoadAirportStateAsync()
        {
            if (airport != null)
                return;

            airport = await airportRepository.CreateAirportWithStateAsync();
            airport.StationEventStarted += Station_EventStarted;
            airport.StationEventEnded += Station_EventEnded;
        }
        public Task<AirportDto> GetAirportStateAsync() => airportRepository.GetAirportStateAsync();

        private async Task Plane_MovingStation(IAirplane sender, MovingStationEventArgs args)
        {
            AirplaneDto dto;
            if (args.IsExiting)
            {
                dto = await airportRepository.RemovePlaneFromStationAsync(sender,sender.PreviousStation);
                airport.RemoveAirplane(sender.Id);

                await hubService.SendAiplaneRemovedAsync(dto);
                return;
            }

            if (args.IsEntering)
            {
                sender.IsOutside = false;
                dto = await airportRepository.EnterPlaneToStationAsync(sender, args.Station);

                await hubService.SendAirplaneLandedAsync(dto);
                return;
            }

            dto = await airportRepository.MovePlaneStationsAsync(sender, sender.PreviousStation, args.Station);

            await hubService.SendAirplaneMovedAsync(dto);
            return;                       
        }
        private async Task Station_EventEnded(IStation sender, StationEventArgs args)
        {
            var dto = await airportRepository.RemoveStationEventAsync(sender, args.EventId);

            await hubService.SendEventRemovedAsync(dto);
        }
        private async Task Station_EventStarted(IStation sender, StationEventArgs args)
        {
            var dto = await airportRepository.StartStationEventAsync(sender, args.EventId);

            await hubService.SendEventStartedAsync(dto);
        }
    }
}
