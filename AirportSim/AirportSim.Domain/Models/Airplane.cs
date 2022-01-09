using AirportSim.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AirportSim.Domain.Models
{
    public delegate Task MovingStationEventHandler(IAirplane sender, MovingStationEventArgs args);
    public class MovingStationEventArgs : EventArgs
    {
        public bool IsEntering { get; set; }
        public bool IsExiting { get; set; }
        public IStation Station { get; set; }
        public Objectives Objective { get; set; }
        public IAirplane Airplane { get; set; }
        public DateTimeOffset Time { get; set; }
     
    }

    public class Airplane : IAirplane, ILoadPlane
    {
        public Guid Id { get; }
        public string Type { get; }
        public bool IsOutside { get; set; }
        public IStation CurrentStation { get; set; }
        public IStation PreviousStation { get; set; }
        public Objectives Objective { get; }
        public event MovingStationEventHandler MovingStation;

        public void Start(IList<IStation> stations,Objectives objective)
        {
            var tokenSource = new CancellationTokenSource();
            stations.Select(s => EnterStation(s, objective, tokenSource, true)).ToList();
        }
        
        Task ILoadPlane.EnterStation(IStation station, Objectives path, bool entering)
        {
            var tokenSource = new CancellationTokenSource();
            return EnterStation(station, path, tokenSource, entering);
        }

        public Airplane(Guid id,string type,Objectives objective,bool isOutside)
        {
            Id = id;
            Type = type;
            Objective = objective;
            IsOutside = isOutside;
        }

        private async Task EnterStation(IStation station,Objectives path,CancellationTokenSource tokenSource,bool entering)
        {
            try
            {
                if (station == null) // End of path.. no where to go...
                {
                    PreviousStation = CurrentStation;
                    CurrentStation = station;
                    MovingStation?.Invoke(this, new MovingStationEventArgs { IsExiting = true, IsEntering = entering, Airplane = this, Objective = path, Time = DateTimeOffset.UtcNow, Station = null });                    
                    ReleaseStation(PreviousStation);
                    return;
                }

                await station.LockStationAsync(tokenSource.Token);
                tokenSource.Cancel();   
                await station.LockEventsAsync(); // If there is event on station                           

                //Entered the station
                PreviousStation = CurrentStation;
                CurrentStation = station;
                MovingStation?.Invoke(this, new MovingStationEventArgs { IsExiting = false, IsEntering = entering, Airplane = this, Objective = path, Time = DateTimeOffset.UtcNow, Station = station });

                //Release the priviews
                ReleaseStation(PreviousStation);

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
        private void MoveToNextStation(IStation station, Objectives path)
        {
            var newTokenSource = new CancellationTokenSource();
            bool any = false;
            switch (path)
            {
                case Objectives.Landing:
                    any = station.LandStations.Select(s => EnterStation(s, path, newTokenSource, false)).ToList().Any();
                    break;
                case Objectives.Departing:
                    any = station.DepartureStations.Select(s => EnterStation(s, path, newTokenSource, false)).ToList().Any();
                    break;
                default:              
                    return;
            }

            if (!any)
                _ = EnterStation(null, path, newTokenSource, false);


            
        }
        private void ReleaseStation(IStation station)
        {
            station?.ReleaseEvents();
            station?.ReleaseStation();
        }      
    }
}
