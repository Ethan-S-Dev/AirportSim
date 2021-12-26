using AirportSim.Simulator.Domain.Interfaces;
using AirportSim.Simulator.Domain.Models;
using System;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Domain
{
    public class AirplaneGenerator : IAirplaneGenerator
    {
        private readonly IRandom random;

        public AirplaneGenerator(IRandom random)
        {
            this.random = random;
        }
        public async Task<Airplane> CreateRandomPlaneAsync()
        {
            return new Airplane()
            {
                Id = Guid.NewGuid(),
                Type = await PlaneTypes.GetRandomTypeAsync(random)
            };
        }
    }
}
