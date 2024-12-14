using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AoC2021
{
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

		private string[] _segmentsPerDigit =
		{
			"abcefg",	// 0
			"cf",		// 1 (unique)
			"acdeg",	// 2
			"acdfg",	// 3
			"bcdf",		// 4 (unique)
			"abdfg",	// 5
			"abdefg",	// 6
			"acf",		// 7 (unique)
			"abcdefg",	// 8 (unique)
			"abcdfg",	// 9
		};

		[SerializeField] private SevenSegmentDisplay _displayPrefab = null;
		[SerializeField] private GameObject _separatorPrefab = null;
		[SerializeField] private Transform _displayContainer = null;
		[SerializeField] private Color _displayColorKnown = Color.green;
		[SerializeField] private Color _displayColorUnknown = Color.white;

		protected override void ExecutePuzzle1()
		{
			ResetDisplay();
		
			int totalKnownDigits = 0;
			foreach (string line in _inputDataLines)
			{
				string[] lineData = SplitString(line, " | ");
				string[] uniqueDigitStrings = SplitString(lineData[0], " ");	// One of each of the ten digits
				string[] puzzleDigitStrings = SplitString(lineData[1], " ");	// The four digit code to solve

				Dictionary<string, int> stringToDigitMapping = new Dictionary<string, int>
				{
					{ OrderString(FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentsPerDigit[1].Length)), 1 },
					{ OrderString(FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentsPerDigit[4].Length)), 4 },
					{ OrderString(FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentsPerDigit[7].Length)), 7 },
					{ OrderString(FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentsPerDigit[8].Length)), 8 },
				};

				totalKnownDigits += puzzleDigitStrings.Count(digitString => stringToDigitMapping.ContainsKey(OrderString(digitString)));

				foreach (string digitString in uniqueDigitStrings.Select(OrderString))
				{
					DisplayDigit(digitString, stringToDigitMapping);
				}

				Instantiate(_separatorPrefab, _displayContainer);
			
				foreach (string digitString in puzzleDigitStrings.Select(OrderString))
				{
					DisplayDigit(digitString, stringToDigitMapping);
				}
			}

			LogResult("Total known digits", totalKnownDigits);
		}

		private void DisplayDigit(string digitString, Dictionary<string, int> stringToDigitMapping)
		{
			string displayDigitString = digitString;
			Color color = _displayColorUnknown;
			if (stringToDigitMapping.TryGetValue(digitString, out int digit))
			{
				displayDigitString = _segmentsPerDigit[digit];
				color = _displayColorKnown;
			}
		
			SevenSegmentDisplay display = Instantiate(_displayPrefab, _displayContainer);
			display.ShowDigit(displayDigitString, color);
		}

		private void ResetDisplay()
		{
			while (_displayContainer.childCount > 0)
			{
				DestroyImmediate(_displayContainer.GetChild(0).gameObject);
			}
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

		protected override void ExecutePuzzle2()
		{
			ResetDisplay();
		
			int sum = 0;
			foreach (string line in _inputDataLines)
			{
				string[] lineData = SplitString(line, " | ");
				string[] uniqueDigitStrings = SplitString(lineData[0], " ");	// One of each of the ten digits
				string[] puzzleDigitStrings = SplitString(lineData[1], " ");	// The four digit code to solve

				Dictionary<string, int> stringToDigitMapping = CalculateStringToDigitMapping(uniqueDigitStrings);

				foreach (string digitString in uniqueDigitStrings.Select(OrderString))
				{
					DisplayDigit(digitString, stringToDigitMapping);
				}

				Instantiate(_separatorPrefab, _displayContainer);
			
				StringBuilder convertedDigits = new StringBuilder();
				foreach (string digitString in puzzleDigitStrings.Select(OrderString))
				{
					if (stringToDigitMapping.TryGetValue(digitString, out int digit))
					{
						convertedDigits.Append(digit);
						DisplayDigit(digitString, stringToDigitMapping);
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
			string digit1 = FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentsPerDigit[1].Length);
			string digit4 = FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentsPerDigit[4].Length);
			string digit7 = FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentsPerDigit[7].Length);
			string digit8 = FindDigitStringWithUniqueSegmentCount(uniqueDigitStrings, _segmentsPerDigit[8].Length);
		
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
}
