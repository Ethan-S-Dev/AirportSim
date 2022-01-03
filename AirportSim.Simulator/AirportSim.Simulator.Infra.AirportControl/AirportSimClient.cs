using AirportSim.Simulator.Domain.Interfaces;
using AirportSim.Simulator.Domain.Models;
using System;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Infra.AirportControl
{
    public class AirportSimClient : IAirportSimClient
    {
        public Task<int[]> GetTrackIndexesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendCrackAsync(int trackNumber)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendEmergencyLandingAsync(Airplane airplane)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendFireAsync(int trackNumber)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendLandingAsync(Airplane airplane)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendTackoffAsync(Airplane airplane)
        {
            throw new NotImplementedException();
        }
    }
}
