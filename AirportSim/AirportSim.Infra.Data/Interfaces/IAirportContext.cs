using AirportSim.Infra.Data.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AirportSim.Infra.Data.Interfaces
{
    public interface IAirportContext
    {
        IEnumerable<StationEntity> Stations { get; }
        IEnumerable<AirplaneEntity> Airplanes { get; }
        IEnumerable<StationEventEntity> Events { get; }

        Task AddPlaneAsync(AirplaneEntity airplaneEntity);
        Task SaveChangesAsync();
        Task AddEventAsync(StationEventEntity stationEventEntity);
        Task<AirplaneEntity> FindAirplaneAsync(Guid id);
        Task<StationEntity> FindStationAsync(string name);
        Task<StationEventEntity> FindEventAsync(Guid eventId);
        void RemoveAirplane(AirplaneEntity planeEntity);
        void RemoveEvent(StationEventEntity eventEntity);
       
    }
}
