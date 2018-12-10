using CommandLine;

namespace ConsoleApp
{
	public class Options
	{
		[Option('g', "generate", HelpText = "Generate new file with size")]
		public int Generate { get; set; } = -1;

		[Option('t', "threads", Default = 8, HelpText = "Number of threads")]
		public int ThreadsCount { get; set; }

		[Option('p', "path", Required = true, HelpText = "Path to file")]
		public string Path { get; set; }

		[Option('s', "scheduler", Required = true, HelpText = "scheduler number (1: Round-Robin 2: Least Loaded 3: Predictive)")]
		public int Scheduler { get; set; }
	}
}