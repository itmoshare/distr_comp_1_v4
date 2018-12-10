using System;

namespace ConsoleApp.Scheduler
{
    public class RoundRobinScheduler : IScheduler
    {
        public ThreadPool ThreadPool { get; }
        private int _threadPointer;
        private int[] _weights;

        public RoundRobinScheduler(ThreadPool threadPool)
        {
            ThreadPool = threadPool;
            _weights = new int[threadPool.Size];
        }
        
        public void Push(Action task, int weight)
        {
            ThreadPool.PushToQueue(_threadPointer, task);
            _weights[_threadPointer] += weight;
            _threadPointer = (_threadPointer + 1) % ThreadPool.Size;
        }
    }
}