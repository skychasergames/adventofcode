using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AoC2024
{
	public class Day7 : PuzzleBase
	{
		private enum Operator
		{
			ADD,
			MULTIPLY,
			
			Count
		}
		
		protected override void ExecutePuzzle1()
		{
			ulong sumOfValidTestValues = 0;
			
			foreach (string equationLine in _inputDataLines)
			{
				string[] equationComponents = SplitString(equationLine, ": ");
				ulong testValue = ulong.Parse(equationComponents[0]);
				int[] numbers = ParseIntArray(equationComponents[1], " ");
				int numPairs = numbers.Length - 1;

				// Generate all possible combinations of operators
				int numOperatorCombinations = (int)Mathf.Pow((int)Operator.Count, numPairs);
				Operator[][] operatorCombinations = new Operator[numOperatorCombinations][];
				for (int operatorCombination = 0; operatorCombination < operatorCombinations.Length; operatorCombination++)
				{
					operatorCombinations[operatorCombination] = new Operator[numPairs];
					for (int pair = 0; pair < numPairs; pair++)
					{
						operatorCombinations[operatorCombination][pair] = (Operator)Mathf.FloorToInt(operatorCombination / Mathf.Pow((int)Operator.Count, pair) % (int)Operator.Count);
					}
				}
				
				// Perform calculations until a solution is found (or all combinations are exhausted)
				foreach (Operator[] operatorCombination in operatorCombinations)
				{
					ulong calculatedValue = (ulong)numbers[0];
					for (int pair = 0; pair < numPairs; pair++)
					{
						switch (operatorCombination[pair])
						{
						case Operator.ADD:
							calculatedValue += (ulong)numbers[pair+1];
							break;
						
						case Operator.MULTIPLY:
							calculatedValue *= (ulong)numbers[pair+1];
							break;
						
						case Operator.Count:
						default:
							throw new ArgumentOutOfRangeException("Unhandled operator: " + operatorCombination[pair].ToString());
						}
					}

					if (calculatedValue == testValue)
					{
						LogResult("Solution found for equation", equationLine);

						sumOfValidTestValues += testValue;
						break;
					}
				}
			}

			LogResult("Total calibration result", sumOfValidTestValues);
		}

		protected override void ExecutePuzzle2()
		{
			
		}
		
		/*				
		1 pair
		{ ADD }
		{ MUL }
		
		2 pairs
		{ ADD, ADD }
		{ MUL, ADD }
		{ ADD, MUL }
		{ MUL, MUL }
		
		3 pairs
		{ ADD, ADD, ADD }
		{ MUL, ADD, ADD }
		{ ADD, MUL, ADD }
		{ MUL, MUL, ADD }
		{ ADD, ADD, MUL }
		{ MUL, ADD, MUL }
		{ ADD, MUL, MUL }
		{ MUL, MUL, MUL }
						
		0 -> 0, 0, 0
		1 -> 1, 0, 0
		2 -> 0, 1, 0
		3 -> 1, 1, 0
		4 -> 0, 0, 1
		5 -> 1, 0, 1
		6 -> 0, 1, 1
		7 -> 1, 1, 1
		etc.
		*/

	}
}
