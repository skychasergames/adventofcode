using System;
using System.Text.RegularExpressions;

namespace AoC2024
{
	public class Day3 : PuzzleBase
	{
		protected override void ExecutePuzzle1()
		{
			// Match the pattern `mul(X,Y)`, where X and Y are each 1-3 digit numbers.
			// The `(?<X>[...])` construct captures the matched subexpression into a named group. 
			Regex regex = new Regex(@"mul\((?<X>\d{1,3}),(?<Y>\d{1,3})\)", RegexOptions.None, TimeSpan.FromSeconds(10));
			int i = 0;
			int totalSum = 0;

			foreach (string line in _inputDataLines)
			{
				Match matches = regex.Match(line);
				while (matches.Success)
				{
					LogResult("Uncorrupted instruction " + i, matches.Groups[0]);
					int x = int.Parse(matches.Result("${X}"));
					int y = int.Parse(matches.Result("${Y}"));
					totalSum += (x * y);
				
					matches = matches.NextMatch();
					i++;
				}

				LogResult("Total sum", totalSum);
			}
		}

		protected override void ExecutePuzzle2()
		{
			
		}
	}
}
