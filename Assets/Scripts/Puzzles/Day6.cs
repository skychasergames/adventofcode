using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

public class Day6 : PuzzleBase
{
	[SerializeField] private int _timeToMature = 8;
	[SerializeField] private int _timeBetweenReproductions = 6;
	[SerializeField] private int _exampleIterations = 18;
	[SerializeField] private int _puzzleIterations = 80;
	
	private List<Lanternfish> _lanternfishes = new List<Lanternfish>();	// Note: I don't approve of the term "fishes" but it gets confusing otherwise

	public static int TimeBetweenReproductions { get; internal set; } // Static ref for lanternfish to access

	protected override void ExecutePuzzle1()
	{
		TimeBetweenReproductions = _timeBetweenReproductions;
		
		_lanternfishes.Clear();
		
		int[] initialValues = ParseIntArray(SplitString(_inputDataLines[0], ","));
		foreach (int initialValue in initialValues)
		{
			Lanternfish lanternfish = new Lanternfish(initialValue);
			_lanternfishes.Add(lanternfish);
		}

		int iterations = _isExample ? _exampleIterations : _puzzleIterations;
		for (int i = 0; i < iterations; i++)
		{
			List<Lanternfish> newLanternfishes = new List<Lanternfish>();
			foreach (Lanternfish lanternfish in _lanternfishes)
			{
				bool shouldReproduce = lanternfish.Tick();
				if (shouldReproduce)
				{
					Lanternfish newLanternfish = new Lanternfish(_timeToMature);
					newLanternfishes.Add(newLanternfish);
				}
			}
			_lanternfishes.AddRange(newLanternfishes);
		}

		LogResult("Total lanternfish", _lanternfishes.Count);
	}

	protected override void ExecutePuzzle2()
	{
		
	}

	public class Lanternfish
	{
		public int timer { get; private set; }

		public Lanternfish(int initialTimer)
		{
			timer = initialTimer;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>Returns true if a new lanterfish should be spawned.</returns>
		public bool Tick()
		{
			timer--;
			if (timer < 0)
			{
				timer = TimeBetweenReproductions;
				return true;
			}

			return false;
		}
	}
}
