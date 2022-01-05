using AirportSim.Infra.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirportSim.Infra.Data
{
    public class AirportContext : DbContext
    {
        public DbSet<StationEntity> Stations { get; set; }
        public DbSet<AirplaneEntity> Airplanes { get; set; }
        public DbSet<StationEventEntity> Events { get; set; }

        public AirportContext(DbContextOptions<AirportContext> options):base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StationEntity>().HasData(
                new StationEntity 
                { 
                    Name = "station1",
                    DisplayName = "Landing 1",
                    WaitTime = new TimeSpan(0,0,10),
                    LandStationNames = new [] { "station2" },
                    DepartureStations = new List<string>(),
                    IsDepartable = false,
                    IsEventable = false,
                    IsLandable = true
                },
                new StationEntity 
                { 
                    Name = "station2",
                    DisplayName = "Landing 2",
                    WaitTime = new TimeSpan(0, 0, 10),
                    LandStationNames = new[] { "station3" },
                    DepartureStations = new List<string>(),
                    IsDepartable = false,
                    IsEventable = false,
                    IsLandable = false
                },
            new StationEntity 
            { 
                Name = "station3",
                DisplayName = "Landing 3",
                WaitTime = new TimeSpan(0, 0, 10),
                LandStationNames = new[] { "station4" },
                DepartureStations = new List<string>(),
                IsDepartable = false,
                IsEventable = false,
                IsLandable = false
            },
            new StationEntity 
            { 
                Name = "station4",
                DisplayName = "Runway",
                WaitTime = new TimeSpan(0, 0, 10),
                LandStationNames = new[] { "station5" },
                DepartureStations = new[] { "station9" },
                IsDepartable = false,
                IsEventable = true,
                IsLandable = false
            },
            new StationEntity 
            { Name = "station5",
                DisplayName = "Transportation route 1",
                WaitTime = new TimeSpan(0, 0, 10),
                LandStationNames = new[] { "station6", "station7" },
                DepartureStations = new List<string>(),
                IsDepartable = false,
                IsEventable = true,
                IsLandable = false
            },
            new StationEntity 
            { 
                Name = "station6",
                DisplayName = "Boarding gate 1",
                WaitTime = new TimeSpan(0, 0, 10),
                LandStationNames = new List<string>(),
                DepartureStations = new[] { "station8" },
                IsDepartable = true,
                IsEventable = true,
                IsLandable = false
            },
            new StationEntity 
            { 
                Name = "station7",
                DisplayName = "Boarding gate 2",
                WaitTime = new TimeSpan(0, 0, 10),
                LandStationNames = new List<string>(),
                DepartureStations = new[] { "station8" },
                IsDepartable = true,
                IsEventable = true,
                IsLandable = false
            },
            new StationEntity 
            {
                Name = "station8",
                DisplayName = "Transportation route 2",
                WaitTime = new TimeSpan(0, 0, 10),
                LandStationNames = new List<string>(),
                DepartureStations = new[] { "station4" },
                IsDepartable = false,
                IsEventable = true,
                IsLandable = false
            },
            new StationEntity 
            { 
                Name = "station9",
                DisplayName = "Takeoff",
                WaitTime = new TimeSpan(0, 0, 10),
                LandStationNames = new List<string>(),
                DepartureStations = new List<string>(),
                IsDepartable = false,
                IsEventable = false,
                IsLandable = false
            });
        }
    }
}
