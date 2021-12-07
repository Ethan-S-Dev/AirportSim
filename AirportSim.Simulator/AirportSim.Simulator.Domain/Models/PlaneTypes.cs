using System;

namespace AirportSim.Simulator.Domain.Models
{
    public static class PlaneTypes
    {
        public const string Default = "Planey The Big Plane";
        public const string Type1 = "Airbus A380";

        private static readonly Random rnd = new(); 
        public static string GetRandomType()
        {
            return rnd.Next(1) switch
            {
                0 => Type1,
                _ => Default,
            };
        }
    }
}
