using System;

namespace ConsoleApp.Scheduler
{
    public class LeastLoadedScheduler : IScheduler
    {
        public ThreadPool ThreadPool { get; }

        private readonly int[] _weights;
        
        public LeastLoadedScheduler(ThreadPool threadPool)
        {
            ThreadPool = threadPool;
            _weights = new int[threadPool.Size];
        }

        /// <inheritdoc />
        public void Push(Action task, int weight)
        {
            var minI = 0;
            var workload = ThreadPool.GetWorkload();
            for (var i = 0; i < workload.Length; i++)
            {
                if (workload[minI] > workload[i])
                    minI = i;
            }
            ThreadPool.PushToQueue(minI, task);
            _weights[minI] += weight;
        }
    }
}