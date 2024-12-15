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
			// Match the pattern `mul(X,Y)`, where X and Y are each 1-3 digit numbers,
			// or alternatively match `do()` or `don't()`.
			Regex regex = new Regex(@"(mul\((?<X>\d{1,3}),(?<Y>\d{1,3})\)|do\(\)|don\'t\(\))", RegexOptions.None, TimeSpan.FromSeconds(10));
			int i = 0;
			int totalSum = 0;
			bool instructionsEnabled = true; 
			foreach (string line in _inputDataLines)
			{
				Match matches = regex.Match(line);
				while (matches.Success)
				{
					string instruction = matches.Groups[0].Value;
					switch (instruction)
					{
					case "do()":
						LogResult("Uncorrupted instruction " + i, instruction);
						instructionsEnabled = true;
						break;
					
					case "don't()":
						LogResult("Uncorrupted instruction " + i, instruction);
						instructionsEnabled = false;
						break;
					
					default:
						if (instructionsEnabled)
						{
							LogResult("Uncorrupted instruction " + i, instruction);
							int x = int.Parse(matches.Result("${X}"));
							int y = int.Parse(matches.Result("${Y}"));
							totalSum += (x * y);
						}
						else
						{
							LogResult("Uncorrupted instruction " + i, instruction + " (IGNORED)");
						}
						break;
					}
				
					matches = matches.NextMatch();
					i++;
				}

				LogResult("Total sum", totalSum);
			}
		}
	}
}
