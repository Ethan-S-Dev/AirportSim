using AirportSim.Infra.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportSim.Infra.Data.Entities
{
    public class StationEventEntity : IDateable
    {
        [Key]
        public Guid Id { get; set; }
        public int EventType { get; set; }
        public TimeSpan EventTime { get; set; }
        public DateTimeOffset RecivedAt { get; set; }
        public bool IsStarted { get; set; }
        public string StationName { get; set; }

        DateTimeOffset IDateable.Date => RecivedAt;
    }
}
