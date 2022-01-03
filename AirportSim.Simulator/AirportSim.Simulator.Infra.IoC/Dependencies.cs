using AirportSim.Simulator.Domain.Interfaces;
using AirportSim.Simulator.Infra.AirportControl;
using AirportSim.Simulator.Infra.Random;
using Microsoft.Extensions.DependencyInjection;

namespace AirportSim.Simulator.Infra.IoC
{
    public static class Dependencies
    {
        public static IServiceCollection AddRandom(this IServiceCollection services)
        {
            services.AddSingleton<IRandom, BasicRandom>();
            return services;
        }

        public static IServiceCollection AddAirportControl(this IServiceCollection services)
        {
            services.AddSingleton<IAirportSimClient, AirportSimClient>();
            return services;
        }
    }
}
