using AirportSim.Simulator.Application.Interfaces;
using AirportSim.Simulator.Domain;
using AirportSim.Simulator.Domain.Interfaces;
using AirportSim.Simulator.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Application.Services
{
    public class Simulator : ISimulator
    {
        private readonly IAirportSimClient simClient;
        private readonly IRandomSimEvents rndSimEvent;
        private readonly IAirplaneGenerator airplaneGenerator;
        private readonly ILogger<Simulator> logger;
        private readonly IRandom random;
        private RandomTimer eventTimer;

        public Simulator(
            IAirportSimClient simClient,
            IRandomSimEvents rndSimEvent,
            IAirplaneGenerator airplaneGenerator ,
            ILogger<Simulator> logger,
            IRandom random)
        {
            this.simClient = simClient;
            this.rndSimEvent = rndSimEvent;
            this.airplaneGenerator = airplaneGenerator;
            this.logger = logger;
            this.random = random;
        }

        public void Init()
        {
            if (eventTimer == null)
            {
                eventTimer = new RandomTimer(random, 1, 8);
                eventTimer.Elapsed += EventTimer_Elapsed;
            }
        }

        public async Task<(bool IsSuccess, string Message)> SendCracksEventAsync()
        {
            var stationName = await rndSimEvent.GetRandomStationAsync();
            var result = await simClient.SendEventAsync(new StationEvent { EventTimeInSeconds = 3, EventType = "Cracks", StationName = stationName });
            return result;

        }

        public async Task<(bool IsSuccess, string Message)> SendDepartureAirplaneAsync()
        {
            var plane = await airplaneGenerator.CreateRandomPlaneAsync();
            var result = await simClient.SendDepartureAsync(plane);
            return result;
        }

        public async Task<(bool IsSuccess, string Message)> SendFireEventAsync()
        {
            var stationName = await rndSimEvent.GetRandomStationAsync();
            var result = await simClient.SendEventAsync(new StationEvent { EventTimeInSeconds = 3, EventType = "Fire" , StationName = stationName });
            return result;
        }

        public async Task<(bool IsSuccess, string Message)> SendLandAirplaneAsync()
        {
            var plane = await airplaneGenerator.CreateRandomPlaneAsync();
            var result = await simClient.SendLandingAsync(plane);
            return result;
        }

        public void Start()
        {
            eventTimer?.Start();
        }

        public void Stop()
        {
            eventTimer?.Stop();
        }

        private async void EventTimer_Elapsed(RandomTimer sender, RandomElapsedEventArgs e)
        {
            var chosenEvent = await rndSimEvent.GetRandomEventAsync();
            Airplane plane = null;
            string stationName = null;
            (bool IsSuccess, string message) result;
            switch (chosenEvent)
            {
                case SimEvents.Land:
                    plane = await airplaneGenerator.CreateRandomPlaneAsync();
                    result = await simClient.SendLandingAsync(plane);
                    break;
                case SimEvents.Takeoff:
                    plane = await airplaneGenerator.CreateRandomPlaneAsync();
                    result = await simClient.SendDepartureAsync(plane);
                    break;
                case SimEvents.Fire:
                    stationName = await rndSimEvent.GetRandomStationAsync();
                    result = await simClient.SendEventAsync(new StationEvent { EventTimeInSeconds = 3, EventType = "Fire", StationName = stationName } );
                    break;
                case SimEvents.Cracks:
                    stationName = await rndSimEvent.GetRandomStationAsync();
                    result = await simClient.SendEventAsync(new StationEvent { EventTimeInSeconds = 3, EventType = "Cracks", StationName = stationName });
                    break;               
                default:
                    result = (false,string.Empty);
                    break;
            }

            if (result.IsSuccess)
            {
                if (chosenEvent == SimEvents.Fire || chosenEvent == SimEvents.Cracks)
                    logger.LogInformation($"Event of type: {Enum.GetName(typeof(SimEvents), chosenEvent)} happend on station {stationName}.");
                else
                    logger.LogInformation($"Event of type: {Enum.GetName(typeof(SimEvents), chosenEvent)} happend with the plane {plane}.");
            }
            else
            {
                if (chosenEvent == SimEvents.Fire || chosenEvent == SimEvents.Cracks)
                    logger.LogInformation($"Event of type: {Enum.GetName(typeof(SimEvents), chosenEvent)} couldn't happen on track number {stationName}.");
                else
                    logger.LogInformation($"Event of type: {Enum.GetName(typeof(SimEvents), chosenEvent)} couldn't happen with the plane {plane}.");
            }
        }
    }
}
