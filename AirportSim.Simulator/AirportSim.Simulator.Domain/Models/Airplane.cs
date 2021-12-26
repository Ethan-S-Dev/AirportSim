using System;

namespace AirportSim.Simulator.Domain.Models
{
    public class Airplane
    {
        public Guid Id { get; set; }
        public string Type { get; set; }

        public override string ToString()
        {
            return $"PlaneId: {Id},Type: {Type}";
        }
    }
}
