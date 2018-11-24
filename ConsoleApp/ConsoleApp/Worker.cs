using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ConsoleApp
{
	public class Worker
	{
		private volatile int _progress = 0;
		public int Progress => _progress;
		public int WorkSize { get; private set; }

		private double _sum = 0;
		private object _sumLock = new object();

		public double DowWork(int threadsCount, (int, int, int)[][] data)
		{
			WorkSize = data.Length;
			var threads = SplitToRanges(data.Length, threadsCount)
				.Select(range => new Thread(() => HandleRange(data, range)))
				.ToArray();

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			return _sum;
		}

		private static IEnumerable<(int, int)> SplitToRanges(int size, int rangesCount)
		{
			var countPerRange = size / rangesCount;
			var cur = 0;
			while (true)
			{
				if (cur + 2 * countPerRange > size)
				{
					yield return (cur, size);
					yield break;
				}

				yield return (cur, cur + countPerRange);
				cur += countPerRange;
			}
		}

		private void HandleRange((int, int, int)[][] data, (int, int) range)
		{
			for (var i = range.Item1; i < range.Item2; i++)
			{
				HandleLine(data[i]);
			}
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