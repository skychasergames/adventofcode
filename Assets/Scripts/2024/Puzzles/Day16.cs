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

		protected override void ExecutePuzzle1()
		{
			InitializeMap();
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
		}

		private IEnumerator ExecutePuzzle()
		{
			EditorWaitForSeconds interval = new EditorWaitForSeconds(_isExample ? _intervalExample : _intervalPuzzle);
			
			List<PathState> possiblePaths = CalculatePossiblePaths();
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
				}

				EditorApplication.QueuePlayerLoopUpdate();
				yield return interval;
			}
			LogResult("Lowest score", lowestScore);

			_executePuzzleCoroutine = null;
		}

		private List<PathState> CalculatePossiblePaths()
		{
			List<PathState> pathStates = new List<PathState> { _startingPathState };
			
			Step(_startingPathState);

			// Local method, recursive
			void Step(PathState currentPathState)
			{
				if (currentPathState.foundEndCell)
				{
					return;
				}
				
				List<(int directionIndex, Vector2Int position, char cell)> options = new List<(int directionIndex, Vector2Int position, char cell)>();
				foreach (int adjacentDirectionIndex in GetAdjacentDirectionIndexes(currentPathState.reindeerDirectionIndex))
				{
					Direction adjacentDirection = _directions[adjacentDirectionIndex];
					Vector2Int adjacentPosition = currentPathState.reindeerPosition + adjacentDirection.vector;
					char adjacentCell = _map.GetCellValue(adjacentPosition);
					if (adjacentCell != '#' && currentPathState.cellsVisited.All(cell => cell.Key != adjacentPosition))
					{
						options.Add((adjacentDirectionIndex, adjacentPosition, adjacentCell));
					}
				}

				switch (options.Count)
				{
				case 0:
					// Dead end
					pathStates.Remove(currentPathState);
					return;
				
				case 1:
					// One path
					PerformOption(currentPathState, options[0]);
					Step(currentPathState);
					break;
				
				default:
					// More than one path
					pathStates.Remove(currentPathState);
					foreach ((int directionIndex, Vector2Int position, char cell) option in options)
					{
						PathState newPathState = new PathState(currentPathState);
						pathStates.Add(newPathState);
						PerformOption(newPathState, option);
						Step(newPathState);
					}
					break;
				}
			}

			return pathStates;
		}
			
		private void PerformOption(PathState pathState, (int directionIndex, Vector2Int position, char cell) option)
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

		protected override void ExecutePuzzle2()
		{
			
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
	}
}
