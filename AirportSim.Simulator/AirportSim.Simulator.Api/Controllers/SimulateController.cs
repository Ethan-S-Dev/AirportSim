using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimulateController : ControllerBase
    {

        [HttpGet("Land")]
        public Task<IActionResult> Land()
        {
            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpGet("Takeoff")]
        public Task<IActionResult> TakeOff()
        {
            return Task.FromResult<IActionResult>(Ok());
        }
        
        [HttpGet("Obstacle/Fire/{trackNum}")]
        public Task<IActionResult> Fire(int trackNum)
        {
            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpGet("Obstacle/Crack/{trackNum}")]
        public Task<IActionResult> Crack(int trackNum)
        {
            return Task.FromResult<IActionResult>(Ok());
        }

        [HttpGet("Obstacle/EmergencyLanding")]
        public Task<IActionResult> EmergencyLanding()
        {
            return Task.FromResult<IActionResult>(Ok());
        }
    }
}
