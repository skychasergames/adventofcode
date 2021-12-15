using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Day8 : PuzzleBase
{
	//	  0:      1:      2:      3:      4:
	//	 aaaa    ....    aaaa    aaaa    ....
	//	b    c  .    c  .    c  .    c  b    c
	//	b    c  .    c  .    c  .    c  b    c
	//	 ....    ....    dddd    dddd    dddd
	//	e    f  .    f  e    .  .    f  .    f
	//	e    f  .    f  e    .  .    f  .    f
	//	 gggg    ....    gggg    gggg    ....
	//	
	//	  5:      6:      7:      8:      9:
	//	 aaaa    aaaa    aaaa    aaaa    aaaa
	//	b    .  b    .  .    c  b    c  b    c
	//	b    .  b    .  .    c  b    c  b    c
	//	 dddd    dddd    ....    dddd    dddd
	//	.    f  e    f  .    f  e    f  .    f
	//	.    f  e    f  .    f  e    f  .    f
	//	 gggg    gggg    ....    gggg    gggg

	private int[] _segmentCountPerDigit =
	{
		6,	// 0
		2,	// 1 (unique)
		5,	// 2
		5,	// 3
		4,	// 4 (unique)
		5,	// 5
		6,	// 6
		3,	// 7 (unique)
		7,	// 8 (unique)
		6,	// 9
	};

	protected override void ExecutePuzzle1()
	{
		int totalKnownDigits = 0;
		foreach (string line in _inputDataLines)
		{
			string[] lineData = SplitString(line, " | ");
			string[] uniqueDigitStrings = SplitString(lineData[0], " ");	// One of each of the ten digits
			string[] puzzleDigitStrings = SplitString(lineData[1], " ");	// The four digit code to solve

			Dictionary<string, int> stringToDigitMapping = new Dictionary<string, int>
			{
				{ FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentCountPerDigit[1]), 1 },
				{ FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentCountPerDigit[4]), 4 },
				{ FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentCountPerDigit[7]), 7 },
				{ FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentCountPerDigit[8]), 8 },
			};

			totalKnownDigits += puzzleDigitStrings.Count(digitString => stringToDigitMapping.ContainsKey(OrderString(digitString)));
		}

		LogResult("Total known digits", totalKnownDigits);
	}

	private string OrderString(string str)
	{
		return string.Concat(str.OrderBy(c => c));
	}

	private string FindDigitStringWithUniqueSegmentCount(string[] digitStrings, int segmentCount)
	{
		List<string> validDigitStrings = digitStrings.Where(str => str.Length == segmentCount).ToList();
		if (validDigitStrings.Count == 1)
		{
			return OrderString(validDigitStrings[0]);
		}

		LogError("Found " + validDigitStrings.Count + " digits with " + segmentCount + " segments");
		return string.Empty;
	}

	protected override void ExecutePuzzle2()
	{
		
	}
}
