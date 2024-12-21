using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace AoC2024
{
	public class Day14 : PuzzleBase
	{
		private const int EXAMPLE_MAP_WIDTH = 11;
		private const int EXAMPLE_MAP_HEIGHT = 7;
		private const int PUZZLE_MAP_WIDTH = 101;
		private const int PUZZLE_MAP_HEIGHT = 103;
		private const string REGEX_PATTERN = @"p=(?<posX>\d+),(?<posY>\d+) v=(?<velX>-?\d+),(?<velY>-?\d+)";

		[SerializeField] private IntGrid _map = null;
		[SerializeField] private Color _guardColor = Color.yellow;
		[SerializeField] private int _steps = 100;
		[SerializeField] private float _stepInterval = 0.5f;
		
		[SerializeField] private int _manualStepDirection = 1;

		private List<Guard> _guards = new List<Guard>();
		private EditorCoroutine _executePuzzleCoroutine = null;

		private int _currentStep = 0;

		protected override void ExecutePuzzle1()
		{
			InitializeMap();
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(_steps), this);
		}

		protected override void ExecutePuzzle2()
		{
			InitializeMap();
			_currentStep = 0;
			
			// Easter Egg first appears on step 6493 (I worked this out using Google Sheets).
			// There's a horizontal visual artifact that happens on step 4 and every 103 steps after,
			// and a vertical visual artifact that happens on step 29 and every 101 steps after.
			// These two visual artifacts finally converge on step 6493, resulting in the Christmas Tree appearing.
			StepSimulation(6493);
		}

		private class Guard
		{
			public Vector2Int position;
			public Vector2Int velocity;
		}

		private void InitializeMap()
		{
			ResetMap();
			
			_map.Initialize(_isExample ? EXAMPLE_MAP_WIDTH : PUZZLE_MAP_WIDTH, _isExample ? EXAMPLE_MAP_HEIGHT : PUZZLE_MAP_HEIGHT);

			Regex regex = new Regex(REGEX_PATTERN);
			
			_guards = new List<Guard>();
			foreach (string line in _inputDataLines)
			{
				Match match = regex.Match(line);
				Guard guard = new Guard
				{
					position = new Vector2Int(int.Parse(match.Result("${posX}")), int.Parse(match.Result("${posY}"))),
					velocity = new Vector2Int(int.Parse(match.Result("${velX}")), int.Parse(match.Result("${velY}")))
				};

				_guards.Add(guard);
				_map.IncrementCellValue(guard.position);
				_map.HighlightCellView(guard.position, _guardColor);
			}
		}
		
		[Button("Reset Map")]
		private void ResetMap()
		{
			if (_executePuzzleCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
				_executePuzzleCoroutine = null;
			}
			
			_map.ClearCellViews();
			_guards.Clear();
		}

		private IEnumerator ExecutePuzzle(int steps)
		{
			EditorWaitForSeconds interval = new EditorWaitForSeconds(_stepInterval);
			
			for (int step = 0; step < steps; step++)
			{
				yield return interval;
				
				LogResult("Step", step);
				StepSimulation();
			}

			int safetyFactor = GetSafetyFactor();
			LogResult("Safety factor", safetyFactor);
		}

		private int GetSafetyFactor()
		{
			int quadrantUL = GetNumberOfGuardsInArea(0, 0, Mathf.FloorToInt(_map.columns / 2f), Mathf.FloorToInt(_map.rows / 2f));
			int quadrantUR = GetNumberOfGuardsInArea(Mathf.CeilToInt(_map.columns / 2f), 0, _map.columns, Mathf.FloorToInt(_map.rows / 2f));
			int quadrantLL = GetNumberOfGuardsInArea(0, Mathf.CeilToInt(_map.rows / 2f), Mathf.FloorToInt(_map.columns / 2f), _map.rows);
			int quadrantLR = GetNumberOfGuardsInArea(Mathf.CeilToInt(_map.columns / 2f), Mathf.CeilToInt(_map.rows / 2f), _map.columns, _map.rows);
			return quadrantUL * quadrantUR * quadrantLL * quadrantLR;

			int GetNumberOfGuardsInArea(int minX, int minY, int maxX, int maxY)
			{
				int guards = 0;
				for (int y = minY; y < maxY; y++)
				{
					for (int x = minX; x < maxX; x++)
					{
						guards += _map.GetCellValue(x, y);
					}
				}

				return guards;
			}
		}

		private void StepSimulation(int step = 1)
		{
			foreach (Guard guard in _guards)
			{
				_map.DecrementCellValue(guard.position);
				if (_map.GetCellValue(guard.position) == 0)
				{
					_map.HighlightCellView(guard.position, Color.white);
				}

				//Log("Moved guard from " + guard.position + " by " + guard.velocity);
				guard.position += step * guard.velocity;
					
				// Teleport (wrap around) if necessary
				while (guard.position.x < 0)
				{
					guard.position.x += _map.columns;
				}
				
				while (guard.position.x >= _map.columns)
				{
					guard.position.x -= _map.columns;
				}
					
				while (guard.position.y < 0)
				{
					guard.position.y += _map.rows;
				}
				
				while (guard.position.y >= _map.rows)
				{
					guard.position.y -= _map.rows;
				}
					
				//LogResult("to", guard.position);
					
				_map.IncrementCellValue(guard.position);
				_map.HighlightCellView(guard.position, _guardColor);
			}
				
			EditorApplication.QueuePlayerLoopUpdate();
		}

		[Button("Manual Step")]
		private void ManualStep()
		{
			_currentStep += _manualStepDirection;
			LogResult("Step", _currentStep);
			StepSimulation(_manualStepDirection);
		}
	}
}
