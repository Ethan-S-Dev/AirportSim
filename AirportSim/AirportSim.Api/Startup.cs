using AirportSim.Api.Hubs;
using AirportSim.Application;
using AirportSim.Application.Interfaces;
using AirportSim.Infra.Data.Interfaces;
using AirportSim.Infra.IoC;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

namespace AirportSim.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable(Configuration["SqlServerEnvVarible"]);
            var clientOrigin = Environment.GetEnvironmentVariable(Configuration["SignalRClientEnvVarible"]);
            var simulatorClientOrigin = Environment.GetEnvironmentVariable(Configuration["SimulatorClientEnvVarible"]);

            services.AddControllers();
            services.AddSignalR(config =>
            {
                config.ClientTimeoutInterval = TimeSpan.FromSeconds(120);
                config.KeepAliveInterval = TimeSpan.FromSeconds(10);
            });

            services.AddHubService();
            services.AddControlTower();
            services.AddAirportData(connectionString);

            services.AddCors(setup =>
            {
                setup.AddPolicy("simulator", conf =>
                {
                    conf.WithOrigins(simulatorClientOrigin)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });

                setup.AddPolicy("signalR", conf =>
                {
                    conf.WithOrigins(clientOrigin)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
                });

            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AirportSim.Api", Version = "v1" });
            });      
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,IAirportContext context,IControlTower controlTower)
        {
            context.EnsureDeleted();
            context.EnsureCreated();
            _ = controlTower.LoadAirportStateAsync();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AirportSim.Api v1"));
            }


            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers()
                .RequireCors("simulator");
                endpoints.MapHub<ControlTowerHub>("/towerhub")
                .RequireCors("signalR");
            });
        }
    }
}
