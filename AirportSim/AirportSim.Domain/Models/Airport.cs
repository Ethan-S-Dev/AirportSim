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
        private readonly IDictionary<string,IStation> stations;
        private readonly ConcurrentDictionary<Guid, IAirplane> airplanes;
        private readonly IStation[] departure;
        private readonly IStation[] landing;
        private readonly IStation[] eventable;

        public Airport(IEnumerable<IStation> stations, IEnumerable<IAirplane> currentPlanes)
        {
            airplanes = new ConcurrentDictionary<Guid, IAirplane>(currentPlanes.ToDictionary(p=>p.Id));
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
        private Task StationEventStartedHandler(IStation sender, StationEventArgs args) => StationEventStarted?.Invoke(sender, args);

        private Task StationEventEndedHandler(IStation sender, StationEventArgs args) => StationEventEnded?.Invoke(sender, args);

        public bool TryAddAirplan(IAirplane plane) => airplanes.TryAdd(plane.Id, plane);

        public bool TryGetStation(string stationName, out IStation station) => stations.TryGetValue(stationName, out station);

        public void RemoveAirplane(Guid id) => airplanes.TryRemove(id, out _);

        public bool CanLand()
        {
            var wating = airplanes.Values.Where(p => p.IsOutside && p.Objective == Objectives.Landing).Count();
            if (wating > 0)
                return false;
            var first = landing.Where(s=>s.AvilableEnteries == 1).FirstOrDefault();
            var first2 = landing.Where(s => s.AvilableEnteries == 0).FirstOrDefault();
            if (first == null)
            {
                if (first2.LandStations.Where(s => s.AvilableEnteries == 1).Any())
                    return true;
                return false;
            }
            if (first.LandStations.Where(s=>s.AvilableEnteries == 1).Any())
                return true;
            return false;
        }

        public bool CanDeparture()
        {
            var wating = airplanes.Values.Where(p => p.IsOutside && p.Objective == Objectives.Departing).Count();
            if (wating > 0)
                return false;

            var free = departure.Where(s => s.AvilableEnteries == 1).Count();
            var first = departure.FirstOrDefault();

            if (free > 1)
               return true;
            
            if(free == 0)
                return false;

            return first.DepartureStations.Where(s => s.AvilableEnteries == 1).Any();

        }

        IList<IStation> IAirport.LandingStations => landing;
        IList<IStation> IAirport.DepartureStations => departure;
        IList<IStation> IAirport.EventableStation => eventable;

        public event StationEventHandler StationEventStarted;
        public event StationEventHandler StationEventEnded;
    }
}
