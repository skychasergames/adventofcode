using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;
using TMPro;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace AoC2024
{
	public class Day16 : PuzzleBase
	{
		[SerializeField] private CharGrid _map = null;
		[SerializeField] private float _intervalExample = 0.5f;
		[SerializeField] private float _intervalPuzzle = 0.01f;
		
		[SerializeField] private Color _wallHighlight = Color.black;
		[SerializeField] private Color _startHighlight = new Color(0.0f, 0.6f, 1f);
		[SerializeField] private Color _endHighlight = Color.green;
		[SerializeField] private Color _pathHighlight = Color.yellow;
		
		private Vector2Int _startPosition = new Vector2Int();
		private Vector2Int _endPosition = new Vector2Int();

		private PathState _startingPathState = null;

		private EditorCoroutine _executePuzzleCoroutine = null;

		private const int MOVE_SCORE = +1;
		private const int TURN_SCORE = +1000;
		
		private const char DIR_N = '^';
		private const char DIR_E = '>';
		private const char DIR_S = 'v';
		private const char DIR_W = '<';
		
		private struct Direction
		{
			public char character;
			public Vector2Int vector;
		}
		
		private List<Direction> _directions = new List<Direction>
		{
			new Direction { character = DIR_N, vector = new Vector2Int(0, -1) },
			new Direction { character = DIR_E, vector = new Vector2Int(1, 0) },
			new Direction { character = DIR_S, vector = new Vector2Int(0, 1) },
			new Direction { character = DIR_W, vector = new Vector2Int(-1, 0) }
		};

		private List<int> GetAdjacentDirectionIndexes(int currentDirectionIndex)
		{
			List<int> adjacentDirections = new List<int>();
			for (int offset = -1; offset <= 1; offset++)
			{
				adjacentDirections.Add((int)Mathf.Repeat(currentDirectionIndex + offset, _directions.Count));
			}

			return adjacentDirections;
		}

		private class PathState
		{
			public Vector2Int reindeerPosition = new Vector2Int();
			public Direction reindeerDirection;
			public int reindeerDirectionIndex = -1;
			public Dictionary<Vector2Int, char> cellsVisited = new Dictionary<Vector2Int, char>();
			public int turnsMade = 0;
			public bool foundEndCell = false;

			public PathState() { }

			public PathState(PathState currentPathState)
			{
				reindeerPosition = currentPathState.reindeerPosition;
				reindeerDirection = currentPathState.reindeerDirection;
				reindeerDirectionIndex = currentPathState.reindeerDirectionIndex;
				cellsVisited = new Dictionary<Vector2Int, char>(currentPathState.cellsVisited);
				turnsMade = currentPathState.turnsMade;
				foundEndCell = false;
			}

			public int GetScore()
			{
				return (cellsVisited.Count * MOVE_SCORE) + (turnsMade * TURN_SCORE);
			}
		}

		private struct PathOption
		{
			public int directionIndex;
			public Vector2Int position;
			public char cell;

			public PathOption(int newDirectionIndex, Vector2Int newPosition, char newCell)
			{
				directionIndex = newDirectionIndex;
				position = newPosition;
				cell = newCell;
			}
		}

		protected override void ExecutePuzzle1()
		{
			InitializeMap();
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
		}

		protected override void ExecutePuzzle2()
		{
			InitializeMap();
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
		}

		private void InitializeMap()
		{
			ResetMap();
			
			_map.Initialize(_inputDataLines);

			for (int y = 0; y < _map.rows; y++)
			{
				for (int x = 0; x < _map.columns; x++)
				{
					char c = _map.GetCellValue(x, y);
					switch (c)
					{
					case '#':
						_map.HighlightCellView(x, y, _wallHighlight);
						break;
					
					case 'S':
						_startPosition = new Vector2Int(x, y);
						_map.HighlightCellView(_startPosition, _startHighlight);

						int startDirectionIndex = _directions.FindIndex(dir => dir.character == DIR_E);
						_startingPathState = new PathState
						{
							reindeerPosition = _startPosition,
							reindeerDirectionIndex = startDirectionIndex,
							reindeerDirection = _directions[startDirectionIndex]
						};
						break;
					
					case 'E':
						_endPosition = new Vector2Int(x, y);
						_map.HighlightCellView(_endPosition, _endHighlight);
						break;
					}
				}
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
			
			_startPosition = Vector2Int.zero;
			_endPosition = Vector2Int.zero;
			_startingPathState = null;
		}

		private IEnumerator ExecutePuzzle()
		{
			EditorWaitForSeconds interval = new EditorWaitForSeconds(_isExample ? _intervalExample : _intervalPuzzle);
			
			List<PathState> possiblePaths = CalculatePossiblePaths();
			List<PathState> lowestScorePaths = new List<PathState>();
			int lowestScore = int.MaxValue;

			foreach (PathState possiblePath in possiblePaths.Where(path => path.foundEndCell))
			{
				// Reset existing highlighted path if required
				char[] directionChars = _directions.Select(dir => dir.character).ToArray();
				for (int y = 0; y < _map.rows; y++)
				{
					for (int x = 0; x < _map.columns; x++)
					{
						if (directionChars.Contains(_map.GetCellValue(x, y)))
						{
							_map.SetCellValue(x, y, '.');
							_map.HighlightCellView(x, y, Color.white);
						}
					}
				}

				// Highlight this path
				foreach (KeyValuePair<Vector2Int, char> cellVisited in possiblePath.cellsVisited)
				{
					if (_map.GetCellValue(cellVisited.Key) == '.')
					{
						_map.SetCellValue(cellVisited.Key, cellVisited.Value);
						_map.HighlightCellView(cellVisited.Key, _pathHighlight);
					}
				}

				int pathScore = possiblePath.GetScore();
				LogResult("Score for path", pathScore);
				if (pathScore < lowestScore)
				{
					lowestScore = pathScore;
					lowestScorePaths.Clear();
					lowestScorePaths.Add(possiblePath);
				}
				else if (pathScore == lowestScore)
				{
					lowestScorePaths.Add(possiblePath);
				}

				EditorApplication.QueuePlayerLoopUpdate();
				yield return interval;
			}
			LogResult("Lowest score", lowestScore);

			HashSet<Vector2Int> positionsAlongLowestScorePaths = new HashSet<Vector2Int> { _startPosition };
			foreach (Vector2Int position in lowestScorePaths.SelectMany(path => path.cellsVisited.Keys))
			{
				positionsAlongLowestScorePaths.Add(position);

				if (_map.GetCellValue(position) != 'E')
				{
					_map.HighlightCellView(position, _pathHighlight);
				}
			}

			LogResult("Total positions along lowest score paths", positionsAlongLowestScorePaths.Count);
			EditorApplication.QueuePlayerLoopUpdate();

			_executePuzzleCoroutine = null;
		}

		private List<PathState> CalculatePossiblePaths()
		{
			List<PathState> completedPathStates = new List<PathState>();

			Queue<(PathState pathState, PathOption option)> pendingPathStates = new Queue<(PathState pathState, PathOption option)>();
			Dictionary<(Vector2Int position, int directionIndex), int> lowestScorePerPosition = new Dictionary<(Vector2Int position, int directionIndex),int>();

			foreach (PathOption pathOption in GetPathOptions(_startingPathState))
			{
				PathState newPathState = new PathState(_startingPathState);
				pendingPathStates.Enqueue((newPathState, pathOption));
			}

			while (pendingPathStates.Count > 0)
			{
				(PathState currentPathState, PathOption currentPathOption) = pendingPathStates.Dequeue();
				PerformOption(currentPathState, currentPathOption);

				// Check current path score against the previous lowest score
				int currentPathScore = currentPathState.GetScore();
				(Vector2Int position, int directionIndex) positionAndDirection = (currentPathState.reindeerPosition, currentPathState.reindeerDirectionIndex);
				if (lowestScorePerPosition.TryGetValue(positionAndDirection, out int currentPositionLowestScore))
				{
					if (currentPathScore <= currentPositionLowestScore)
					{
						// More efficient than, or equally efficient to, the previous score
						lowestScorePerPosition[positionAndDirection] = currentPathScore;
					}
					else
					{
						// Not the most efficient way to reach this position, abort now
						continue;
					}
				}
				else
				{
					// First time reaching this position
					lowestScorePerPosition.Add(positionAndDirection, currentPathScore);
				}

				if (currentPathState.foundEndCell)
				{
					completedPathStates.Add(currentPathState);
				}
				else
				{
					foreach (PathOption option in GetPathOptions(currentPathState))
					{
						PathState newPathState = new PathState(currentPathState);
						pendingPathStates.Enqueue((newPathState, option));
					}
				}
			}

			return completedPathStates;
		}

		private List<PathOption> GetPathOptions(PathState pathState)
		{
			List<PathOption> options = new List<PathOption>();
			foreach (int adjacentDirectionIndex in GetAdjacentDirectionIndexes(pathState.reindeerDirectionIndex))
			{
				Direction adjacentDirection = _directions[adjacentDirectionIndex];
				Vector2Int adjacentPosition = pathState.reindeerPosition + adjacentDirection.vector;
				char adjacentCell = _map.GetCellValue(adjacentPosition);
				if (adjacentCell != '#' && pathState.cellsVisited.All(cell => cell.Key != adjacentPosition))
				{
					options.Add(new PathOption(adjacentDirectionIndex, adjacentPosition, adjacentCell));
				}
			}

			return options;
		}
			
		private void PerformOption(PathState pathState, PathOption option)
		{
			if (option.directionIndex != pathState.reindeerDirectionIndex)
			{
				pathState.turnsMade++;
				pathState.reindeerDirectionIndex = option.directionIndex;
				pathState.reindeerDirection = _directions[pathState.reindeerDirectionIndex];
			}

			pathState.reindeerPosition += pathState.reindeerDirection.vector;
			pathState.cellsVisited.Add(pathState.reindeerPosition, pathState.reindeerDirection.character);

			if (option.cell == 'E')
			{
				pathState.foundEndCell = true;
			}
		}
	}
}
