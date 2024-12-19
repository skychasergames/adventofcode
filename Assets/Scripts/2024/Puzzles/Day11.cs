using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AoC2024
{
	public class Day11 : PuzzleBase
	{
		[SerializeField] private int _blinks = 25;
		
		protected override void ExecutePuzzle1()
		{
			List<ulong> stones = ParseIntArray(_inputDataLines[0], " ").Select(intValue => (ulong)intValue).ToList();
			LogResult("Initial arrangement", string.Join(" ", stones));
			
			for (int blink = 0; blink < _blinks; blink++)
			{
				for (int i = stones.Count-1; i >= 0; i--)
				{
					TransformStone(ref stones, i);
				}
				
				LogResult("After " + (blink+1) + " blink", string.Join(" ", stones));
			}
			
			LogResult("Total stones", stones.Count);
		}

		protected override void ExecutePuzzle2()
		{
			
		}

		private void TransformStone(ref List<ulong> stones, int index)
		{
			ulong stone = stones[index];
			if (stone == 0)
			{
				stones[index] = 1;
			}
			else
			{
				string stoneString = stone.ToString();
				if (stoneString.Length % 2 == 0)
				{
					string leftStone = stoneString.Substring(0, stoneString.Length / 2);
					string rightStone = stoneString.Substring(stoneString.Length / 2);
					stones[index] = ulong.Parse(leftStone);
					stones.Insert(index+1, ulong.Parse(rightStone));
				}
				else
				{
					stones[index] *= 2024;
				}
			}
		}
	}
}
