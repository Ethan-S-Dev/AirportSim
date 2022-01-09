using AirportSim.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirportSim.Domain.Interfaces
{
    public interface IStation
    {
        IList<IStation> LandStations { get; }
        IList<IStation> DepartureStations { get; }

        TimeSpan WaitTime { get; }
        string Name { get; }
        string DisplayName { get; }
        public event StationEventHandler StationEventStarted;
        public event StationEventHandler StationEventEnded;
        Task StartStationEventAsync(Guid eventId, string eventType, TimeSpan eventTime);

        Task LockStationAsync(CancellationToken cancellationToken);
        void ReleaseStation();

        Task LockEventsAsync();
        void ReleaseEvents();

        int AvilableEnteries { get; }

        bool IsEventable { get; }
        bool IsLandable { get; }
        bool IsDepartable { get; }
    }
}
