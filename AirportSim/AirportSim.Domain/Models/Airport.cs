using System.Collections.Generic;

namespace AirportSim.Domain.Models
{
    public class Airport
    {
        private readonly Dictionary<string,Station> stations;
        public Airport()
        {
            var station1 = new Station("station1","Landing 1");
            var station2 = new Station("station2", "Landing 2");
            var station3 = new Station("station3", "Landing 3");
            var station4 = new Station("station4", "Runway");
            var station5 = new Station("station5", "Transportation route 1");
            var station6 = new Station("station6", "Boarding gate 1");
            var station7 = new Station("station7", "Boarding gate 2");
            var station8 = new Station("station8", "Transportation route 2");
            var station9 = new Station("station9", "Takeoff");

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

            landing = new[] { stations["station1"] };
            departure = new[] { stations["station6"], stations["station7"] };
        }

        private readonly Station[] landing;
        public IList<Station> LandingStations
        {
            get
            {
                return landing;
            }
        }

        private readonly Station[] departure;
        public IList<Station> DepartureStations
        {
            get { return departure; }
        }
    }
}
