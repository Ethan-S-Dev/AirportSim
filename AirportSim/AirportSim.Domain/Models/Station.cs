using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AirportSim.Domain.Models
{
    public class Station
    {
        public Station(string name,string displayName) : this(new TimeSpan(0, 0, 5), name, displayName)
        { }
        public Station(TimeSpan waitTime,string name, string displayName)
        {
            Semaphore = new SemaphoreSlim(1);
            LandStations = new List<Station>();
            DepartureStations = new List<Station>();
            WaitTime = waitTime;
            Name = name;
            DisplayName = displayName;
        }
        public List<Station> LandStations { get; set; }
        public List<Station> DepartureStations { get; set; }
        public SemaphoreSlim Semaphore { get; }
        public TimeSpan WaitTime { get; }
        public string Name { get; }
        public string DisplayName { get; }
    }
}
