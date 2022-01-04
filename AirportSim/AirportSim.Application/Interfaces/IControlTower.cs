using AirportSim.Domain.Models;
using System;
using System.Threading.Tasks;

namespace AirportSim.Application.Interfaces
{
    public interface IControlTower
    {
        bool TryLand(Airplane plane);
        bool TryDeparture(Airplane plane);
        bool TryStartFire(string stationName, TimeSpan time)
        bool TryStartCracks(string stationName, TimeSpan time)
    }
}
