using AirportSim.Simulator.Domain.Models;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Domain.Interfaces
{
    public interface IAirplaneGenerator
    {
        Task<Airplane> CreateRandomPlaneAsync();
    }
}
