using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AirportSim.Domain.Models
{
    public delegate Task MovingStationEventHandler(Airplane sender, MovingStationEventArgs args);
    public class MovingStationEventArgs : EventArgs
    {
        public bool IsEntering { get; set; }
        public bool IsExiting { get; set; }
        public Station Station { get; set; }
        public Path Objective { get; set; }
        public Airplane Airplane { get; set; }
        public DateTimeOffset Time { get; set; }
    }

    public class Airplane
    {
        public Guid Id { get; set; }
        public string Type { get; set; }

        private List<Task> currentTask = new List<Task>();

        public event MovingStationEventHandler MovingStation;

        public void StartLanding(IList<Station> stations)
        {
            var tokenSource = new CancellationTokenSource();
            currentTask.AddRange(stations.Select(s => EnterStation(s,Path.Landing, tokenSource)));
        }

        public void StartDeparture(IList<Station> stations)
        {
            var tokenSource = new CancellationTokenSource();
            currentTask.AddRange(stations.Select(s => EnterStation(s, Path.Departing, tokenSource,true)));
        }

        private async Task EnterStation(Station station,Path path,CancellationTokenSource tokenSource,bool entering)
        {
            try
            {
                if (station == null) // End of path.. no where to go...
                {
                    currentTask = new List<Task>();
                    MovingStation?.Invoke(this, new MovingStationEventArgs { IsExiting = true, IsEntering = entering, Airplane = this, Objective = path, Time = DateTimeOffset.UtcNow, Station = null });
                    return;
                }

                await station.Lock.WaitAsync(tokenSource.Token);
                tokenSource.Cancel();
                
                await station.EventLock.WaitAsync(); // If there is event on station

                //Entered the station
                MovingStation?.Invoke(this, new MovingStationEventArgs { IsExiting = false, IsEntering = entering, Airplane = this, Objective = path, Time = DateTimeOffset.UtcNow, Station = station });


                await Task.Delay(station.WaitTime);

                MoveToNextStation(station, path);

            }
            finally
            {
                if (tokenSource != null)
                {
                    ((IDisposable)tokenSource).Dispose();
                }
            }
        }

        private void MoveToNextStation(Station station, Path path)
        {
            var newTokenSource = new CancellationTokenSource();

            if (path == Path.Landing)
                currentTask.AddRange(station.LandStations.Select(s => EnterStation(s, path, newTokenSource,false)));
            else
                currentTask.AddRange(station.DepartureStations.Select(s => EnterStation(s, path, newTokenSource,false)));
          
            station.Lock.Release();
            station.EventLock.Release();
        }
    }
}
