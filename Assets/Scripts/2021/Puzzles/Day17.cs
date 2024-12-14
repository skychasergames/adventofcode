using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace AoC2021
{
	public class Day17 : PuzzleBase
	{
		[SerializeField] private Transform _probe = null;
		[SerializeField] private Transform _trajectoryRendererParent = null;
		[SerializeField] private LineRenderer _trajectoryRendererPrefab = null;
		[SerializeField] private SpriteRenderer _targetAreaGrid = null;
		[SerializeField] private Color _colorHit = Color.yellow;
		[SerializeField] private Color _colorMiss = Color.gray;
	
		[SerializeField] private float _stepInterval = 0.5f;
		[SerializeField] private bool _isDebugMode = false;

		private BoundsInt _targetArea;

		private EditorCoroutine _executePuzzleCoroutine = null;
		private bool _advanceExecution = false;

		[Button]
		private void ResetGrid()
		{
			if (_executePuzzleCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
				_executePuzzleCoroutine = null;
			}
		
			while (_trajectoryRendererParent.childCount > 0)
			{
				DestroyImmediate(_trajectoryRendererParent.GetChild(0).gameObject);
			}
		
			_probe.localPosition = Vector3.zero;
			_targetAreaGrid.transform.localPosition = -_targetAreaGrid.transform.parent.localPosition;
			_targetAreaGrid.size = Vector2.one * 5;
		}

		[Button, ShowIf(nameof(_isDebugMode))]
		private void DebugAdvanceExecution()
		{
			_advanceExecution = true;
		}
	
		protected override void ExecutePuzzle1()
		{
			// Reset and initialize grid
			ResetGrid();
			InitializeGrid();
		
			// Execute puzzle!
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(false), this);
		}

		private void InitializeGrid()
		{
			// Parse puzzle input
			int[] data = ParseIntArray(
				_inputDataLines[0]									// -> "target area: x=20..30, y=-10..-5"
					.Split(':')[1].Trim(' ')						// -> "x=20..30,y=-10..-5"
					.Split(',')										// -> { "x=20..30", "y=-10..-5" }
					.Select(substring => substring.Split('=')[1])	// -> { "20..30", "-10..-5" }
					.SelectMany(substring => substring.Split(new [] { ".." }, StringSplitOptions.None))
					.ToArray()										// -> { "20", "30", "-10", "-5" }
			);

			// Calculate bounds and set target area grid position
			// Note: I've given the bounds a z depth, because it doesn't like it when the z size is 0 :shrug:
			_targetArea = new BoundsInt(
				xMin: data[0],
				yMin: data[2],
				zMin: -1, 
				sizeX: (data[1] - data[0]) + 1, // Values are max-inclusive, hence the +1
				sizeY: (data[3] - data[2]) + 1,
				sizeZ: 2
			);
		
			_targetAreaGrid.transform.localPosition = _targetArea.center;
			_targetAreaGrid.size = (Vector2Int)_targetArea.size;
		}

		private IEnumerator ExecutePuzzle(bool executeUntilSynchronized)
		{
			EditorWaitForSeconds interval = new EditorWaitForSeconds(_stepInterval);
		
			EditorApplication.QueuePlayerLoopUpdate();
			yield return WaitToAdvanceExecution(interval);

			int highestPointInSuccessfulLines = int.MinValue;
			int successfulLines = 0;
			List<int> possibleXSpeeds = GetPossibleXSpeeds();
			List<int> possibleYSpeeds = GetPossibleYSpeeds();
			foreach (int x in possibleXSpeeds)
			{
				foreach (int y in possibleYSpeeds)
				{
					bool hitTarget = PlotTrajectory(new Vector2Int(x, y), out List<Vector2Int> positions);
					VisualizeTrajectory(positions, hitTarget ? _colorHit : _colorMiss);

					if (hitTarget)
					{
						successfulLines++;

						int highestPointInThisLine = positions.Select(position => position.y).Max();
						if (highestPointInThisLine > highestPointInSuccessfulLines)
						{
							highestPointInSuccessfulLines = highestPointInThisLine;
						}
					}
				}

				EditorApplication.QueuePlayerLoopUpdate();
				yield return interval;
			}

			LogResult("Total lines", _trajectoryRendererParent.childCount);
			LogResult("Successful lines", successfulLines);
			LogResult("Highest point", highestPointInSuccessfulLines);
		
			EditorApplication.QueuePlayerLoopUpdate();
		}
	
		private IEnumerator WaitToAdvanceExecution(EditorWaitForSeconds interval)
		{
			if (_isDebugMode)
			{
				_advanceExecution = false;
				yield return new WaitUntil(() => _advanceExecution);
			}
			else
			{
				yield return interval;
			}
		}

		private bool PlotTrajectory(Vector2Int velocity, out List<Vector2Int> positions)
		{
			positions = new List<Vector2Int>();
			Vector2Int currentPosition = Vector2Int.zero;
			int maxX = _targetArea.xMax;
			int minY = _targetArea.yMin;
		
			while (true)
			{
				positions.Add(currentPosition);

				if (_targetArea.Contains(new Vector3Int(currentPosition.x, currentPosition.y, 0)))
				{
					return true;
				}
			
				if (currentPosition.x > maxX || currentPosition.y < minY)
				{
					return false;
				}

				// Move probe to new position
				currentPosition += velocity;

				// Apply drag and gravity
				velocity.x = (int)Mathf.MoveTowards(velocity.x, 0, 1);
				velocity.y--;
			}
		}

		private void VisualizeTrajectory(List<Vector2Int> positions, Color color)
		{
			LineRenderer trajectoryRenderer = Instantiate(_trajectoryRendererPrefab, _trajectoryRendererParent);
			Vector3[] positionsArray = new Vector3[positions.Count];
			for (int i = 0; i < positionsArray.Length; i++)
			{
				positionsArray[i] = new Vector3(positions[i].x, positions[i].y);
			}

			trajectoryRenderer.positionCount = positionsArray.Length;
			trajectoryRenderer.SetPositions(positionsArray);
			trajectoryRenderer.startColor = trajectoryRenderer.endColor = color;
		}
	
		// Logic for calculating possible X speeds
		//     x=0
		//      |
		//      v
		// ...............................
		// ........................TTTTTTT
		// ....S...................TTTTTTT
		// ........................TTTTTTT
		// n..6.....5....4...3..2.1#TTTTTT  <- If X < first column, then X must be >= the values on this speed curve (in this example, X must be >= 6).
		// ........................#######  <- Any X which falls within the target area columns is also valid (may land on first step). Any > last column is invalid because it would overshoot.
		// ...............................
	
		private List<int> GetPossibleXSpeeds()
		{
			List<int> possibleXSpeeds = new List<int>();
		
			int minX = 0;
			for (int distanceToTargetArea = _targetArea.xMin; distanceToTargetArea >= 0; distanceToTargetArea -= minX)
			{
				minX++;
			}
		
			for (int x = minX; x < _targetArea.xMax; x++)
			{
				possibleXSpeeds.Add(x);
			}

			return possibleXSpeeds;
		}
	
		// Logic for calculating possible Y speeds (...bear with me)
		// If Y < 0
		// ..............
		// ..S...........  <- y=0
		// ..........n...
		// ..........6...
		// ..........5...
		// ..........4...
		// ..TTT#TTTTTTTT
		// ..TTT#TTTTTTTT
		// ..TTT#TTTTTTTT
		// ..TTT#TTTT#TTT
		//      ^    ^
		//      |    '-- If Y > first row but < 0, then Y must be > the (negative) distance of the area (eg. -4).
		//      '------- Any Y which falls within the target area rows is also valid (may land on first step). Any < last row is invalid because it would overshoot.

		// If Y >= 0
		// ....................
		// ..0~1...............
		// .1...2..............
		// ....................
		// .2...3..............
		// .............0~1....
		// ............1...2...
		// .3...4..............
		// ............2...3...
		// ....................
		// ....................    
		// .SA..5......SB..4...  <- y=0  (note: SA=4, SB=3)
		// ....................     If Y > 0, the probe will reach y=0 again at velocity -(Y+1). (eg., if Y starts at 4 then when it reaches y=0 again it will be -5).
		// .............TTTTTTT     ...and when it does reach y=0, it must have Y velocity <= distance to the last row of the target area, or it will overshoot.
		// ..TTTTTTT....TTTTTTT
		// ..TTTTTTT....TTT#TTT
		// ..TTT#TTT...........
		// ....................
	
		// THEREFORE, Y must be >= last row of target area, and <= -(last row + 1)
	
		private List<int> GetPossibleYSpeeds()
		{
			List<int> possibleYSpeeds = new List<int>();

			int minY = _targetArea.yMin;
			int maxY = -(_targetArea.yMin + 1);
			for (int y = minY; y <= maxY; y++)
			{
				possibleYSpeeds.Add(y);
			}

			return possibleYSpeeds;
		}

		protected override void ExecutePuzzle2()
		{
		
		}
	}
}
