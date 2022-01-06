using System.Collections.Generic;

namespace AirportSim.Domain.Dtos
{
    public class AirportDto
    {
        public IEnumerable<AirplaneDto> Airplanes { get; set; }
        public IEnumerable<EventDto> Events { get; set; }
        public IEnumerable<StationDto> Stations { get; set; }
    }
}
