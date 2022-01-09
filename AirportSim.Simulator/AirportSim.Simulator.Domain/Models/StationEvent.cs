namespace AirportSim.Simulator.Domain.Models
{
    public class StationEvent
    {
        public string EventType { get; set; }
        public string StationName { get; set; }
        public double EventTimeInSeconds { get; set; }
    }
}
