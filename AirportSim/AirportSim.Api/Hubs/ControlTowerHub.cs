﻿using AirportSim.Api.Interfaces;
using AirportSim.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AirportSim.Api.Hubs
{
    public class ControlTowerHub : Hub<IControlTowerClient>
    {
        private readonly IControlTower controlTower;

        public ControlTowerHub(IControlTower controlTower)
        {
            this.controlTower = controlTower;
        }
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();

            var state = await controlTower.GetAirportStateAsync();
            await Clients.Caller.InitializeAirport(state);
        }
    }
}
