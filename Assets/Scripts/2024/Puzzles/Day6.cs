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
	public class Day6 : PuzzleBase
	{
		[SerializeField] private BoolGrid _map = null;
		[SerializeField] private TextMeshProUGUI _guard = null;
		[SerializeField] private float _moveIntervalExample = 0.25f;
		[SerializeField] private float _moveIntervalPuzzle = 0.01f;
		[SerializeField] private int _moveIntervalSkipPuzzle = 100;
		[SerializeField] private float _obstructionIntervalExample = 1f;
		[SerializeField] private float _obstructionIntervalPuzzle = 0.5f;
		
		[SerializeField] private Color _obstructionHighlight = Color.black;
		[SerializeField] private Color _visitedPositionHighlight = Color.yellow;
		
		[SerializeField] private Color _potentialObstructionHighlight = Color.grey;
		[SerializeField] private Color _potentialVisitedPositionHighlight = new Color(1.0f, 0.6f, 0.0f);
		
		private Vector2Int _guardStartPosition = new Vector2Int();
		private Direction _guardStartDirection;
		private int _guardStartDirectionIndex = -1;
		
		private Vector2Int _guardPosition = new Vector2Int();
		private Direction _guardDirection;
		private int _guardDirectionIndex = -1;
		private HashSet<Vector2Int> _positionsVisited = new HashSet<Vector2Int>();
		private HashSet<Vector2Int> _potentialPositionsVisited = new HashSet<Vector2Int>();

		private int _guardLoopObstructionPositionsFound = 0;
		
		private EditorCoroutine _executePuzzleCoroutine = null;

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

		protected override void ExecutePuzzle1()
		{
			InitializeMap();
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle1Process(), this);
		}

		private IEnumerator ExecutePuzzle1Process()
		{
			yield return PlotGuardPatrolRoute();
			
			LogResult("Total positions visited by guard", _positionsVisited.Count);
			_executePuzzleCoroutine = null;
		}

		protected override void ExecutePuzzle2()
		{
			InitializeMap();

			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle2Process(), this);
		}

		private IEnumerator ExecutePuzzle2Process()
		{
			yield return PlotGuardPatrolRoute();
			
			EditorWaitForSeconds obstructionInterval = new EditorWaitForSeconds(_isExample ? _obstructionIntervalExample : _obstructionIntervalPuzzle);

			Log("Guard route plotted");

			// Identify potential obstruction locations which cause the guard to get stuck in a loop
			_guardLoopObstructionPositionsFound = 0;
			foreach (Vector2Int potentialObstructionPosition in _positionsVisited.Where(potentialObstructionPosition => potentialObstructionPosition != _guardStartPosition))
			{
				LogResult("Testing obstruction position", potentialObstructionPosition);
				_map.SetCellValue(potentialObstructionPosition, true);
				_map.HighlightCellView(potentialObstructionPosition, _potentialObstructionHighlight);

				yield return PlotPotentialGuardRoute(potentialObstructionPosition);
				
				EditorApplication.QueuePlayerLoopUpdate();
				yield return obstructionInterval;

				// Restore state of cell highlights
				_map.SetCellValue(potentialObstructionPosition, false);
				foreach (Vector2Int potentialVisitedPosition in _potentialPositionsVisited)
				{
					_map.HighlightCellView(potentialVisitedPosition, Color.white);
				}
				foreach (Vector2Int originalVisitedPosition in _positionsVisited)
				{
					_map.HighlightCellView(originalVisitedPosition, _visitedPositionHighlight);
				}
			}
			
			LogResult("Total potential obstructions that cause guard to loop", _guardLoopObstructionPositionsFound);
			_executePuzzleCoroutine = null;
		}

		private void InitializeMap()
		{
			ResetMap();
			
			_map.Initialize(_inputDataLines, new[] { '#' }, new[] { '.', DIR_N, DIR_E, DIR_S, DIR_W });
			foreach (Vector2Int cell in _map.GetCoordsOfCellValue(true))
			{
				_map.HighlightCellView(cell, _obstructionHighlight);
			}
			
			LocateGuardOnMap();
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
			
			_guardStartPosition = _guardPosition = Vector2Int.zero;
			_guardStartDirectionIndex = _guardDirectionIndex = 0;
			_guardStartDirection = _guardDirection = _directions[0];
			_guard.transform.position = Vector2.zero;
			_guard.SetText("^");
		}

		private void LocateGuardOnMap()
		{
			char[] directionChars = _directions.Select(dir => dir.character).ToArray();
			for (int y = 0; y < _inputDataLines.Length; y++)
			{
				for (int x = 0; x < _inputDataLines[0].Length; x++)
				{
					char c = _inputDataLines[y][x];
					if (directionChars.Contains(c))
					{
						_guardStartPosition = _guardPosition = new Vector2Int(x, y);
						_guardStartDirectionIndex = _guardDirectionIndex = _directions.FindIndex(dir => c == dir.character);
						_guardStartDirection = _guardDirection = _directions[_guardDirectionIndex];
						UpdateGuardView();
						return;
					}
				}
			}
		}

		private void UpdateGuardView()
		{
			// Set guard position
			CellView guardCell = _map.GetCellView(_guardPosition);
			if (guardCell != null)
			{
				_guard.transform.position = guardCell.transform.position;
			}

			// Set guard rotation (text)
			_guard.text = _guardDirection.character.ToString();
		}

		private IEnumerator PlotGuardPatrolRoute()
		{
			EditorWaitForSeconds interval = new EditorWaitForSeconds(_isExample ? _moveIntervalExample : _moveIntervalPuzzle);

			_positionsVisited.Clear();
			_positionsVisited.Add(_guardPosition);
			_map.HighlightCellView(_guardPosition, _visitedPositionHighlight);

			yield return interval;

			int intervalsSkipped = 0;

			bool isGuardOnMap = true;
			while (isGuardOnMap)
			{
				Vector2Int nextPosition = _guardPosition + _guardDirection.vector;
				bool isNextCellOnBoard = _map.CellExists(nextPosition);
				if (isNextCellOnBoard)
				{
					bool isNextCellBlocked = _map.GetCellValue(nextPosition);
					if (isNextCellBlocked)
					{
						// Rotate guard clockwise
						_guardDirectionIndex = (_guardDirectionIndex + 1) % _directions.Count;
						_guardDirection = _directions[_guardDirectionIndex];
					}
					else
					{
						// Move guard forward
						_guardPosition += _guardDirection.vector;
						_positionsVisited.Add(_guardPosition);
						_map.HighlightCellView(_guardPosition, _visitedPositionHighlight);

						if (_isExample)
						{
							UpdateGuardView();
							yield return interval;
						}
						else
						{
							intervalsSkipped++;
							if (intervalsSkipped > _moveIntervalSkipPuzzle)
							{
								UpdateGuardView();
								intervalsSkipped = 0;
								yield return interval;
							}
						}
					}
				}
				else
				{
					isGuardOnMap = false;
				}
			}
			
			UpdateGuardView();
		}

		private IEnumerator PlotPotentialGuardRoute(Vector2Int potentialObstructionPosition)
		{
			EditorWaitForSeconds interval = new EditorWaitForSeconds(_isExample ? _moveIntervalExample : _moveIntervalPuzzle);

			_guardPosition = _guardStartPosition;
			_guardDirectionIndex = _guardStartDirectionIndex;
			_guardDirection = _guardStartDirection;
			UpdateGuardView();
			
			HashSet<(Vector2Int position, char dir)> obstructionsEncountered = new HashSet<(Vector2Int, char)>();
			
			_potentialPositionsVisited.Clear();
			_potentialPositionsVisited.Add(_guardPosition);
			_map.HighlightCellView(_guardPosition, _potentialVisitedPositionHighlight);

			yield return interval;

			int intervalsSkipped = 0;

			bool isGuardOnMap = true;
			while (isGuardOnMap)
			{
				Vector2Int nextPosition = _guardPosition + _guardDirection.vector;
				bool isNextCellOnBoard = _map.CellExists(nextPosition);
				if (isNextCellOnBoard)
				{
					bool isNextCellBlocked = _map.GetCellValue(nextPosition);
					if (isNextCellBlocked)
					{
						// Encountered obstruction
						bool isNewObstruction = obstructionsEncountered.Add((nextPosition, _guardDirection.character));
						if (!isNewObstruction)
						{
							// Encountered the same obstruction from the same direction again -- guard is a caught in a loop!
							_guardLoopObstructionPositionsFound++;
							LogResult("Loop found with obstruction added at", potentialObstructionPosition);
							break;
						}
						
						// Rotate guard clockwise
						_guardDirectionIndex = (_guardDirectionIndex + 1) % _directions.Count;
						_guardDirection = _directions[_guardDirectionIndex];
					}
					else
					{
						// Move guard forward
						_guardPosition += _guardDirection.vector;
						_potentialPositionsVisited.Add(_guardPosition);
						_map.HighlightCellView(_guardPosition, _potentialVisitedPositionHighlight);

						if (_isExample)
						{
							UpdateGuardView();
							yield return interval;
						}
						else
						{
							intervalsSkipped++;
							if (intervalsSkipped > _moveIntervalSkipPuzzle)
							{
								UpdateGuardView();
								intervalsSkipped = 0;
								yield return interval;
							}
						}
					}
				}
				else
				{
					isGuardOnMap = false;
				}
			}

			UpdateGuardView();
		}
	}
}
