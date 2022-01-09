using System;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Application.Interfaces
{
    public interface ISimulator
    {
        void Init();
        void Stop();
        void Start();
        Task<(bool IsSuccess, string Message)> SendLandAirplaneAsync();
        Task<(bool IsSuccess, string Message)> SendDepartureAirplaneAsync();
        Task<(bool IsSuccess, string Message)> SendFireEventAsync();
        Task<(bool IsSuccess, string Message)> SendCracksEventAsync();
    }
}
