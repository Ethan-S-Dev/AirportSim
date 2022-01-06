using System.ComponentModel.DataAnnotations;

namespace AirportSim.Api.Contracts
{
    public class StationEventRequest
    {
        [Required]
        public string EventType { get; set; }
        [Required]
        public string StationName { get; set; }
        [Required]
        public double EventTimeInSeconds { get; set; }
    }
}
