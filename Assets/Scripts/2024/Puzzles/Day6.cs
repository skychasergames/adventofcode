using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;
using TMPro;
using Unity.EditorCoroutines.Editor;

namespace AoC2024
{
	public class Day6 : PuzzleBase
	{
		[SerializeField] private BoolGrid _map = null;
		[SerializeField] private TextMeshProUGUI _guard = null;
		[SerializeField] private float _intervalExample = 0.25f;
		[SerializeField] private float _intervalPuzzle = 0.01f;
		[SerializeField] private int _intervalSkipPuzzle = 10;
		
		[SerializeField] private Color _blockedPositionHighlight = Color.black;
		[SerializeField] private Color _visitedPositionHighlight = Color.yellow;
		
		private Vector2Int _guardPosition = new Vector2Int();
		private Direction _guardDirection;
		private int _guardDirectionIndex = -1;
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
			ResetMap();
			
			_map.Initialize(_inputDataLines, new[] { '#' }, new[] { '.', DIR_N, DIR_E, DIR_S, DIR_W });
			foreach (Vector2Int cell in _map.GetCoordsOfCellValue(true))
			{
				_map.HighlightCellView(cell, _blockedPositionHighlight);
			}
			
			LocateGuardOnMap();
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
		}

		protected override void ExecutePuzzle2()
		{
			
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
			
			_guardPosition = Vector2Int.zero;
			_guard.transform.position = Vector2.zero;
			_guardDirectionIndex = 0;
			_guardDirection = _directions[0];
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
						_guardPosition = new Vector2Int(x, y);
						_guardDirectionIndex = _directions.FindIndex(dir => c == dir.character);
						_guardDirection = _directions[_guardDirectionIndex];
						UpdateGuardView();
						return;
					}
				}
			}
		}

		private void MoveGuardForward()
		{
			_guardPosition += _guardDirection.vector;
			UpdateGuardView();
		}

		private void RotateGuardClockwise()
		{
			_guardDirectionIndex = (_guardDirectionIndex + 1) % _directions.Count;
			_guardDirection = _directions[_guardDirectionIndex];
			UpdateGuardView();
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

		private IEnumerator ExecutePuzzle()
		{
			EditorWaitForSeconds interval = new EditorWaitForSeconds(_isExample ? _intervalExample : _intervalPuzzle);

			HashSet<Vector2Int> positionsVisited = new HashSet<Vector2Int>();
			positionsVisited.Add(_guardPosition);
			_map.HighlightCellView(_guardPosition, _visitedPositionHighlight);

			yield return interval;

			int intervalsSkipped = 0;

			bool isGuardOnMap = true;
			while (isGuardOnMap)
			{
				bool isNextCellOnBoard = _map.CellExists(_guardPosition + _guardDirection.vector);
				if (isNextCellOnBoard)
				{
					bool isNextCellBlocked = _map.GetCellValue(_guardPosition + _guardDirection.vector);
					if (isNextCellBlocked)
					{
						RotateGuardClockwise();
					}
					else
					{
						MoveGuardForward();
						positionsVisited.Add(_guardPosition);
						_map.HighlightCellView(_guardPosition, _visitedPositionHighlight);

						if (_isExample)
						{
							yield return interval;
						}
						else
						{
							intervalsSkipped++;
							if (intervalsSkipped > _intervalSkipPuzzle)
							{
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
			

			LogResult("Total positions visited by guard", positionsVisited.Count);
			_executePuzzleCoroutine = null;
		}
	}
}
