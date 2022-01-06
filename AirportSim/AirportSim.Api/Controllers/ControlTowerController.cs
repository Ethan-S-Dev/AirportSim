using AirportSim.Api.Contracts;
using AirportSim.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
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
            if (await controlTower.TryLandAsync(new Domain.Models.Airplane() { Id = airplane.Id, Type = airplane.Type }))
                return Ok();
            return BadRequest();
        }

        [HttpPost("departure")]
        public async Task<IActionResult> DepartureAirplane(AirplaneRequest airplane)
        {
            if (await controlTower.TryDepartureAsync(new Domain.Models.Airplane() { Id = airplane.Id, Type = airplane.Type }))
                return Ok();
            return BadRequest();
        }

        [HttpPost("events/fire")]
        public async Task<IActionResult> StartFire(StationEventRequest stationEvent)
        {
            if (await controlTower.TryStartFireAsync(stationEvent.Name, stationEvent.Time))
                return Ok();
            return BadRequest();
        }

        [HttpPost("events/cracks")]
        public async Task<IActionResult> StartCracks(StationEventRequest stationEvent)
        {
            if (await controlTower.TryStartCracksAsync(stationEvent.Name, stationEvent.Time))
                return Ok();
            return BadRequest();
        }
    }
}
