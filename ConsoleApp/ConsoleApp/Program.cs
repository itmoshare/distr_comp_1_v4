using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using CommandLine;
using ConsoleApp.Scheduler;

namespace ConsoleApp
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(o =>
				{
					if (o.Generate != -1)
					{
						GenerateFile(o.Path, o.Generate);
					}
					else
					{
						HandleFile(o.Path, o.ThreadsCount, o.Scheduler);
					}
				});
		}

		private static void GenerateFile(string path, int size)
		{
			var random = new Random();
			using (var file = File.CreateText(path))
			{
				for (var i = 0; i < size; i++)
				{
					var points = Enumerable.Range(0, random.Next(2, 10))
						.Select(x => $"({random.Next()};{random.Next()};{random.Next()})");
					file.WriteLine(string.Join(" ", points));
				}
			}

			Console.WriteLine("Generated");
		}

		private static void HandleFile(string path, int threadsCount, int schedulerNumber)
		{
			Console.WriteLine("Parsing data...");
			var data = FileParser.Parse(path);

			var worker = new Worker();
			StartLogging(worker);

			var threadPool = new ThreadPool(threadsCount);
			var stopwatch = Stopwatch.StartNew();
			var scheduler = GetScheduler(threadPool, schedulerNumber);
			var result = worker.DoWork(scheduler, data);
			BuildReport(stopwatch.Elapsed, scheduler);

			Console.WriteLine("\nWork done!");
		}

		private static IScheduler GetScheduler(ThreadPool threadPool, int number)
		{
			switch (number)
			{
				case 1:
					return new RoundRobinScheduler(threadPool);
				case 2:
					return new LeastLoadedScheduler(threadPool);
				case 3:
					return new PredictiveScheduler(threadPool);
				default:
					throw new ArgumentException();
			}
		}

		private static void BuildReport(TimeSpan fullTime, IScheduler scheduler)
		{
			var report = $"threads: {scheduler.ThreadPool.Size} time: {fullTime.Milliseconds}";
			for (var i = 0; i < scheduler.ThreadPool.Size; i++)
			{
				var liveTime = scheduler.ThreadPool.GetThreadLiveTime(i).Milliseconds;
				var waitTime = scheduler.ThreadPool.GetThreadWorkTime(i).Milliseconds;
				report += $"\t thread {i} : " +
				          $"live time = {liveTime} " +
				          $"work time = {waitTime}" +
				          $"wait time = {waitTime - liveTime}";
			}
			Console.WriteLine(report);
		}

		private static void StartLogging(Worker worker)
		{
			var timer = new Timer
			{
				Interval = 400,
				AutoReset = true,
				Enabled = true,
			};
			timer.Elapsed += (sender, eventArgs) =>
			{
				if (worker.WorkSize == 0)
				{
					return;
				}

				if (worker.Progress >= worker.WorkSize)
					timer.Stop();

				Console.Write($"\r{(float) worker.Progress / (worker.WorkSize != 0 ? worker.WorkSize : 1):P}");
			};
		}
	}
}