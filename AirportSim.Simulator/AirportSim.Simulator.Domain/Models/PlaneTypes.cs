using AirportSim.Simulator.Domain.Interfaces;
using System;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Domain.Models
{
    public static class PlaneTypes
    {
        public const string Default = "Planey The Big Plane";
        public const string Type1 = "Airbus A380";

        public static async Task<string> GetRandomTypeAsync(IRandom random)
        {
            return await random.GetIntegerAsync(0,1) switch
            {
                0 => Type1,
                _ => Default,
            };
        }
    }
}
