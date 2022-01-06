using AirportSim.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirportSim.Domain.Models
{
    public delegate Task StationEventHandler(Station sender, StationEventArgs args);
    public class StationEventArgs : EventArgs
    {
        public Guid EventId { get; set; }
        public Station Station { get; set;}
        public DateTimeOffset Time { get; set; }
        public StationEvents EventType { get; set; }
        public TimeSpan EventTime { get; set; }

    }
    public class Station : IStation
    {
        public event StationEventHandler StationEventStarted;
        public event StationEventHandler StationEventEnded;
        public Station(TimeSpan waitTime, string name, string displayName,bool isEventable, bool isLandable, bool isDepartable)
        {
            Lock = new SemaphoreSlim(1);
            EventLock = new SemaphoreSlim(1);

            LandStations = new List<Station>();
            DepartureStations = new List<Station>();
            WaitTime = waitTime;
            Name = name;
            DisplayName = displayName;

            IsEventable = isEventable;
            IsDepartable = isDepartable;
            IsLandable = isLandable;
        }
        public List<Station> LandStations { get; }
        public List<Station> DepartureStations { get; }
        public SemaphoreSlim Lock { get; }
        public SemaphoreSlim EventLock { get; }
        public TimeSpan WaitTime { get; }
        public string Name { get; }
        public string DisplayName { get; }
        public async Task StartStationEventAsync(Guid eventId,StationEvents eventType,TimeSpan eventTime)
        {
            await EventLock.WaitAsync();
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
            EventLock.Release();
        }

        async Task IStation.ContinueStationEventAsync(Guid eventId, StationEvents eventType, TimeSpan eventTime)
        {
            await EventLock.WaitAsync();
            await Task.Delay(eventTime);
            StationEventEnded?.Invoke(this, new StationEventArgs
            {
                EventId = eventId,
                EventTime = eventTime,
                EventType = eventType,
                Station = this,
                Time = DateTimeOffset.UtcNow
            });
            EventLock.Release();
        }

        public bool IsEventable { get; }
        public bool IsLandable { get; }
        public bool IsDepartable { get; }
    }
}
