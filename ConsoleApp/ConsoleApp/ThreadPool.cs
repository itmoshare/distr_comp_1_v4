using System;
using System.Collections.Concurrent;
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
        private readonly bool[] _threadStates;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentQueue<Action>[] _actionQueues;
        private readonly int[] _handledCount;

        public int[] GetWorkload() => _actionQueues.Select(x => x.Count).ToArray();

        public bool IsFree() => GetWorkload().All(x => x == 0) && _threadStates.All(x => !x);
        
        private readonly Stopwatch[] _workStopwatches;
        private readonly Stopwatch[] _waitStopwatches;
        
        public ThreadPool(int size)
        {
            Size = size;
            _cancellationTokenSource = new CancellationTokenSource();
            _threadStates = new bool[Size];
            _threads = new Thread[Size];
            _actionQueues = new ConcurrentQueue<Action>[Size];
            _handledCount = new int[Size];
            _waitStopwatches = new Stopwatch[Size];
            _workStopwatches = new Stopwatch[Size];
            for (var i = 0; i < Size; i++)
            {
                var i1 = i;
                _actionQueues[i] = new ConcurrentQueue<Action>();
                _threads[i] = new Thread(() => ThreadRuntimeMethod(i1, _cancellationTokenSource.Token));
                _workStopwatches[i] = new Stopwatch();
                _waitStopwatches[i] = new Stopwatch();
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
            _cancellationTokenSource.Cancel();
        }
        
        private void ThreadRuntimeMethod(int threadNumber, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_actionQueues[threadNumber].TryDequeue(out var action))
                {
                    ThreadEnterWork(threadNumber);
                    action?.Invoke();
                    ThreadExitWork(threadNumber);
                }
            }
            _waitStopwatches[threadNumber].Stop();
        }

        private void ThreadEnterWork(int threadNumber)
        {
            _waitStopwatches[threadNumber].Stop();
            _workStopwatches[threadNumber].Start();
            _threadStates[threadNumber] = true;
        }
        
        private void ThreadExitWork(int threadNumber)
        {
            _workStopwatches[threadNumber].Stop();
            _waitStopwatches[threadNumber].Start();
            _handledCount[threadNumber]++;
            _threadStates[threadNumber] = false;
        }
        
        public TimeSpan GetThreadWorkTime(int threadNumber) => _workStopwatches[threadNumber].Elapsed;

        public TimeSpan GetThreadWaitTime(int threadNumber) => _waitStopwatches[threadNumber].Elapsed;

        public int[] GetHandledCounts() => _handledCount;
    }
}