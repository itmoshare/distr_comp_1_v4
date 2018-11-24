using CommandLine;

namespace ConsoleApp
{
	public class Options
	{
		[Option('g', "generate", HelpText = "Generate new file with size")]
		public int Generate { get; set; } = -1;

		[Option('t', "threads", Default = 1, HelpText = "Number of threads")]
		public int ThreadsCount { get; set; }

		[Option('p', "path", Required = true, HelpText = "Path to file")]
		public string Path { get; set; }
	}
}