using AirportSim.Domain.Models;
using System;
using System.Collections.Generic;

namespace AirportSim.Domain.Interfaces
{
    public interface IAirport
    {
        IList<IStation> LandingStations { get; }
        IList<IStation> DepartureStations { get; }
        IList<IStation> EventableStation { get; }

        event StationEventHandler StationEventStarted;
        event StationEventHandler StationEventEnded;

        bool TryAddAirplan(IAirplane plane);
        bool TryGetStation(string stationName, out IStation station);
        void RemoveAirplane(Guid id);

        bool CanLand();
        bool CanDeparture();
    }
}
