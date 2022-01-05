using AirportSim.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirportSim.Domain.Interfaces
{
    public interface IAirportRepository
    {
        Task AddPlaneAsync(Airplane plane);
        Task AddStationEventAsync(Station stationName,Guid eventId, StationEvents fire, TimeSpan time);
        Task RemovePlaneAsync(Airplane toRemoved);
        Task MovePlaneToStationAsync(Airplane sender, Station station);
        Task StartStationEventAsync(Station sender, Guid eventId);
        Task RemoveStationEventAsync(Station sender, Guid eventId);
        (IEnumerable<Station>, IEnumerable<Airplane>) LoadStationsAndPlanes();
    }
}
