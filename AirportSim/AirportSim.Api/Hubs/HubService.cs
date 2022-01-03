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
        public Task SendHello()
        {
            return hubContext.Clients.All.SendAsync("hello", "message");
        }
    }
}
