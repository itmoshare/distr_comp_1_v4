using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ConsoleApp
{
	public class FileParser
	{
		private static readonly Regex _regex = new Regex(@"^\((\d+);(\d+);(\d+)\)$", RegexOptions.Compiled);

		public static (int, int, int)[][] Parse(string path)
		{
			var result = new List<(int, int, int)[]>();
			using (var reader = new StreamReader(path))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					result.Add(ParseLine(line));
				}
			}

			return result.ToArray();
		}

		private static (int, int, int)[] ParseLine(string line)
		{
			return line
				.Split(' ')
				.Select(x => _regex.Match(x))
				.Select(x => (Convert.ToInt32(x.Groups[1].Value), Convert.ToInt32(x.Groups[2].Value), Convert.ToInt32(x.Groups[3].Value)))
				.ToArray();
		}
	}
}