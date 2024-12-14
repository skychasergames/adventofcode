using System.Collections.Generic;
using System.Text;

public class Day3 : PuzzleBase
{
	protected override void ExecutePuzzle1()
	{
		int columns = _inputDataLines[0].Length;
		
		int[] totalZeroesPerColumn = new int[columns];
		int[] totalOnesPerColumn = new int[columns];

		// Count digits
		foreach (string line in _inputDataLines)
		{
			for (int column = 0; column < columns; column++)
			{
				switch (line[column])
				{
				case '0':
					totalZeroesPerColumn[column]++;
					break;
				
				case '1':
					totalOnesPerColumn[column]++;
					break;
				
				default:
					LogError("Invalid character", line[column]);
					break;
				}
			}
		}
		
		// Calculate "gamma" binary number (made of the most common digit per column)
		// and "epsilon" binary number (made of the least common digit per column)
		StringBuilder gammaRateBinary = new StringBuilder(columns);
		StringBuilder epsilonRateBinary = new StringBuilder(columns);
		for (int column = 0; column < columns; column++)
		{
			if (totalOnesPerColumn[column] > totalZeroesPerColumn[column])
			{
				gammaRateBinary.Append('1');
				epsilonRateBinary.Append('0');
			}
			else
			{
				gammaRateBinary.Append('0');
				epsilonRateBinary.Append('1');
			}
		}
		
		int gammaRate = BinaryToInt(gammaRateBinary.ToString());
		int epsilonRate = BinaryToInt(epsilonRateBinary.ToString());
		LogResult("Gamma rate", gammaRate);
		LogResult("Epsilon rate", epsilonRate);
		LogResult("Final product", gammaRate * epsilonRate);
	}
	
	protected override void ExecutePuzzle2()
	{
		string oxygenGeneratorRatingBinary = FindPuzzle2Rating(true, '1');
		string co2ScrubberRatingBinary = FindPuzzle2Rating(false, '0');
		
		LogResult("Oxygen generator rating", oxygenGeneratorRatingBinary);
		LogResult("CO2 scrubber rating", co2ScrubberRatingBinary);
		
		int oxygenGeneratorRating = BinaryToInt(oxygenGeneratorRatingBinary);
		int co2ScrubberRating = BinaryToInt(co2ScrubberRatingBinary);
		LogResult("Final product", oxygenGeneratorRating * co2ScrubberRating);
	}

	private string FindPuzzle2Rating(bool findMostCommon, char tieBreaker)
	{
		int columns = _inputDataLines[0].Length;

		List<string> validLines = new List<string>(_inputDataLines);
		for (int column = 0; column < columns; column++)
		{
			int totalZeroes = 0;
			int totalOnes = 0;

			foreach (string line in validLines)
			{
				switch (line[column])
				{
				case '0':
					totalZeroes++;
					break;

				case '1':
					totalOnes++;
					break;

				default:
					LogError("Invalid character", line[column]);
					break;
				}
			}

			char keepCharacter;
			if (totalZeroes == totalOnes)
			{
				keepCharacter = tieBreaker;
			}
			else if (findMostCommon)
			{
				keepCharacter = (totalOnes > totalZeroes ? '1' : '0');
			}
			else
			{
				keepCharacter = (totalOnes > totalZeroes ? '0' : '1');
			}
			
			validLines.RemoveAll(line => !line[column].Equals(keepCharacter));
			
			if (validLines.Count == 1)
			{
				return validLines[0];
			}
		}

		LogError("Failed to find single rating, ended up with", validLines.Count);
		return null;
	}

	private int BinaryToInt(string binary)
	{
		return System.Convert.ToInt32(binary, 2);
	}
}
