using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
						HandleFile(o.Path, o.ThreadsCount).ConfigureAwait(false).GetAwaiter().GetResult();
					}
				});
		}

		private static void GenerateFile(string path, int size)
		{
			var random = new Random();
			using (var file = File.CreateText(path))
			{
				file.WriteLine(size);
				for (var i = 0; i < size; i++)
				{
					var points = Enumerable.Range(0, random.Next(2, 10))
						.Select(x => $"({random.Next()};{random.Next()};{random.Next()})");
					file.WriteLine(string.Join(" ", points));
				}
			}

			Console.WriteLine("Generated");
		}

		private static async Task HandleFile(string path, int threadsCount)
		{
			var worker = new Worker();

			using (var file = File.OpenRead(path))
			{
				var stopwatch = Stopwatch.StartNew();
				var task = worker.DowWork(threadsCount, file);
				StartLogging(task, worker);
				Console.WriteLine($"\nWork done! result:{await task} time:{stopwatch.ElapsedMilliseconds}");
			}

			Console.WriteLine("\nWork done!");
		}

		private static void StartLogging(Task task, Worker worker)
		{
			var timer = new Timer
			{
				Interval = 500,
				AutoReset = true,
				Enabled = true
			};
			timer.Elapsed += (sender, eventArgs) =>
			{
				if (task.IsCompleted)
					timer.Stop();
				Console.Write($"\r{(float) worker.Progress / worker.WorkSize:P}");
			};
		}
	}
}