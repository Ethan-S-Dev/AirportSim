using AirportSim.Simulator.Domain.Interfaces;
using AirportSim.Simulator.Domain.Models;
using System;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Domain
{
    public class RandomSimEvents : IRandomSimEvents
    {
        private readonly IAirportSimClient airportSimClient;
        private readonly IRandom random;

        public RandomSimEvents(IAirportSimClient airportSimClient,IRandom random)
        {
            this.airportSimClient = airportSimClient;
            this.random = random;
        }

        public async Task<SimEvents> GetRandomEventAsync()
        {
            var numberOfEvents = Enum.GetValues(typeof(SimEvents)).Length;
            var eventNumber = await random.GetIntegerAsync(0, numberOfEvents);
            if(!Enum.TryParse<SimEvents>(eventNumber.ToString(), out var result))
                throw new Exception("Number generated is no in range of the event enum");
            return result;
        }

        public async Task<string> GetRandomStationAsync()
        {
            var trackIndexes = await airportSimClient.GetStationNamesAsync();
            var indexOfName = await random.GetIntegerAsync(0, trackIndexes.Length);
            return trackIndexes[indexOfName];
        }
    }
}
