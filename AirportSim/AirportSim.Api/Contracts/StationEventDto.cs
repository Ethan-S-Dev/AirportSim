using System;

namespace AirportSim.Api.Contracts
{
    public class StationEventDto
    {
        public string Name { get; set; }
        public TimeSpan Time { get; set; }
    }
}
