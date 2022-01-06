using AirportSim.Domain.Dtos;
using AirportSim.Domain.Models;
using System;
using System.Threading.Tasks;

namespace AirportSim.Application.Interfaces
{
    public interface IControlTower
    {
        Task LoadAirportStateAsync();
        Task<(bool IsSuccess, string Message)> TryLandAsync(Airplane plane);
        Task<(bool IsSuccess, string Message)> TryDepartureAsync(Airplane plane);
        Task<(bool IsSuccess, string Message)> TryStartEventAsync(string eventType, string stationName, TimeSpan timeSpan);
        Task<AirportDto> GetAirportStateAsync();
    }
}
