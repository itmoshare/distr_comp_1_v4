using System;

namespace ConsoleApp.Scheduler
{
    public interface IScheduler
    {
        void Push(Action task, int weight);

        ThreadPool ThreadPool { get; }
    }
}