using System;

namespace ConsoleApp.Scheduler
{
    public class PredictiveScheduler : IScheduler
    {
        public ThreadPool ThreadPool { get; }

        public PredictiveScheduler(ThreadPool threadPool)
        {
            ThreadPool = threadPool;
        }
        
        public void Push(Action action, int weight)
        {
            throw new NotImplementedException();
        }
    }
}