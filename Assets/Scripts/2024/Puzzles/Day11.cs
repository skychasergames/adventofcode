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
			Dictionary<ulong, ulong> stoneCounts = new Dictionary<ulong, ulong>();
			foreach (ulong stone in ParseIntArray(_inputDataLines[0], " ").Select(intValue => (ulong)intValue))
			{
				if (!stoneCounts.ContainsKey(stone))
				{
					stoneCounts.AddIfUnique<ulong, ulong>(stone, 0);
					stoneCounts[stone]++;
				}
			}
			
			LogResult("Initial arrangement", string.Join(" ", stoneCounts));

			for (int blink = 0; blink < _blinks; blink++)
			{
				Dictionary<ulong, ulong> newStoneCounts = new Dictionary<ulong, ulong>();
				foreach (KeyValuePair<ulong, ulong> stoneCount in stoneCounts)
				{
					List<ulong> newStones = TransformStone(stoneCount.Key);
					foreach (ulong newStone in newStones)
					{
						newStoneCounts.AddIfUnique<ulong, ulong>(newStone, 0);
						newStoneCounts[newStone] += stoneCount.Value;
					}
				}

				stoneCounts = newStoneCounts;
				
				LogResult("After " + (blink+1) + " blink", string.Join(" ", stoneCounts));
			}
			
			LogResult("Total stones", stoneCounts.Sum(pair => pair.Value));
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

		private List<ulong> TransformStone(ulong stone)
		{
			List<ulong> newStones = new List<ulong>();
			if (stone == 0)
			{
				newStones.Add(1);
			}
			else
			{
				string stoneString = stone.ToString();
				if (stoneString.Length % 2 == 0)
				{
					string leftStone = stoneString.Substring(0, stoneString.Length / 2);
					string rightStone = stoneString.Substring(stoneString.Length / 2);
					newStones.Add(ulong.Parse(leftStone));
					newStones.Add(ulong.Parse(rightStone));
				}
				else
				{
					newStones.Add(stone * 2024);
				}
			}

			return newStones;
		}
	}
}
