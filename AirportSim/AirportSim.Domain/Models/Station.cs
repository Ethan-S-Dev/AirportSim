using AirportSim.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirportSim.Domain.Models
{
    public delegate Task StationEventHandler(IStation sender, StationEventArgs args);
    public class StationEventArgs : EventArgs
    {
        public Guid EventId { get; set; }
        public IStation Station { get; set;}
        public DateTimeOffset Time { get; set; }
        public string EventType { get; set; }
        public TimeSpan EventTime { get; set; }

    }
    public class Station : IStation, ILoadStation
    {
        public event StationEventHandler StationEventStarted;
        public event StationEventHandler StationEventEnded;
        public Station(TimeSpan waitTime, string name, string displayName,bool isEventable, bool isLandable, bool isDepartable)
        {
            stationLock = new SemaphoreSlim(1);
            eventLock = new SemaphoreSlim(1);

            LandStations = new List<IStation>();
            DepartureStations = new List<IStation>();
            WaitTime = waitTime;
            Name = name;
            DisplayName = displayName;

            IsEventable = isEventable;
            IsDepartable = isDepartable;
            IsLandable = isLandable;
        }
        public IList<IStation> LandStations { get; }
        public IList<IStation> DepartureStations { get; }
        public TimeSpan WaitTime { get; }
        public string Name { get; }
        public string DisplayName { get; }
        public bool IsEventable { get; }
        public bool IsLandable { get; }
        public bool IsDepartable { get; }

        public int AvilableEnteries => stationLock.CurrentCount;

        public async Task StartStationEventAsync(Guid eventId,string eventType,TimeSpan eventTime)
        {
            await LockEventsAsync();
            StationEventStarted?.Invoke(this,new StationEventArgs 
            {
                EventId = eventId,
                EventTime = eventTime,
                EventType = eventType,
                Station = this,
                Time = DateTimeOffset.UtcNow 
            });
            await Task.Delay(eventTime);
            StationEventEnded?.Invoke(this, new StationEventArgs 
            {
                EventId = eventId,
                EventTime = eventTime, 
                EventType = eventType, 
                Station = this, 
                Time = DateTimeOffset.UtcNow 
            });
            ReleaseEvents();
        }
        public Task LockStationAsync(CancellationToken cancellationToken) => stationLock.WaitAsync(cancellationToken);
        public void ReleaseStation() => stationLock.Release();
        public Task LockEventsAsync() => eventLock.WaitAsync();
        public void ReleaseEvents() => eventLock.Release();

        async Task ILoadStation.ContinueStationEventAsync(Guid eventId, string eventType, TimeSpan eventTime)
        {
            await LockEventsAsync();
            await Task.Delay(eventTime);
            StationEventEnded?.Invoke(this, new StationEventArgs
            {
                EventId = eventId,
                EventTime = eventTime,
                EventType = eventType,
                Station = this,
                Time = DateTimeOffset.UtcNow
            });
            ReleaseEvents();
        }

        private readonly SemaphoreSlim stationLock;
        private readonly SemaphoreSlim eventLock;
    }
}
