using AirportSim.Simulator.Domain.Interfaces;
using AirportSim.Simulator.Domain.Models;
using System;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Infra.AirportControl
{
    public class AirportSimClient : IAirportSimClient
    {
        public Task<string[]> GetTrackNamesAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendCrackAsync(string trackName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendEmergencyLandingAsync(Airplane airplane)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SendFireAsync(string trackName)
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
