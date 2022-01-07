using AirportSim.Domain.Interfaces;
using System;

namespace AirportSim.Domain.Dtos
{
    public class StationDto : IDto
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public double WaitTimeInSeconds { get; set; }
        public Guid? CurrentPlaneId { get; set; }
        public bool IsEventable { get; set; }
    }
}
