using System;

namespace AirportSim.Api.Contracts
{
    public class StationEventRequest
    {
        public string Name { get; set; }
        public TimeSpan Time { get; set; }
    }
}
