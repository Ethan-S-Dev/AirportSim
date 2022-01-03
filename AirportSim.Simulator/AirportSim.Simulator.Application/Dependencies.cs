using AirportSim.Simulator.Application.Interfaces;
using AirportSim.Simulator.Domain;
using AirportSim.Simulator.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AirportSim.Simulator.Application
{
    public static class Dependencies
    {
        public static IServiceCollection AddSimulator(this IServiceCollection services)
        {
            services.AddSingleton<IRandomSimEvents, RandomSimEvents>();
            services.AddSingleton<IAirplaneGenerator, AirplaneGenerator>();
            services.AddSingleton<ISimulator, Services.Simulator>();
            return services;
        }
    }
}
