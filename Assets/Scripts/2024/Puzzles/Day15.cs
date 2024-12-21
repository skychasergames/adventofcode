using System;
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
	public class Day15 : PuzzleBase
	{
		[SerializeField] private CharGrid _map = null;
		[SerializeField] private float _intervalExample = 1f;
		[SerializeField] private float _intervalPuzzle = 0.01f;
		
		[SerializeField] private Color _wallHighlight = Color.black;
		[SerializeField] private Color _boxHighlight = new Color(0.7f, 0.5f, 0.1f);
		[SerializeField] private Color _robotHighlight = new Color(0.0f, 0.6f, 1.0f);
		
		private Vector2Int _robotPosition = new Vector2Int();
		
		private EditorCoroutine _executePuzzleCoroutine = null;

		private const char DIR_N = '^';
		private const char DIR_E = '>';
		private const char DIR_S = 'v';
		private const char DIR_W = '<';
		private const char ROBOT = '@';
		private const char BOX = 'O';
		private const char WALL = '#';
		private const char SPACE = '.';
		
		private struct Direction
		{
			public char character;
			public Vector2Int vector;
		}
		
		private Dictionary<char, Vector2Int> _directions = new Dictionary<char, Vector2Int>
		{
			{ DIR_N, new Vector2Int(0, -1) },
			{ DIR_E, new Vector2Int(1, 0) },
			{ DIR_S, new Vector2Int(0, 1) },
			{ DIR_W, new Vector2Int(-1, 0) }
		};

		protected override void ExecutePuzzle1()
		{
			InitializeMap();
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
		}

		private IEnumerator ExecutePuzzle()
		{
			EditorWaitForSeconds interval = new EditorWaitForSeconds(_isExample ? _intervalExample : _intervalPuzzle);
			
			foreach (char directionChar in _inputDataLines.Where(s => _directions.Keys.Contains(s[0])).SelectMany(s => s))
			{
				yield return interval;
				if (CanRobotMoveInDirection(directionChar, out Vector2Int direction, out List<Vector2Int> boxesToMove))
				{
					MoveRobotAndBoxesInDirection(directionChar, direction, boxesToMove);
				}

				EditorApplication.QueuePlayerLoopUpdate();
			}

			int totalGPS = CalculateGPS();
			LogResult("Total GPS", totalGPS);
			
			_executePuzzleCoroutine = null;
		}

		protected override void ExecutePuzzle2()
		{
			
		}

		private void InitializeMap()
		{
			ResetMap();
			
			_map.Initialize(_inputDataLines.Where(s => s[0] == WALL).ToArray());
			
			foreach (Vector2Int wallCell in _map.GetCoordsOfCellValue(WALL))
			{
				_map.HighlightCellView(wallCell, _wallHighlight);
			}
			
			foreach (Vector2Int boxCell in _map.GetCoordsOfCellValue(BOX))
			{
				_map.HighlightCellView(boxCell, _boxHighlight);
			}

			_robotPosition = _map.GetCoordsOfCellValue(ROBOT)[0];
			_map.HighlightCellView(_robotPosition, _robotHighlight);
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
		}

		private bool CanRobotMoveInDirection(char directionChar, out Vector2Int direction, out List<Vector2Int> boxesToMove)
		{
			// Set robot rotation (text)
			_map.SetCellValue(_robotPosition, directionChar);
			
			// Determine if robot can move, and if so which boxes to move
			direction = _directions[directionChar];
			boxesToMove = new List<Vector2Int>();
			Vector2Int checkCell = _robotPosition + direction;
			while (_map.CellExists(checkCell))
			{
				char nextCellValue = _map.GetCellValue(checkCell);
				switch (nextCellValue)
				{
				case SPACE:
					return true;
				
				case BOX:
					boxesToMove.Add(checkCell);
					checkCell += direction;
					break;
				
				case WALL:
					return false;
				
				default:
					throw new ArgumentOutOfRangeException("Unhandled char: " + nextCellValue);
				}
			}
			
			// Somehow reached edge of map?
			return false;
		}
		
		private void MoveRobotAndBoxesInDirection(char directionChar, Vector2Int direction, List<Vector2Int> boxesToMove)
		{
			_map.SetCellValue(_robotPosition, SPACE);
			_map.HighlightCellView(_robotPosition, Color.white);

			if (boxesToMove.Count > 0)
			{
				Vector2Int emptySpaceCell = boxesToMove[boxesToMove.Count-1] + direction;
				_map.SetCellValue(emptySpaceCell, BOX);
				_map.HighlightCellView(emptySpaceCell, _boxHighlight);
			}

			_robotPosition += direction;
			_map.SetCellValue(_robotPosition, directionChar);
			_map.HighlightCellView(_robotPosition, _robotHighlight);
		}

		private int CalculateGPS()
		{
			int totalGPS = 0;
			for (int y = 0; y < _map.rows; y++)
			{
				for (int x = 0; x < _map.columns; x++)
				{
					if (_map.GetCellValue(x, y) == BOX)
					{
						int boxGPS = x + 100 * y;
						totalGPS += boxGPS;
					}
				}
			}

			return totalGPS;
		}
	}
}
