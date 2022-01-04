using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirportSim.Domain.Models
{
    public delegate Task StationEventHandler(Station sender, StationEventArgs args);
    public class StationEventArgs : EventArgs
    {
        public Station Station { get; set;}
        public DateTimeOffset Time { get; set; }
        public StationEvents EventType { get; set; }
        public TimeSpan EventTime { get; set; }

    }
    public class Station 
    {
        public event StationEventHandler StationEventStarted;
        public event StationEventHandler StationEventEnded;
        public Station(string name, string displayName) : this(new TimeSpan(0, 0, 5), name, displayName)
        { }
        public Station(TimeSpan waitTime, string name, string displayName)
        {
            Lock = new SemaphoreSlim(1);
            EventLock = new SemaphoreSlim(1);

            LandStations = new List<Station>();
            DepartureStations = new List<Station>();
            WaitTime = waitTime;
            Name = name;
            DisplayName = displayName;
        }
        public List<Station> LandStations { get; set; }
        public List<Station> DepartureStations { get; set; }
        public SemaphoreSlim Lock { get; }
        public SemaphoreSlim EventLock { get; }
        public TimeSpan WaitTime { get; }
        public string Name { get; }
        public string DisplayName { get; }
        public async Task DoStationEventAsync(StationEvents eventType,TimeSpan eventTime)
        {
            await EventLock.WaitAsync();
            StationEventStarted?.Invoke(this,new StationEventArgs { EventTime = eventTime,EventType = eventType,Station = this,Time = DateTimeOffset.UtcNow });
            await Task.Delay(eventTime);
            StationEventEnded?.Invoke(this, new StationEventArgs { EventTime = eventTime, EventType = eventType, Station = this, Time = DateTimeOffset.UtcNow });
            EventLock.Release();
        } 
    }
}
