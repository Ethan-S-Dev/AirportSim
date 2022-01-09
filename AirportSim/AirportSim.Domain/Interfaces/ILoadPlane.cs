using AirportSim.Domain.Models;
using System.Threading.Tasks;

namespace AirportSim.Domain.Interfaces
{
    public interface ILoadPlane
    {
        Task EnterStation(IStation station, Objectives path, bool entering);
    }
}
