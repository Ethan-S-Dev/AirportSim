using AirportSim.Simulator.Domain.Models;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Domain.Interfaces
{
    public interface IAirportSimClient
    {
        Task<(bool IsSuccess, string Message)> SendLandingAsync(Airplane airplane);
        Task<(bool IsSuccess, string Message)> SendDepartureAsync(Airplane airplane);
        Task<(bool IsSuccess, string Message)> SendEventAsync(StationEvent stationEvent);
        Task<string[]> GetStationNamesAsync();
    }
}
