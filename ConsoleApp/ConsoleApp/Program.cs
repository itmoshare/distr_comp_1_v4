using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Timers;
using CommandLine;

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
						HandleFile(o.Path, o.ThreadsCount);
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

		private static void HandleFile(string path, int threadsCount)
		{
			Console.WriteLine("Parsing data...");
			var data = FileParser.Parse(path);

			var worker = new Worker();
			StartLogging(worker);

			var stopwatch = Stopwatch.StartNew();
			var result = worker.DowWork(threadsCount, data);
			File.AppendAllText("result.txt", $"\nWork done! threads: {threadsCount} result: {result} time: {stopwatch.ElapsedMilliseconds}");

			Console.WriteLine("\nWork done!");
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