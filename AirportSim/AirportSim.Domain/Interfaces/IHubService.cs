using AirportSim.Domain.Dtos;
using System.Threading.Tasks;

namespace AirportSim.Domain.Interfaces
{
    public interface IHubService
    {
        Task SendAirplaneEnteredAsync(AirplaneDto dto);
        Task SendEventEnteredAsync(EventDto dto);
        Task SendAiplaneRemovedAsync(AirplaneDto dto);
        Task SendAirplaneLandedAsync(AirplaneDto dto);
        Task SendAirplaneMovedAsync(AirplaneDto dto);
        Task SendEventRemovedAsync(EventDto dto);
        Task SendEventStartedAsync(EventDto dto);
    }
}
