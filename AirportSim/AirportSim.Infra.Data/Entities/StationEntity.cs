using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportSim.Infra.Data.Entities
{
    public class StationEntity 
    {
        [Key]
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public TimeSpan WaitTime { get; set; }
        public Guid CurrentPlaneId { get; set; }
        public IEnumerable<string> LandStationNames { get; set; }
        public IEnumerable<string> DepartureStations { get; set; }
        public bool IsEventable { get; set; }
        public bool IsLandable { get; set; }
        public bool IsDepartable { get; set; }

        
    }
}
