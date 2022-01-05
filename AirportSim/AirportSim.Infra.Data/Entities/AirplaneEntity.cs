using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportSim.Infra.Data.Entities
{
    public class AirplaneEntity
    {
        [Key]
        public Guid Id { get; set; }
        public string Type { get; set; }
        public bool IsEntered { get; set; }
        public bool IsExited { get; set; }
        public int Objective { get; set; }
        public string StationName { get; set; }
        [ForeignKey(nameof(StationName))]
        public StationEntity CurrentStation { get; set; }
        public DateTimeOffset EnteredAt { get; set; }
    }
}
