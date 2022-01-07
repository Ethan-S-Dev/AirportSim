using AirportSim.Domain.Interfaces;
using System;

namespace AirportSim.Domain.Dtos
{
    public class AirplaneDto : IDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public bool IsOutside { get; set; }
        public string Objective { get; set; }
        public string CurrentStationName { get; set; }
        public DateTimeOffset EnteredAt { get; set; }
    }
}
