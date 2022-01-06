using AirportSim.Application.Interfaces;
using AirportSim.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AirportSim.Application
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddControlTower(this IServiceCollection services)
        {
            services.AddSingleton<IControlTower, ControlTower>();

            return services;
        }
    }
}
