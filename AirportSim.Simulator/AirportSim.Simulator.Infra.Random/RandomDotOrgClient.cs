using AirportSim.Simulator.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Infra.Random
{
    public class RandomDotOrgClient : IRandom
    {
        public Task<bool> GetBooleanAsync()
        {
            throw new NotImplementedException();
        }

        public Task<int> GetIntegerAsync(int min, int max)
        {
            throw new NotImplementedException();
        }
    }
}
