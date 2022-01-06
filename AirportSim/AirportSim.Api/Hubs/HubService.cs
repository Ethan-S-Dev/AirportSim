using AirportSim.Api.Interfaces;
using AirportSim.Domain.Dtos;
using AirportSim.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AirportSim.Api.Hubs
{
    public class HubService : IHubService
    {
        private readonly IHubContext<ControlTowerHub,IControlTowerClient> hubContext;
        public HubService(IHubContext<ControlTowerHub, IControlTowerClient> hubContext)
        {
            this.hubContext = hubContext;
        }

        public  Task SendAiplaneRemovedAsync(AirplaneDto dto) => hubContext.Clients.All.RemoveAirplane(dto);
        public Task SendAirplaneEnteredAsync(AirplaneDto dto) => hubContext.Clients.All.AddAirplane(dto); 
        public  Task SendAirplaneLandedAsync(AirplaneDto dto) => hubContext.Clients.All.StartLanding(dto);
        public Task SendAirplaneMovedAsync(AirplaneDto dto) => hubContext.Clients.All.MoveStation(dto);
        public Task SendEventEnteredAsync(EventDto dto) => hubContext.Clients.All.AddEvent(dto);
        public Task SendEventRemovedAsync(EventDto dto) => hubContext.Clients.All.RemoveEvent(dto);
        public Task SendEventStartedAsync(EventDto dto) => hubContext.Clients.All.StartEvent(dto);
    }
}
