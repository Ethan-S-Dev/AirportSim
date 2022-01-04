using AirportSim.Simulator.Domain.Models;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Domain.Interfaces
{
    public interface IAirportSimClient
    {
        Task<bool> SendLandingAsync(Airplane airplane);
        Task<bool> SendTackoffAsync(Airplane airplane);
        Task<bool> SendFireAsync(string trackName);
        Task<bool> SendCrackAsync(string trackName);
        Task<string[]> GetTrackNamesAsync();
    }
}
