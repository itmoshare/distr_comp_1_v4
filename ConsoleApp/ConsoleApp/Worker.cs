using System;
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

		public double DoWork(IScheduler scheduler, (int, int, int)[][] data)
		{
			WorkSize = data.Length;
			scheduler.ThreadPool.Start();
			foreach (var curLine in data)
			{
				scheduler.Push(() => HandleLine(curLine), curLine.Length);
			}

			while (true)
			{
				var workload = scheduler.ThreadPool.GetWorkload();
				if (workload.All(x => x == 0))
					break;
				Thread.Sleep(50);
			}
			
			scheduler.ThreadPool.Stop();

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
				Interlocked.Increment(ref _progress);
			}
		}

		private static double CalcDistance((int, int, int) point1, (int, int, int) point2)
		{
			return Math.Sqrt(
				Math.Pow(point1.Item1 + point2.Item1, 2) +
				Math.Pow(point1.Item2 + point2.Item2, 2) +
				Math.Pow(point1.Item3 + point2.Item3, 2));
		}
	}
}