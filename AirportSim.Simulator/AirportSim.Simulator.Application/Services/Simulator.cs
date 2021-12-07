using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AirportSim.Simulator.Application.Services
{
    public class Simulator
    {
        private Timer eventTimer;
        public Simulator()
        {
            eventTimer = new(10000);
            eventTimer.AutoReset = true;
            eventTimer.Elapsed += EventTimer_Elapsed;
        }

        private void EventTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
