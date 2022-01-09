using AirportSim.Domain.Models;
using System;
using System.Collections.Generic;

namespace AirportSim.Domain.Interfaces
{
    public interface IAirplane
    {
        Guid Id { get; }
        string Type { get; }
        bool IsOutside { get; set; }
        IStation CurrentStation { get; set; }
        IStation PreviousStation { get; set; }
        Objectives Objective { get; }
        event MovingStationEventHandler MovingStation;

        void Start(IList<IStation> stations,Objectives objective);
    }
}
