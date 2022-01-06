using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportSim.Infra.Data.Interfaces
{
    public interface IDateable
    {
        public DateTimeOffset Date { get; }
    }
}
