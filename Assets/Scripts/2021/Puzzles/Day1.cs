using UnityEngine;

namespace AoC2021
{
	public class Day1 : PuzzleBase
	{
		protected override void ExecutePuzzle1()
		{
			int numIncreases = 0;
			int previousValue = int.MinValue;
			foreach (string line in _inputDataLines)
			{
				if (int.TryParse(line, out int value))
				{
					if (previousValue == int.MinValue)
					{
						previousValue = value;
						continue;
					}

					if (value > previousValue)
					{
						numIncreases++;
					}

					previousValue = value;
				}
			}

			LogResult("Num increases", numIncreases);
		}

		protected override void ExecutePuzzle2()
		{
			int numIncreases = 0;
			int sumOfPreviousWindow = int.MinValue;

			const int window = 3;
			for (int rootLineNum = 0; rootLineNum < _inputDataLines.Length - (window - 1); rootLineNum++)
			{
				int sumOfWindow = 0;
				for (int lineNum = rootLineNum; lineNum < rootLineNum + window; lineNum++)
				{
					if (int.TryParse(_inputDataLines[lineNum], out int lineValue))
					{
						sumOfWindow += lineValue;
					}
				}

				if (sumOfPreviousWindow == int.MinValue)
				{
					sumOfPreviousWindow = sumOfWindow;
					continue;
				}

				if (sumOfWindow > sumOfPreviousWindow)
				{
					numIncreases++;
				}

				sumOfPreviousWindow = sumOfWindow;
			}

			LogResult("Num increases", numIncreases);
		}
	}
}