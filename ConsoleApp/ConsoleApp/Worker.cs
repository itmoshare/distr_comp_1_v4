using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ConsoleApp.Scheduler;

namespace ConsoleApp
{
	public class Worker
	{
		private volatile int _progress;
		public int Progress => _progress;
		public int WorkSize { get; private set; }

		private double _sum;
		private object _sumLock = new object();

		private Stopwatch _stopwatch = new Stopwatch();
		public TimeSpan WorkElapsed => _stopwatch.Elapsed;
		
		public double DoWork(IScheduler scheduler, (int, int, int)[][] data)
		{
			WorkSize = data.Length;
			_stopwatch.Start();
			scheduler.ThreadPool.Start();
			foreach (var curLine in data)
			{
				scheduler.Push(() => HandleLine(curLine), curLine.Length);
			}

			while (true)
			{
				if (scheduler.ThreadPool.IsFree())
				{
					Thread.Sleep(5);
					if (scheduler.ThreadPool.IsFree())
						break;
				}
				Thread.Sleep(5);
			}
			
			scheduler.ThreadPool.Stop();
			_stopwatch.Stop();

			return _sum;
		}

		private void HandleLine((int, int, int)[] data)
		{
			var curSum = data
				.Skip(1)
				.Zip(data, CalcDistance)
				.Sum();
			lock (_sumLock)
			{
				_sum += curSum;
			}
			Interlocked.Increment(ref _progress);
		}

		private static double CalcDistance((int, int, int) point1, (int, int, int) point2)
		{
			var t = 0d;
			for (int i = 0; i < 10000; i++)
			{
				t += i / 1.2f;
			}

			return t;
//			return Math.Sqrt(
//				Math.Pow(point1.Item1 + point2.Item1, 2) +
//				Math.Pow(point1.Item2 + point2.Item2, 2) +
//				Math.Pow(point1.Item3 + point2.Item3, 2));
		}
	}
}