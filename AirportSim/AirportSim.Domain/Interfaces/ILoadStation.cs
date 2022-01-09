using System;
using System.Threading.Tasks;

namespace AirportSim.Domain.Interfaces
{
    public interface ILoadStation
    {
        Task ContinueStationEventAsync(Guid eventId, string eventType, TimeSpan eventTime);
    }
}
