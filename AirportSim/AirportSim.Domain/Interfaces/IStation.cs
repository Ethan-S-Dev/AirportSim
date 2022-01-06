using AirportSim.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportSim.Domain.Interfaces
{
    public interface IStation
    {
        Task ContinueStationEventAsync(Guid eventId, string eventType, TimeSpan eventTime);
    }
}
