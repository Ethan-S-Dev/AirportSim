using AirportSim.Simulator.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Infra.Random
{
    public class BasicRandom : IRandom
    {
        private readonly System.Random rnd;

        public BasicRandom() => rnd = new System.Random();

        public Task<bool> GetBooleanAsync() => Task.FromResult(GetBoolean());

        public Task<int> GetIntegerAsync(int min, int max) => Task.FromResult(rnd.Next(min, max));

        private bool GetBoolean()
        {
            return rnd.Next(2) switch
            {
                0 => true,
                1 => false,
                _ => throw new InvalidOperationException("The random number generated is out of bounds"),
            };
        }
    }
}
