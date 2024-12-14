using System.Collections.Generic;
using UnityEngine;

public class Day6 : PuzzleBase
{
	[SerializeField] private int _timeToMature = 8;
	[SerializeField] private int _timeBetweenReproductions = 6;
	[SerializeField] private int _exampleIterations = 18;
	[SerializeField] private int _puzzle1Iterations = 80;
	[SerializeField] private int _puzzle2Iterations = 256;
	
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

		int iterations = _isExample ? _exampleIterations : _puzzle1Iterations;
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
	
	private long[] _lanternfishesByLifecycleStage;

	protected override void ExecutePuzzle2()
	{
		_lanternfishesByLifecycleStage = new long[_timeToMature + 1];
		
		int[] initialValues = ParseIntArray(SplitString(_inputDataLines[0], ","));
		foreach (int initialValue in initialValues)
		{
			_lanternfishesByLifecycleStage[initialValue]++;
		}

		for (int i = 0; i < _puzzle2Iterations; i++)
		{
			long reproducingLanternfishes = _lanternfishesByLifecycleStage[0];
			
			// Age all lanternfishes
			for (int stage = 1; stage <= _timeToMature; stage++)
			{
				_lanternfishesByLifecycleStage[stage - 1] = _lanternfishesByLifecycleStage[stage];
			}

			// Reset lifecycle stage for reproducing lanternfishes, and spawn new lanternfishes
			_lanternfishesByLifecycleStage[_timeBetweenReproductions] += reproducingLanternfishes;	// Add because there might be juveniles that are already at this lifecycle stage
			_lanternfishesByLifecycleStage[_timeToMature] = reproducingLanternfishes;
		}

		long totalLanternfishes = 0;
		foreach (long lanternfishes in _lanternfishesByLifecycleStage)
		{
			totalLanternfishes += lanternfishes;
		}

		LogResult("Total lanternfish", totalLanternfishes);
	}
}
