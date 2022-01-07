using AirportSim.Domain.Interfaces;
using AirportSim.Infra.Data;
using AirportSim.Infra.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AirportSim.Infra.IoC
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddAirportData(this IServiceCollection services,string connectionString)
        {
            services.AddDbContextFactory<AirportContext>(options =>
            {
                options.UseSqlServer(connectionString);
            },ServiceLifetime.Singleton);
            services.AddSingleton<IAirportContextFactory,AirportContextFactory>();
            services.AddSingleton<IAirportRepository, AirportRepository>();
            return services;
        }

    }
}
