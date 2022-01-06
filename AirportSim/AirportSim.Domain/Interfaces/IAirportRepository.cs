using AirportSim.Domain.Dtos;
using AirportSim.Domain.Models;
using System;
using System.Threading.Tasks;

namespace AirportSim.Domain.Interfaces
{
    public interface IAirportRepository
    {
        Task<AirplaneDto> AddPlaneAsync(Airplane plane, Path landing);
        Task<EventDto> AddStationEventAsync(Station stationName,Guid eventId, StationEvents fire, TimeSpan time);
        Task<AirplaneDto> MovePlaneStationsAsync(Airplane sender, Station priviewsStation, Station nextStation);
        Task<AirplaneDto> RemovePlaneFromStationAsync(Airplane sender, Station priviewsStation);
        Task<AirplaneDto> EnterPlaneToStationAsync(Airplane sender, Station nextStation);
        Task<EventDto> StartStationEventAsync(Station sender, Guid eventId);
        Task<EventDto> RemoveStationEventAsync(Station sender, Guid eventId);
        Task<IAirport> CreateAirportWithStateAsync();
        Task<AirportDto> GetAirportStateAsync();

    }
}
