using AirportSim.Domain.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirportSim.Domain.Models
{
    public class Airport : IAirport
    {
        private readonly IDictionary<string,Station> stations;
        private readonly ConcurrentDictionary<Guid, Airplane> airplanes;
        private readonly Station[] departure;
        private readonly Station[] landing;
        private readonly Station[] eventable;

        public Airport(IEnumerable<Station> stations, IEnumerable<Airplane> currentPlanes)
        {
            airplanes = new ConcurrentDictionary<Guid, Airplane>(currentPlanes.ToDictionary(p=>p.Id));
            this.stations = stations.ToDictionary(s=>s.Name);

            eventable = stations.Where(station => station.IsEventable).ToArray();

            foreach (var station in eventable)
            {
                station.StationEventStarted += StationEventStartedHandler;
                station.StationEventEnded += StationEventEndedHandler;
            }

            landing = stations.Where(station => station.IsLandable).ToArray();
            departure = stations.Where(station => station.IsDepartable).ToArray();
        }    
        private Task StationEventStartedHandler(Station sender, StationEventArgs args) => StationEventStarted?.Invoke(sender, args);

        private Task StationEventEndedHandler(Station sender, StationEventArgs args) => StationEventEnded?.Invoke(sender, args);

        public bool TryAddAirplan(Guid id, Airplane plane) => airplanes.TryAdd(id, plane);

        public bool TryGetStation(string stationName, out Station station) => stations.TryGetValue(stationName, out station);

        public void RemoveAirplane(Guid id) => airplanes.TryRemove(id, out _);

        IList<Station> IAirport.LandingStations => landing;
        IList<Station> IAirport.DepartureStations => departure;
        IList<Station> IAirport.EventableStation => eventable;

        public event StationEventHandler StationEventStarted;
        public event StationEventHandler StationEventEnded;
    }
}
