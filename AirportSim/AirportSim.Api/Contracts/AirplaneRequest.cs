using System;
using System.ComponentModel.DataAnnotations;

namespace AirportSim.Api.Contracts
{
    public class AirplaneRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Type { get; set; }
    }
}
