using AirportSim.Api.Contracts;
using AirportSim.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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
        public IActionResult LandAirplane(AirplaneDto airplane)
        {
            if (controlTower.TryLand(new Domain.Models.Airplane() { Id = airplane.Id, Type = airplane.Type }))
                return Ok();
            return BadRequest();
        }

        [HttpPost("departure")]
        public IActionResult DepartureAirplane(AirplaneDto airplane)
        {
            if (controlTower.TryDeparture(new Domain.Models.Airplane() { Id = airplane.Id, Type = airplane.Type }))
                return Ok();
            return BadRequest();
        }
    }
}
