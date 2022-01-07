using AirportSim.Domain.Interfaces;
using System;

namespace AirportSim.Domain.Dtos
{
    public class EventDto : IDto
    {
        public Guid Id { get; set; }
        public string EventType { get; set; }
        public double EventTimeInSeconds { get; set; }
        public DateTimeOffset ReceivedAt { get; set; }
        public bool IsStarted { get; set; }
        public string StationName { get; set; }
    }
}
