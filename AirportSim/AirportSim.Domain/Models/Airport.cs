using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AirportSim.Domain.Models
{
    public class Airport
    {
        private readonly Dictionary<string,Station> stations;
        private readonly ConcurrentDictionary<Guid, Airplane> airplanes;
        private readonly Station[] departure;
        private readonly Station[] landing;
        private readonly Station[] eventable;

        public Airport()
        {
            airplanes = new ConcurrentDictionary<Guid, Airplane>();

            var station1 = new Station("station1","Landing 1");
            var station2 = new Station("station2", "Landing 2");
            var station3 = new Station("station3", "Landing 3");
            var station4 = new Station("station4", "Runway");
            var station5 = new Station("station5", "Transportation route 1");
            var station6 = new Station("station6", "Boarding gate 1");
            var station7 = new Station("station7", "Boarding gate 2");
            var station8 = new Station("station8", "Transportation route 2");
            var station9 = new Station("station9", "Takeoff");

            station4.StationEventStarted += StationEventStartedHandler;
            station5.StationEventStarted += StationEventStartedHandler;
            station6.StationEventStarted += StationEventStartedHandler;
            station7.StationEventStarted += StationEventStartedHandler;
            station8.StationEventStarted += StationEventStartedHandler;

            station4.StationEventEnded += StationEventEndedHandler;
            station5.StationEventEnded += StationEventEndedHandler;
            station6.StationEventEnded += StationEventEndedHandler;
            station7.StationEventEnded += StationEventEndedHandler;
            station8.StationEventEnded += StationEventEndedHandler;

            station1.LandStations.Add(station2);
            station2.LandStations.Add(station3);
            station3.LandStations.Add(station4);
            station4.LandStations.Add(station5);
            station5.LandStations.Add(station6);
            station5.LandStations.Add(station7);

            station6.DepartureStations.Add(station8);
            station7.DepartureStations.Add(station8);
            station8.DepartureStations.Add(station4);
            station4.DepartureStations.Add(station9);

            stations = new Dictionary<string, Station>();
            stations.Add("station1", station1);
            stations.Add("station2", station2);
            stations.Add("station3", station3);
            stations.Add("station4", station4);
            stations.Add("station5", station5);
            stations.Add("station6", station6);
            stations.Add("station7", station7);
            stations.Add("station8", station8);
            stations.Add("station9", station9);

            eventable = new[] { station4, station5, station6, station7, station8 };

            landing = new[] { station1 };
            departure = new[] { station6, station7 };
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
