using AirportSim.Domain.Dtos;
using AirportSim.Domain.Models;
using System;
using System.Threading.Tasks;

namespace AirportSim.Domain.Interfaces
{
    public interface IAirportRepository
    {
        Task<AirplaneDto> AddPlaneAsync(IAirplane plane, Objectives landing);
        Task<EventDto> AddStationEventAsync(IStation stationName,Guid eventId, string eventType, TimeSpan time);
        Task<AirplaneDto> MovePlaneStationsAsync(IAirplane sender, IStation priviewsStation, IStation nextStation);
        Task<AirplaneDto> RemovePlaneFromStationAsync(IAirplane sender, IStation priviewsStation);
        Task<AirplaneDto> EnterPlaneToStationAsync(IAirplane sender, IStation nextStation);
        Task<EventDto> StartStationEventAsync(IStation sender, Guid eventId);
        Task<EventDto> RemoveStationEventAsync(IStation sender, Guid eventId);
        Task<IAirport> CreateAirportWithStateAsync();
        Task<AirportDto> GetAirportStateAsync();

    }
}
