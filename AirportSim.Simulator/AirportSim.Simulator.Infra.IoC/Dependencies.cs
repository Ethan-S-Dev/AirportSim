using AirportSim.Simulator.Domain.Interfaces;
using AirportSim.Simulator.Infra.Random;
using Microsoft.Extensions.DependencyInjection;

namespace AirportSim.Simulator.Infra.IoC
{
    public static class Dependencies
    {
        public static IServiceCollection AddRandomClient(this IServiceCollection services)
        {
            services.AddSingleton<IRandom, BasicRandom>();
            return services;
        }
    }
}
