using AirportSim.Simulator.Application.Interfaces;
using AirportSim.Simulator.Domain.Interfaces;
using AirportSim.Simulator.Domain.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Timers;

namespace AirportSim.Simulator.Application.Services
{
    public class Simulator : ISimulator
    {
        private readonly IAirportSimClient simClient;
        private readonly IRandomSimEvents rndSimEvent;
        private readonly IAirplaneGenerator airplaneGenerator;
        private readonly ILogger<Simulator> logger;
        private Timer eventTimer;

        public Simulator(
            IAirportSimClient simClient,
            IRandomSimEvents rndSimEvent,
            IAirplaneGenerator airplaneGenerator ,
            ILogger<Simulator> logger)
        {
            this.simClient = simClient;
            this.rndSimEvent = rndSimEvent;
            this.airplaneGenerator = airplaneGenerator;
            this.logger = logger;
        }

        public void Init()
        {
            if (eventTimer == null)
            {
                eventTimer = new(10000);
                eventTimer.AutoReset = true;
                eventTimer.Elapsed += EventTimer_Elapsed;
            }
        }

        private async void EventTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var chosenEvent = await rndSimEvent.GetRandomEventAsync();
            Airplane plane = null;
            int trackNumber = -1;
            bool result;
            switch (chosenEvent)
            {
                case SimEvents.Land:
                    plane = await airplaneGenerator.CreateRandomPlaneAsync();
                    result = await simClient.SendLandingAsync(plane);
                    break;
                case SimEvents.Takeoff:
                    plane = await airplaneGenerator.CreateRandomPlaneAsync();
                    result = await simClient.SendTackoffAsync(plane);
                    break;
                case SimEvents.Fire:
                    trackNumber = await rndSimEvent.GetRandomTrackAsync();
                    result = await simClient.SendFireAsync(trackNumber);
                    break;
                case SimEvents.Crack:
                    trackNumber = await rndSimEvent.GetRandomTrackAsync();
                    result = await simClient.SendCrackAsync(trackNumber);
                    break;
                case SimEvents.EmergencyLanding:
                    plane = await airplaneGenerator.CreateRandomPlaneAsync();
                    result = await simClient.SendEmergencyLandingAsync(plane);
                    break;
                default:
                    result = false;
                    break;
            }

            if (result)
            {
                if (chosenEvent == SimEvents.Fire || chosenEvent == SimEvents.Crack)
                    logger.LogInformation($"Event of type: {Enum.GetName(typeof(SimEvents), chosenEvent)} happend on track number {trackNumber}.");
                else
                    logger.LogInformation($"Event of type: {Enum.GetName(typeof(SimEvents), chosenEvent)} happend with the plane {plane}.");
            }
            else
            {
                if (chosenEvent == SimEvents.Fire || chosenEvent == SimEvents.Crack)
                    logger.LogInformation($"Event of type: {Enum.GetName(typeof(SimEvents), chosenEvent)} couldn't happen on track number {trackNumber}.");
                else
                    logger.LogInformation($"Event of type: {Enum.GetName(typeof(SimEvents), chosenEvent)} couldn't happen with the plane {plane}.");
            }
        }
    }
}
