using AirportSim.Infra.Data.Entities;
using AirportSim.Infra.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AirportSim.Infra.Data
{
    public class AirportContext : DbContext , IAirportContext
    {
        public DbSet<StationEntity> Stations { get; set; }
        public DbSet<AirplaneEntity> Airplanes { get; set; }
        public DbSet<StationEventEntity> Events { get; set; }

        IEnumerable<StationEntity> IAirportContext.Stations => Stations;
        IEnumerable<AirplaneEntity> IAirportContext.Airplanes => Airplanes;
        IEnumerable<StationEventEntity> IAirportContext.Events => Events;

        public AirportContext(DbContextOptions<AirportContext> options):base(options)
        {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StationEntity>()
                .Property(s => s.DepartureStationNames)
                .HasConversion(
                v => JsonSerializer.Serialize(v,default),
                v => JsonSerializer.Deserialize<List<string>>(v, default));

            modelBuilder.Entity<StationEntity>()
                .Property(s => s.LandStationNames)
                .HasConversion(
                v => JsonSerializer.Serialize(v, default),
                v => JsonSerializer.Deserialize<List<string>>(v, default));

            modelBuilder.Entity<StationEntity>()
                
                .HasData(
                new StationEntity 
                { 
                    Name = "station1",
                    DisplayName = "Landing 1",
                    WaitTimeInSeconds = 10,
                    LandStationNames = new [] { "station2" },
                    DepartureStationNames = Array.Empty<string>(),
                    IsDepartable = false,
                    IsEventable = false,
                    IsLandable = true
                },
                new StationEntity 
                { 
                    Name = "station2",
                    DisplayName = "Landing 2",
                    WaitTimeInSeconds = 10,
                    LandStationNames = new[] { "station3" },
                    DepartureStationNames = Array.Empty<string>(),
                    IsDepartable = false,
                    IsEventable = false,
                    IsLandable = false
                },
            new StationEntity 
            { 
                Name = "station3",
                DisplayName = "Landing 3",
                WaitTimeInSeconds = 10,
                LandStationNames = new[] { "station4" },
                DepartureStationNames = Array.Empty<string>(),
                IsDepartable = false,
                IsEventable = false,
                IsLandable = false
            },
            new StationEntity 
            { 
                Name = "station4",
                DisplayName = "Runway",
                WaitTimeInSeconds = 10,
                LandStationNames = new[] { "station5" },
                DepartureStationNames = new[] { "station9" },
                IsDepartable = false,
                IsEventable = true,
                IsLandable = false
            },
            new StationEntity 
            { Name = "station5",
                DisplayName = "Transportation route 1",
                WaitTimeInSeconds = 10,
                LandStationNames = new[] { "station6", "station7" },
                DepartureStationNames = Array.Empty<string>(),
                IsDepartable = false,
                IsEventable = true,
                IsLandable = false
            },
            new StationEntity 
            { 
                Name = "station6",
                DisplayName = "Boarding gate 1",
                WaitTimeInSeconds = 10,
                LandStationNames = Array.Empty<string>(),
                DepartureStationNames = new[] { "station8" },
                IsDepartable = true,
                IsEventable = true,
                IsLandable = false
            },
            new StationEntity 
            { 
                Name = "station7",
                DisplayName = "Boarding gate 2",
                WaitTimeInSeconds = 10,
                LandStationNames = Array.Empty<string>(),
                DepartureStationNames = new[] { "station8" },
                IsDepartable = true,
                IsEventable = true,
                IsLandable = false
            },
            new StationEntity 
            {
                Name = "station8",
                DisplayName = "Transportation route 2",
                WaitTimeInSeconds = 10,
                LandStationNames = Array.Empty<string>(),
                DepartureStationNames = new[] { "station4" },
                IsDepartable = false,
                IsEventable = true,
                IsLandable = false
            },
            new StationEntity 
            { 
                Name = "station9",
                DisplayName = "Takeoff",
                WaitTimeInSeconds = 10,
                LandStationNames = Array.Empty<string>(),
                DepartureStationNames = Array.Empty<string>(),
                IsDepartable = false,
                IsEventable = false,
                IsLandable = false
            });
        }

        async Task IAirportContext.AddPlaneAsync(AirplaneEntity airplaneEntity) => await Airplanes.AddAsync(airplaneEntity);
        async Task IAirportContext.SaveChangesAsync() => await SaveChangesAsync();
        async Task IAirportContext.AddEventAsync(StationEventEntity stationEventEntity) => await Events.AddAsync(stationEventEntity);
        async Task<AirplaneEntity> IAirportContext.FindAirplaneAsync(Guid id) => await Airplanes.FindAsync(id);
        async Task<StationEntity> IAirportContext.FindStationAsync(string name) => await Stations.FindAsync(name);
        async Task<StationEventEntity> IAirportContext.FindEventAsync(Guid eventId) => await Events.FindAsync(eventId);
        void IAirportContext.RemoveAirplane(AirplaneEntity planeEntity) => Airplanes.Remove(planeEntity);
        void IAirportContext.RemoveEvent(StationEventEntity eventEntity) => Events.Remove(eventEntity);

        void IAirportContext.EnsureCreated() => Database.EnsureCreated();
        void IAirportContext.EnsureDeleted() => Database.EnsureDeleted();
    }
}
