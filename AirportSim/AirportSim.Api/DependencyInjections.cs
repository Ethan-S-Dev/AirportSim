using AirportSim.Api.Hubs;
using AirportSim.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AirportSim.Api
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddHubService(this IServiceCollection services)
        {
            services.AddSingleton<IHubService, HubService>();
            return services;
        }
    }
}
