using System;

namespace AirportSim.Api.Contracts
{
    public class AirplaneRequest
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
    }
}
