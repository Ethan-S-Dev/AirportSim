using AirportSim.Domain.Dtos;
using AirportSim.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AirportSim.Api.Hubs
{
    public class HubService : IHubService
    {
        private readonly IHubContext<ControlTowerHub> hubContext;

        public HubService(IHubContext<ControlTowerHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        public Task SendAiplaneRemovedAsync(AirplaneDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task SendAirplaneEnteredAsync(AirplaneDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task SendAirplaneLandedAsync(AirplaneDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task SendAirplaneMovedAsync(AirplaneDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task SendEventEnteredAsync(EventDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task SendEventRemovedAsync(EventDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task SendEventStartedAsync(EventDto dto)
        {
            throw new System.NotImplementedException();
        }

        public Task SendHello()
        {
            return hubContext.Clients.All.SendAsync("hello", "message");
        }
    }
}
