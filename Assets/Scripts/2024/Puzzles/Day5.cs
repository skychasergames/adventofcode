using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AoC2024
{
	public class Day5 : PuzzleBase
	{
		private readonly Dictionary<int, PageRules> _pageRuleset = new Dictionary<int, PageRules>();
		
		protected override void ExecutePuzzle1()
		{
			BuildPageRuleset();

			int sumOfMiddleNumbers = 0;
			
			string[] updateData = _inputDataLines.Where(line => line.Contains(',')).ToArray();
			foreach (string updateString in updateData)
			{
				int[] updatePages = ParseIntArray(SplitString(updateString, ","));
				bool isUpdateInCorrectOrder = IsUpdateInCorrectOrder(updatePages);
				
				LogResult(updateString + " in correct order", isUpdateInCorrectOrder);

				if (isUpdateInCorrectOrder)
				{
					sumOfMiddleNumbers += updatePages[Mathf.FloorToInt(updatePages.Length / 2f)];
				}
			}

			LogResult("Sum of middle numbers", sumOfMiddleNumbers);
		}

		protected override void ExecutePuzzle2()
		{
			
		}

		private class PageRules
		{
			public HashSet<int> mustComeAfterPages = new HashSet<int>();
			public HashSet<int> mustComeBeforePages = new HashSet<int>();
		}

		private void BuildPageRuleset()
		{
			_pageRuleset.Clear();
			
			string[] ruleData = _inputDataLines.Where(line => line.Contains('|')).ToArray();
			foreach (string rule in ruleData)
			{
				int[] rulePair = ParseIntArray(SplitString(rule, "|"));
				int earlierPage = rulePair[0];
				int laterPage = rulePair[1];
				
				if (!_pageRuleset.ContainsKey(earlierPage))
				{
					_pageRuleset.Add(earlierPage, new PageRules());
				}
				_pageRuleset[earlierPage].mustComeBeforePages.Add(laterPage);
				
				if (!_pageRuleset.ContainsKey(laterPage))
				{
					_pageRuleset.Add(laterPage, new PageRules());
				}
				_pageRuleset[laterPage].mustComeAfterPages.Add(earlierPage);
			}
		}

		private bool IsUpdateInCorrectOrder(int[] updatePages)
		{
			for (int thisPageIndex = 0; thisPageIndex < updatePages.Length; thisPageIndex++)
			{
				if (_pageRuleset.TryGetValue(updatePages[thisPageIndex], out PageRules thisPage))
				{
					for (int otherPageIndex = 0; otherPageIndex < updatePages.Length; otherPageIndex++)
					{
						if (thisPageIndex == otherPageIndex)
						{
							continue;
						}
							
						int otherPage = updatePages[otherPageIndex];
						if ((thisPageIndex < otherPageIndex && thisPage.mustComeAfterPages.Contains(otherPage)) ||
						    (thisPageIndex > otherPageIndex && thisPage.mustComeBeforePages.Contains(otherPage)))
						{
							return false;
						}
					}
				}
			}

			return true;
		}
	}
}