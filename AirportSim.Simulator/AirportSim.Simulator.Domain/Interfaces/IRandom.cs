using System.Threading.Tasks;

namespace AirportSim.Simulator.Domain.Interfaces
{
    public interface IRandom
    {
        Task<int> GetIntegerAsync(int min,int max);
        Task<bool> GetBooleanAsync();
    }
}
