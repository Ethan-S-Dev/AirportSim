using AirportSim.Simulator.Domain.Models;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Domain.Interfaces
{
    public interface IRandomSimEvents
    {
        Task<SimEvents> GetRandomEventAsync();
        Task<string> GetRandomStationAsync();
    }
}
