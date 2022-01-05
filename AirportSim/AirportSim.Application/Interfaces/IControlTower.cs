using AirportSim.Domain.Models;
using System;
using System.Threading.Tasks;

namespace AirportSim.Application.Interfaces
{
    public interface IControlTower
    {
        void LoadAirportState();
        Task<bool> TryLandAsync(Airplane plane);
        Task<bool> TryDepartureAsync(Airplane plane);
        Task<bool> TryStartFireAsync(string stationName, TimeSpan time);
        Task<bool> TryStartCracksAsync(string stationName, TimeSpan time);
    }
}
