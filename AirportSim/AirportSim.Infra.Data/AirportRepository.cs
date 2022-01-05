using AirportSim.Domain.Interfaces;
using AirportSim.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportSim.Infra.Data
{
    public class AirportRepository : IAirportRepository
    {
        private readonly AirportContext airportContext;

        public AirportRepository(AirportContext airportContext)
        {
            this.airportContext = airportContext;
        }

        public Task AddPlaneAsync(Airplane plane)
        {
            throw new NotImplementedException();
        }

        public Task AddStationEventAsync(Station stationName, Guid eventId, StationEvents fire, TimeSpan time)
        {
            throw new NotImplementedException();
        }

        public (IEnumerable<Station>, IEnumerable<Airplane>) LoadStationsAndPlanes()
        {
            var stations = airportContext.Stations
                .Select(se => new Station(se.WaitTime, se.Name, se.DisplayName, se.IsEventable, se.IsLandable, se.IsDepartable))
                .ToDictionary(s=>s.Name);

            foreach (var station in stations.Values)
            {
                var stationEntity = airportContext.Stations.Find(station.Name);
                foreach (var landingStationName in stationEntity.LandStationNames)
                {
                    var landingStation = stations[landingStationName];
                    station.LandStations.Add(landingStation);
                }

                foreach (var departureStationName in stationEntity.DepartureStations)
                {
                    var departureStation = stations[departureStationName];
                    station.DepartureStations.Add(departureStation);
                }
            }


            var planes = airportContext.Airplanes
                .Select(x => new Airplane()
                {
                    Id = x.Id,
                    Type = x.Type
                }).ToList();

            // start current events and planes

            return (stations.Values, planes);
        }

        public Task MovePlaneToStationAsync(Airplane sender, Station station)
        {
            throw new NotImplementedException();
        }

        public Task RemovePlaneAsync(Airplane toRemoved)
        {
            throw new NotImplementedException();
        }

        public Task RemoveStationEventAsync(Station sender, Guid eventId)
        {
            throw new NotImplementedException();
        }

        public Task StartStationEventAsync(Station sender, Guid eventId)
        {
            throw new NotImplementedException();
        }
    }
}
