using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace AoC2021
{
	public class Day14 : PuzzleBase
	{
		[SerializeField] private int _puzzle1Iterations = 10;
		[SerializeField] private int _puzzle2Iterations = 40;
		[SerializeField] private float _stepInterval = 1f;
	
		private EditorCoroutine _executePuzzleCoroutine = null;

		protected override void ExecutePuzzle1()
		{
			if (_executePuzzleCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
			}
		
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle1Process(), this);
		}

		private IEnumerator ExecutePuzzle1Process()
		{
			string polymerTemplate = _inputDataLines[0];

			List<PairInsertionRule> pairInsertionRules = _inputDataLines
				.Where(line => line.Contains("->"))
				.Select(ruleString => new PairInsertionRule(ruleString))
				.ToList();

			string polymer = polymerTemplate;
			for (int puzzleIteration = 0; puzzleIteration < _puzzle1Iterations; puzzleIteration++)
			{
				StringBuilder polymerStringBuilder = new StringBuilder();
			
				// Append each element, inserting a new element if necessary
				for (int pairIndex = 0; pairIndex < polymer.Length - 1; pairIndex++)
				{
					string pair = polymer.Substring(pairIndex, 2);

					polymerStringBuilder.Append(pair[0]);
				
					PairInsertionRule insertionRule = pairInsertionRules.FirstOrDefault(rule => rule.Pair.Equals(pair));
					if (insertionRule != null)
					{
						polymerStringBuilder.Append(insertionRule.ElementToInsert);
					}
				}
			
				// Append the final element
				polymerStringBuilder.Append(polymer.Last());

				// Finished
				polymer = polymerStringBuilder.ToString();

				LogResult("Current polymer string", polymer);
			
				EditorWaitForSeconds interval = new EditorWaitForSeconds(_stepInterval);
				yield return interval;
			}

			List<IGrouping<char, char>> elements = polymer
				.GroupBy(element => element)
				.OrderByDescending(group => group.Count())
				.ToList();

			char mostCommonElement = elements.First().Key;
			int mostCommonElementCount = elements.First().Count();
			LogResult("Most common element", mostCommonElement + " (x" + mostCommonElementCount + ")");
			
			char leastCommonElement = elements.Last().Key;
			int leastCommonElementCount = elements.Last().Count();
			LogResult("Least common element", leastCommonElement + " (x" + leastCommonElementCount + ")");

			int finalResult = mostCommonElementCount - leastCommonElementCount;
			LogResult("Final result", finalResult);

			_executePuzzleCoroutine = null;
		}

		private class PairInsertionRule
		{
			public string Pair { get; }
			public char ElementToInsert { get; }

			public PairInsertionRule(string ruleString)
			{
				Pair = ruleString.Substring(0, 2);
				ElementToInsert = ruleString.Last();
			}
		}

		protected override void ExecutePuzzle2()
		{
			if (_executePuzzleCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
			}
		
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle2Process(), this);
		}

		private IEnumerator ExecutePuzzle2Process()
		{
			string polymerTemplate = _inputDataLines[0];

			List<PairInsertionRule> pairInsertionRules = _inputDataLines
				.Where(line => line.Contains("->"))
				.Select(ruleString => new PairInsertionRule(ruleString))
				.ToList();

			Dictionary<string, ulong> pairCounts = new Dictionary<string, ulong>();
			Dictionary<char, ulong> elementCounts = new Dictionary<char, ulong>();
		
			// Build initial pair/element counts
			for (int pairIndex = 0; pairIndex < polymerTemplate.Length - 1; pairIndex++)
			{
				string pair = polymerTemplate.Substring(pairIndex, 2);

				if (!pairCounts.ContainsKey(pair))
				{
					pairCounts.Add(pair, 0);
				}
				pairCounts[pair]++;

				char element = pair[0];
				if (!elementCounts.ContainsKey(element))
				{
					elementCounts.Add(element, 0);
				}
				elementCounts[element]++;
			}

			char lastElement = polymerTemplate.Last();
			if (!elementCounts.ContainsKey(lastElement))
			{
				elementCounts.Add(lastElement, 0);
			}
			elementCounts[lastElement]++;
		
			// Execute puzzle
			for (int puzzleIteration = 0; puzzleIteration < _puzzle2Iterations; puzzleIteration++)
			{
				Dictionary<string, ulong> previousPairCounts = pairCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
			
				foreach (var previousPairCount in previousPairCounts)
				{
					string pair = previousPairCount.Key;
					ulong count = previousPairCount.Value;
				
					PairInsertionRule insertionRule = pairInsertionRules.FirstOrDefault(rule => rule.Pair.Equals(pair));
					if (insertionRule != null)
					{
						char element = insertionRule.ElementToInsert;
					
						// Remove the old pair and add the two new ones formed by inserting the new element
						pairCounts[pair] -= count;
					
						string newPair1 = string.Concat(pair[0], element);
						if (!pairCounts.ContainsKey(newPair1))
						{
							pairCounts.Add(newPair1, 0);
						}
						pairCounts[newPair1] += count;
					
						string newPair2 = string.Concat(element, pair[1]);
						if (!pairCounts.ContainsKey(newPair2))
						{
							pairCounts.Add(newPair2, 0);
						}
						pairCounts[newPair2] += count;
					
						// Add the new element
						if (!elementCounts.ContainsKey(element))
						{
							elementCounts.Add(element, 0);
						}
						elementCounts[element] += count;
					}
				}
			
				LogResult("Completed iteration", puzzleIteration + 1);
			
				EditorWaitForSeconds interval = new EditorWaitForSeconds(_stepInterval);
				yield return interval;
			}

			List<KeyValuePair<char, ulong>> elements = elementCounts
				.OrderByDescending(group => group.Value)
				.ToList();

			char mostCommonElement = elements.First().Key;
			ulong mostCommonElementCount = elements.First().Value;
			LogResult("Most common element", mostCommonElement + " (x" + mostCommonElementCount + ")");
			
			char leastCommonElement = elements.Last().Key;
			ulong leastCommonElementCount = elements.Last().Value;
			LogResult("Least common element", leastCommonElement + " (x" + leastCommonElementCount + ")");

			ulong finalResult = mostCommonElementCount - leastCommonElementCount;
			LogResult("Final result", finalResult);

			_executePuzzleCoroutine = null;
		}
	}
}
