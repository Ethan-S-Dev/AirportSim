using AirportSim.Domain.Dtos;
using System;
using System.Threading.Tasks;

namespace AirportSim.Application.Interfaces
{
    public interface IControlTower
    {
        Task LoadAirportStateAsync();
        Task<(bool IsSuccess, string Message)> TryLandAsync(Guid id, string type);
        Task<(bool IsSuccess, string Message)> TryDepartureAsync(Guid id, string type);
        Task<(bool IsSuccess, string Message)> TryStartEventAsync(string eventType, string stationName, TimeSpan timeSpan);
        Task<AirportDto> GetAirportStateAsync();
    }
}
