using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ConsoleApp
{
    public class ThreadPool
    {
        public int Size { get; }
        private readonly Thread[] _threads;
        private readonly Queue<Action>[] _actionQueues;

        public int[] GetWorkload() => _actionQueues.Select(x => x.Count).ToArray();

        private readonly Stopwatch[] _threadLiveStopwatches;
        private readonly Stopwatch[] _workStopwatches;
        
        public ThreadPool(int size)
        {
            Size = size;
            _actionQueues = new Queue<Action>[Size];
            _threads = new Thread[Size];
            _threadLiveStopwatches = new Stopwatch[Size];
            _workStopwatches = new Stopwatch[Size];
            for (var i = 0; i < Size; i++)
            {
                var i1 = i;
                _threads[i] = new Thread(() => ThreadRuntimeMethod(i1));
                _threadLiveStopwatches[i] = new Stopwatch();
                _workStopwatches[i] = new Stopwatch();
            }
        }

        public void PushToQueue(int threadNumber, Action action)
        {
            _actionQueues[threadNumber].Enqueue(action);
        }

        public void Start()
        {
            foreach (var thread in _threads)
            {
                thread.Start();
            }
        }

        public void Stop()
        {           
            foreach (var thread in _threads)
            {
                thread.Abort();
            }
            
            for (var i = 0; i < Size; i++)
            {
                _threadLiveStopwatches[i].Stop();
                _workStopwatches[i].Stop();
            }
        }
        
        private void ThreadRuntimeMethod(int threadNumber)
        {
            _threadLiveStopwatches[threadNumber].Start();
            while (true)
            {
                _actionQueues[threadNumber].TryDequeue(out var action);
                _workStopwatches[threadNumber].Start();
                action?.Invoke();
                _workStopwatches[threadNumber].Stop();
            }
        }

        public TimeSpan GetThreadLiveTime(int threadNumber) => _threadLiveStopwatches[threadNumber].Elapsed;

        public TimeSpan GetThreadWorkTime(int threadNumber) => _workStopwatches[threadNumber].Elapsed;
    }
}