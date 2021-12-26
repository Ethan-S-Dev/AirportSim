using AirportSim.Simulator.Domain.Models;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Domain.Interfaces
{
    public interface IAirportSimClient
    {
        Task<bool> SendLandingAsync(Airplane airplane);
        Task<bool> SendTackoffAsync(Airplane airplane);
        Task<bool> SendFireAsync(int trackNumber);
        Task<bool> SendCrackAsync(int trackNumber);
        Task<bool> SendEmergencyLandingAsync(Airplane airplane);
        Task<int[]> GetTrackIndexesAsync();

    }
}
