using AirportSim.Domain.Models;

namespace AirportSim.Application.Interfaces
{
    public interface IControlTower
    {
        bool TryLand(Airplane plane);
        bool TryDeparture(Airplane plane);
    }
}
