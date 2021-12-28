using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

public class Day14 : PuzzleBase
{
	[SerializeField] private int _puzzleIterations = 10;
	[SerializeField] private float _stepInterval = 1f;
	
	private EditorCoroutine _executePuzzleCoroutine = null;

	protected override void ExecutePuzzle1()
	{
		if (_executePuzzleCoroutine != null)
		{
			EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
		}
		
		_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
	}

	private IEnumerator ExecutePuzzle()
	{
		string polymerTemplate = _inputDataLines[0];

		List<PairInsertionRule> pairInsertionRules = _inputDataLines
			.Where(line => line.Contains("->"))
			.Select(ruleString => new PairInsertionRule(ruleString))
			.ToList();

		string polymer = polymerTemplate;
		for (int puzzleIteration = 0; puzzleIteration < _puzzleIterations; puzzleIteration++)
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
		
	}
}
