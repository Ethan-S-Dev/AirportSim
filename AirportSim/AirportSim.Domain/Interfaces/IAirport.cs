using AirportSim.Domain.Models;
using System;
using System.Collections.Generic;

namespace AirportSim.Domain.Interfaces
{
    public interface IAirport
    {
        IList<Station> LandingStations { get; }
        IList<Station> DepartureStations { get; }
        IList<Station> EventableStation { get; }

        event StationEventHandler StationEventStarted;
        event StationEventHandler StationEventEnded;

        bool TryAddAirplan(Airplane plane);
        bool TryGetStation(string stationName, out Station station);
        void RemoveAirplane(Guid id);
    }
}
