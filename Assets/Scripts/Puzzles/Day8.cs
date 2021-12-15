using System.Collections.Generic;
using System.Linq;
using System.Text;
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
				{ OrderString(FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentCountPerDigit[1])), 1 },
				{ OrderString(FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentCountPerDigit[4])), 4 },
				{ OrderString(FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentCountPerDigit[7])), 7 },
				{ OrderString(FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentCountPerDigit[8])), 8 },
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
			return validDigitStrings[0];
		}

		LogError("Found " + validDigitStrings.Count + " digits with " + segmentCount + " segments");
		return string.Empty;
	}

	//  dddd
	// e    a
	// e    a
	//  ffff
	// g    b
	// g    b
	//  cccc
	protected override void ExecutePuzzle2()
	{
		int sum = 0;
		foreach (string line in _inputDataLines)
		{
			string[] lineData = SplitString(line, " | ");
			string[] uniqueDigitStrings = SplitString(lineData[0], " ");	// One of each of the ten digits
			string[] puzzleDigitStrings = SplitString(lineData[1], " ");	// The four digit code to solve

			Dictionary<string, int> stringToDigitMapping = CalculateStringToDigitMapping(uniqueDigitStrings);

			StringBuilder convertedDigits = new StringBuilder();
			foreach (string digitString in puzzleDigitStrings.Select(OrderString))
			{
				if (stringToDigitMapping.TryGetValue(digitString, out int digit))
				{
					convertedDigits.Append(digit);
				}
				else
				{
					LogError("Mapping doesn't contain digit string", digitString);
				}
			}

			if (int.TryParse(convertedDigits.ToString(), out int value))
			{
				sum += value;
			}
			else
			{
				LogError("Converted digits could not be parsed as int", convertedDigits.ToString());
			}
		}

		LogResult("Sum of converted values", sum);
	}

	private Dictionary<string, int> CalculateStringToDigitMapping(string[] uniqueDigitStrings)
	{
		// Use the four unique digits (1, 4, 7, 8) as a starting point
		string digit1 = FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentCountPerDigit[1]);
		string digit4 = FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentCountPerDigit[4]);
		string digit7 = FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentCountPerDigit[7]);
		string digit8 = FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentCountPerDigit[8]);
		
		List<string> digitStringsWithFiveSegments = uniqueDigitStrings.Where(digitString => digitString.Length == 5).ToList();
		List<string> digitStringsWithSixSegments = uniqueDigitStrings.Where(digitString => digitString.Length == 6).ToList();
		
		// Find the remaining digits (0, 2, 3, 5, 6, 9)
		// 9 - Has six segments, includes all segments that belong to 4
		string digit9 = digitStringsWithSixSegments.First(digitString => ContainsAllSegmentsFromDigit(digitString, digit4));
		digitStringsWithSixSegments.Remove(digit9);

		// 0 - Has six segments, includes all segments that belong to 1  
		string digit0 = digitStringsWithSixSegments.First(digitString => ContainsAllSegmentsFromDigit(digitString, digit1));
		digitStringsWithSixSegments.Remove(digit0);
		
		// 6 - The other digit with six segments
		string digit6 = digitStringsWithSixSegments[0];
		
		// 3 - Has five segments, includes all segments that belong to 1
		string digit3 = digitStringsWithFiveSegments.First(digitString => ContainsAllSegmentsFromDigit(digitString, digit1));
		digitStringsWithFiveSegments.Remove(digit3);
		
		// 5 - Has five segments, all of which fit into 6 and/or 9
		string digit5 = digitStringsWithFiveSegments.First(digitString => ContainsAllSegmentsFromDigit(digit6, digitString));	// Parameters are backwards on this one!
		digitStringsWithFiveSegments.Remove(digit5);
		
		// 2 - The remaining digit
		string digit2 = digitStringsWithFiveSegments[0];

		return new Dictionary<string, int>
		{
			{ OrderString(digit0), 0 },
			{ OrderString(digit1), 1 },
			{ OrderString(digit2), 2 },
			{ OrderString(digit3), 3 },
			{ OrderString(digit4), 4 },
			{ OrderString(digit5), 5 },
			{ OrderString(digit6), 6 },
			{ OrderString(digit7), 7 },
			{ OrderString(digit8), 8 },
			{ OrderString(digit9), 9 },
		};
	}
	
	private bool ContainsAllSegmentsFromDigit(string thisDigit, string otherDigit)
	{
		foreach (char c in otherDigit)
		{
			if (!thisDigit.Contains(c))
			{
				return false;
			}
		}

		return true;
	}
}
