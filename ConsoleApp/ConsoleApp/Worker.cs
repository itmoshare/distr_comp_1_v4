using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp
{
	public class Worker
	{
		private volatile int _progress = 0;
		public int Progress => _progress;
		public int WorkSize { get; private set; }

		private TaskFactory _limitedTaskFactory;
		private readonly Stack<Task> _tasks = new Stack<Task>();
		private static readonly Regex _regex = new Regex(@"^\((\d+);(\d+);(\d+)\)$", RegexOptions.Compiled);
		private double _sum = 0;
		private object _sumLock = new object();

		public async Task<double> DowWork(int threadsCount, Stream inputStream)
		{
			_limitedTaskFactory = new TaskFactory(new VeryLimitedScheduler(threadsCount));
			using (var reader = new StreamReader(inputStream))
			{
				WorkSize = Convert.ToInt32(reader.ReadLine());
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					var line1 = line;
					_tasks.Push(_limitedTaskFactory.StartNew(() => HandleLine(line1)));
				}
			}

			await Task.WhenAll(_tasks).ConfigureAwait(false);
			return _sum;
		}

		private void HandleLine(string line)
		{
			var values = ParseLine(line);

			var curSum = values
				.Skip(1)
				.Zip(values, CalcDistance)
				.Sum();
			lock (_sumLock)
			{
				_sum += curSum;
				Interlocked.Increment(ref _progress);
			}
		}

		private static (int, int, int)[] ParseLine(string line)
		{
			return line
				.Split(' ')
				.Select(x => _regex.Match(x))
				.Select(x => (Convert.ToInt32(x.Groups[1].Value), Convert.ToInt32(x.Groups[2].Value), Convert.ToInt32(x.Groups[3].Value)))
				.ToArray();
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