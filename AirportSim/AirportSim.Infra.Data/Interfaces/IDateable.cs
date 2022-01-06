using System;

namespace AirportSim.Infra.Data.Interfaces
{
    public interface IDateable
    {
        public DateTimeOffset Date { get; }
    }
}
