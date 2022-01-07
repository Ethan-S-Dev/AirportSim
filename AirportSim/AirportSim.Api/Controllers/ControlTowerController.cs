using AirportSim.Api.Contracts;
using AirportSim.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace AirportSim.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControlTowerController : ControllerBase
    {
        private readonly IControlTower controlTower;

        public ControlTowerController(IControlTower controlTower)
        {
            this.controlTower = controlTower;
        }

        [HttpPost("land")]
        public async Task<IActionResult> LandAirplane(AirplaneRequest airplane)
        {
            var result = await controlTower.TryLandAsync(airplane.Id,airplane.Type);
            if (result.IsSuccess)
                return Ok(result.Message);
            return BadRequest(result.Message);
        }

        [HttpPost("departure")]
        public async Task<IActionResult> DepartureAirplane(AirplaneRequest airplane)
        {
            var result = await controlTower.TryDepartureAsync(airplane.Id,airplane.Type);
            if (result.IsSuccess)
                return Ok(result.Message);
            return BadRequest(result.Message);
        }

        [HttpPost("event")]
        public async Task<IActionResult> StartEvent(StationEventRequest stationEvent)
        {
            var result = await controlTower
                .TryStartEventAsync(
                stationEvent.EventType,
                stationEvent.StationName,
                TimeSpan.FromSeconds(stationEvent.EventTimeInSeconds));
            if (result.IsSuccess)
                return Ok(result.Message);
            return BadRequest(result.Message);
        }

    }
}
