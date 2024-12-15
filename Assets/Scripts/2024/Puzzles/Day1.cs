using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AoC2024
{
	public class Day1 : PuzzleBase
	{
		protected override void ExecutePuzzle1()
		{
			GetLocationLists(out List<int> leftList, out List<int> rightList);
			leftList.Sort();
			rightList.Sort();

			int[] distances = new int[leftList.Count];

			for (int i = 0; i < leftList.Count; i++)
			{
				distances[i] = Mathf.Abs(leftList[i] - rightList[i]);
				LogResult("Distance " + i, distances[i]);
			}

			int sum = distances.Sum();
			LogResult("Total distance", sum);
		}

		protected override void ExecutePuzzle2()
		{
			
		}

		private void GetLocationLists(out List<int> leftList, out List<int> rightList)
		{
			leftList = new List<int>();
			rightList = new List<int>();

			foreach (string line in _inputDataLines)
			{
				string[] ids = SplitString(line, "   ");
				
				if (int.TryParse(ids[0], out int leftID))
				{
					leftList.Add(leftID);
				}
				else
				{
					LogError("Error parsing leftID", leftID);
				}
				
				if (int.TryParse(ids[1], out int rightID))
				{
					rightList.Add(rightID);
				}
				else
				{
					LogError("Error parsing rightID", rightID);
				}
			}
		}
	}
}