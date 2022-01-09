using AirportSim.Simulator.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimulateController : ControllerBase
    {
        private readonly ISimulator simulator;

        public SimulateController(ISimulator simulator)
        {
            this.simulator = simulator;
        }

        [HttpGet("stop")]
        public IActionResult Stop()
        {
            simulator.Stop();
            return Ok();
        }

        [HttpGet("start")]
        public IActionResult Start()
        {
            simulator.Start();
            return Ok();
        }

        [HttpGet("land")]
        public async Task<IActionResult> Land()
        {
            var result = await simulator.SendLandAirplaneAsync();
            if(result.IsSuccess)
                return Ok(new { result.Message });
            return BadRequest(new { result.Message });
        }

        [HttpGet("departure")]
        public async Task<IActionResult> Departure()
        {
            var result = await simulator.SendDepartureAirplaneAsync();
            if (result.IsSuccess)
                return Ok(new { result.Message });
            return BadRequest(new { result.Message });
        }
        
        [HttpGet("event/fire")]
        public async Task<IActionResult> Fire()
        {
            var result = await simulator.SendFireEventAsync();
            if (result.IsSuccess)
                return Ok(new { result.Message });
            return BadRequest(new { result.Message });
        }

        [HttpGet("event/cracks")]
        public async Task<IActionResult> Cracks()
        {
            var result = await simulator.SendCracksEventAsync();
            if (result.IsSuccess)
                return Ok(new { result.Message });
            return BadRequest(new { result.Message });
        }
    }
}
