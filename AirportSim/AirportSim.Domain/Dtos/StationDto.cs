using System;

namespace AirportSim.Domain.Dtos
{
    public class StationDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public double WaitTimeInSeconds { get; set; }
        public Guid CurrentPlaneId { get; set; }
        public bool IsEventable { get; set; }
    }
}
