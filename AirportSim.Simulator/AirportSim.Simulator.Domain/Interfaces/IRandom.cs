namespace AirportSim.Simulator.Domain.Interfaces
{
    public interface IRandom
    {
        int GetInteger(int min,int max);
        bool GetBoolean();
    }
}
