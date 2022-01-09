using AirportSim.Simulator.Domain.Interfaces;
using AirportSim.Simulator.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Infra.AirportControl
{
    public class AirportSimClient : IAirportSimClient, IDisposable
    {
        private readonly HttpClient http;
        private readonly ILogger<AirportSimClient> logger;

        public AirportSimClient(IConfiguration config, ILogger<AirportSimClient> logger)
        {
            this.http = new HttpClient();
            this.logger = logger;
            var baseUrl = Environment.GetEnvironmentVariable(config["airportSimUrlEnvVariable"]);
            logger.LogInformation(baseUrl);
            http.BaseAddress = new Uri(baseUrl + "/api/controltower/");
        }

        public Task<string[]> GetStationNamesAsync()
        {
            return Task.FromResult<string[]>(new[] { "station4", "station5", "station6", "station7", "station8" });
        }

        public async Task<(bool IsSuccess, string Message)> SendEventAsync(StationEvent stationEvent)
        {
            try
            {
                var result = await http.PostAsJsonAsync("event", stationEvent);
                var message = await result.Content.ReadAsStringAsync();
                if (result.IsSuccessStatusCode)
                    return (true, message);

                logger.LogWarning(message);
                return (false, message);
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex.Message);
                return (false, ex.Message);
            }
        }

        public async Task<(bool IsSuccess, string Message)> SendLandingAsync(Airplane airplane)
        {
            try
            {
                var result = await http.PostAsJsonAsync("land", airplane);
                var message = await result.Content.ReadAsStringAsync();
                if (result.IsSuccessStatusCode)
                    return (true, message);

                logger.LogWarning(message);
                return (true, message);
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex.Message);
                return (false, ex.Message);
            }
        }

        public async Task<(bool IsSuccess, string Message)> SendDepartureAsync(Airplane airplane)
        {
            try
            {
                var result = await http.PostAsJsonAsync("departure", airplane);
                var message = await result.Content.ReadAsStringAsync();
                if (result.IsSuccessStatusCode)
                    return (true, message);

                logger.LogWarning(message);
                return (false, message);
            }
            catch (HttpRequestException ex)
            {
                logger.LogWarning(ex.Message);
                return (false, ex.Message);
            }
        }

        public void Dispose()
        {
            this.http.Dispose();
        }
    }
}
