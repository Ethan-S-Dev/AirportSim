using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirportSim.Domain.Models
{
    public class Airport
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

        public IList<Station> LandingStations => landing;
        public IDictionary<string, Station> Stations => stations;
        public IList<Station> DepartureStations => departure;
        public ConcurrentDictionary<Guid, Airplane> Airplanes => airplanes;

        public IList<string> EventableStationNames => eventable.Select(s => s.Name).ToList();

        public event StationEventHandler StationEventStarted;
        public event StationEventHandler StationEventEnded;
    }
}
