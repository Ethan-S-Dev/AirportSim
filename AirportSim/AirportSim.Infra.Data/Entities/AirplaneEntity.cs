using AirportSim.Infra.Data.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace AirportSim.Infra.Data.Entities
{
    public class AirplaneEntity : IDateable
    {
        [Key]
        public Guid Id { get; set; }
        public string Type { get; set; }
        public bool IsOutside { get; set; }
        public int Objective { get; set; }
        public string StationName { get; set; }
        public DateTimeOffset EnteredAt { get; set; }

        DateTimeOffset IDateable.Date => EnteredAt;
    }
}
