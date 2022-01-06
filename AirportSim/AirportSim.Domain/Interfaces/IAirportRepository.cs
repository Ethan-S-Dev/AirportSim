using AirportSim.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirportSim.Domain.Interfaces
{
    public interface IAirportRepository
    {
        Task AddPlaneAsync(Airplane plane, Path landing);
        Task AddStationEventAsync(Station stationName,Guid eventId, StationEvents fire, TimeSpan time);
        Task MovePlaneStationsAsync(Airplane sender, Station priviewsStation, Station nextStation);
        Task RemovePlaneFromStationAsync(Airplane sender, Station priviewsStation);
        Task EnterPlaneToStationAsync(Airplane sender, Station nextStation);
        Task StartStationEventAsync(Station sender, Guid eventId);
        Task RemoveStationEventAsync(Station sender, Guid eventId);
        Task<IAirport> CreateAirportWithStatreAsync();
    }
}
