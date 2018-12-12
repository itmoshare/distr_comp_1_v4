using System;

namespace ConsoleApp.Scheduler
{
    public class PredictiveScheduler : IScheduler
    {
        public ThreadPool ThreadPool { get; }

        private long[] _workload;

        public PredictiveScheduler(ThreadPool threadPool)
        {
            ThreadPool = threadPool;
            _workload = new long[threadPool.Size];
        }
        
        public void Push(Action action, int weight)
        {
            var minI = 0;
            for (int i = 0; i < _workload.Length; i++)
            {
                if (_workload[i] < _workload[minI])
                    minI = i;
            }

            _workload[minI] += weight;
            ThreadPool.PushToQueue(minI, action);
        }
    }
}