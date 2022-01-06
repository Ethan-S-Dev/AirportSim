using AirportSim.Domain.Models;
using System.Threading.Tasks;

namespace AirportSim.Domain.Interfaces
{
    public interface IPlane
    {
        Task EnterStation(Station station, Path path, bool entering);
    }
}
