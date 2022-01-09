using AirportSim.Simulator.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AirportSim.Simulator.Domain
{
    public delegate void RandomElapsedEventHandler(RandomTimer sender, RandomElapsedEventArgs args);
    public class RandomElapsedEventArgs
    {
        public TimeSpan WaitedTime { get; set; }
    }
    public class RandomTimer
    {
        private readonly IRandom random;
        private readonly int minSec;
        private readonly int maxSec;
        private readonly bool autoReset;
        private Task timerLoop;
        private Task<int> nextWaitTimeTask;
        public event RandomElapsedEventHandler Elapsed;
        private CancellationTokenSource tokenSource;
        private bool live = false;

        public RandomTimer(IRandom random, int minSec, int maxSec, bool live = true,bool autoReset = true)
        {
            this.random = random;
            this.minSec = minSec;
            this.maxSec = maxSec;
            this.autoReset = autoReset;
            nextWaitTimeTask = random.GetIntegerAsync(minSec, maxSec);
            if (live)
                Start();
        }

        private async Task Loop()
        {
            using (tokenSource = new CancellationTokenSource())
            {
                while (live)
                {
                    var time = TimeSpan.FromSeconds(await nextWaitTimeTask);
                    nextWaitTimeTask = random.GetIntegerAsync(minSec, maxSec);
                    await Task.Delay(time, tokenSource.Token);
                    Elapsed?.Invoke(this, new RandomElapsedEventArgs() { WaitedTime = time });
                    if (!autoReset)
                        continue;
                }
            }
        }

        public void Start()
        {
            if (live)
                return;

            live = true;
            timerLoop = Loop();
        }

        public void Stop()
        {
            if (!live)
                return;

            live = false;
            tokenSource.Cancel();
        }
    }
}
