using AirportSim.Domain.Dtos;
using System.Threading.Tasks;

namespace AirportSim.Api.Interfaces
{
    public interface IControlTowerClient
    {
        Task RemoveAirplane(AirplaneDto dto);
        Task AddAirplane(AirplaneDto dto);
        Task StartLanding(AirplaneDto dto);
        Task MoveStation(AirplaneDto dto);
        Task AddEvent(EventDto dto);
        Task RemoveEvent(EventDto dto);
        Task StartEvent(EventDto dto);
        Task InitializeAirport(AirportDto dto);
    }
}
