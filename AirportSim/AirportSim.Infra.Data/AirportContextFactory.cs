using AirportSim.Infra.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AirportSim.Infra.Data
{
    public class AirportContextFactory :  IAirportContextFactory
    {
        private readonly IDbContextFactory<AirportContext> contextFactory;

        public AirportContextFactory(IDbContextFactory<AirportContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        IAirportContext IAirportContextFactory.CreateAirportContext() => contextFactory.CreateDbContext();
    }
}
