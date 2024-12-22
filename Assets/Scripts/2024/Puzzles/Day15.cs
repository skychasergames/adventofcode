using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
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
		private const char BOX_L = '[';
		private const char BOX_R = ']';
		private const char WALL = '#';
		private const char SPACE = '.';
		
		private Dictionary<char, Vector2Int> _directions = new Dictionary<char, Vector2Int>
		{
			{ DIR_N, new Vector2Int(0, -1) },
			{ DIR_E, new Vector2Int(1, 0) },
			{ DIR_S, new Vector2Int(0, 1) },
			{ DIR_W, new Vector2Int(-1, 0) }
		};

		protected override void ExecutePuzzle1()
		{
			InitializeMap(false);
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
		}

		protected override void ExecutePuzzle2()
		{
			InitializeMap(true);
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
		}
		
		private void InitializeMap(bool doubleWidth)
		{
			ResetMap();
			
			_map.Initialize(_inputDataLines.Where(s => s[0] == WALL).Select(s => doubleWidth ? ConvertToDoubleWidth(s) : s).ToArray());
			
			foreach (Vector2Int wallCell in _map.GetCoordsOfCellValue(WALL))
			{
				_map.HighlightCellView(wallCell, _wallHighlight);
			}
			
			foreach (Vector2Int boxCell in _map.GetCoordsOfCellValue(BOX).Union(_map.GetCoordsOfCellValue(BOX_L)).Union(_map.GetCoordsOfCellValue(BOX_R)))
			{
				_map.HighlightCellView(boxCell, _boxHighlight);
			}

			_robotPosition = _map.GetCoordsOfCellValue(ROBOT)[0];
			_map.HighlightCellView(_robotPosition, _robotHighlight);
		}

		private string ConvertToDoubleWidth(string singleWidthString)
		{
			StringBuilder doubleWidthString = new StringBuilder();
			foreach (char c in singleWidthString)
			{
				switch (c)
				{
				case ROBOT:
					doubleWidthString.Append(ROBOT).Append(SPACE);
					break;
				
				case BOX:
					doubleWidthString.Append(BOX_L).Append(BOX_R);
					break;
				
				case WALL:
					doubleWidthString.Append(WALL).Append(WALL);
					break;
				
				case SPACE:
					doubleWidthString.Append(SPACE).Append(SPACE);
					break;
				
				default:
					throw new ArgumentOutOfRangeException("Unhandled char: " + c);
				}
			}

			return doubleWidthString.ToString();
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

		private IEnumerator ExecutePuzzle()
		{
			EditorWaitForSeconds interval = new EditorWaitForSeconds(_isExample ? _intervalExample : _intervalPuzzle);
			
			foreach (char directionChar in _inputDataLines.Where(s => _directions.Keys.Contains(s[0])).SelectMany(s => s))
			{
				yield return interval;
				if (CanRobotMoveInDirection(directionChar, out Vector2Int direction, out List<Vector2Int> boxesToMove, out List<Vector2Int> emptySpaceCells))
				{
					MoveRobotAndBoxesInDirection(directionChar, direction, boxesToMove, emptySpaceCells);
				}

				EditorApplication.QueuePlayerLoopUpdate();
			}

			int totalGPS = CalculateGPS();
			LogResult("Total GPS", totalGPS);
			
			_executePuzzleCoroutine = null;
		}
		
		private bool CanRobotMoveInDirection(char directionChar, out Vector2Int direction, out List<Vector2Int> boxesToMove, out List<Vector2Int> emptySpaceCells)
		{
			// Set robot rotation (text)
			_map.SetCellValue(_robotPosition, directionChar);
			
			// Determine if robot can move, and if so which boxes to move

			bool canRobotAndAllBoxesMove = true;
			Vector2Int dir = _directions[directionChar];
			List<Vector2Int> tempBoxesToMove = new List<Vector2Int>();
			List<Vector2Int> tempEmptySpaceCells = new List<Vector2Int>();
			
			CheckCell(_robotPosition + dir);

			// Local method, recursive
			void CheckCell(Vector2Int cell)
			{
				if (_map.CellExists(cell))
				{
					char nextCellValue = _map.GetCellValue(cell);
					switch (nextCellValue)
					{
					case SPACE:
						tempEmptySpaceCells.Add(cell);
						return;
				
					case BOX:
						tempBoxesToMove.Add(cell);
						CheckCell(cell + dir);
						break;
					
					case BOX_L:
						Vector2Int rightCell = cell + new Vector2Int(1, 0);

						if (!tempBoxesToMove.Contains(cell))
						{
							tempBoxesToMove.Add(cell);
							CheckCell(cell + dir);
						}

						if (!tempBoxesToMove.Contains(rightCell))
						{
							tempBoxesToMove.Add(rightCell);
							CheckCell(rightCell + dir);
						}
						break;
					
					case BOX_R:
						Vector2Int leftCell = cell + new Vector2Int(-1, 0);

						if (!tempBoxesToMove.Contains(cell))
						{
							tempBoxesToMove.Add(cell);
							CheckCell(cell + dir);
						}

						if (!tempBoxesToMove.Contains(leftCell))
						{
							tempBoxesToMove.Add(leftCell);
							CheckCell(leftCell + dir);
						}
						break;
				
					case WALL:
						canRobotAndAllBoxesMove = false;
						return;
				
					default:
						throw new ArgumentOutOfRangeException("Unhandled char: " + nextCellValue);
					}
				}
			}

			direction = dir;
			boxesToMove = tempBoxesToMove;
			emptySpaceCells = tempEmptySpaceCells;
			return canRobotAndAllBoxesMove;
		}
		
		private void MoveRobotAndBoxesInDirection(char directionChar, Vector2Int direction, List<Vector2Int> boxesToMove, List<Vector2Int> emptySpaceCells)
		{
			_map.SetCellValue(_robotPosition, SPACE);
			_map.HighlightCellView(_robotPosition, Color.white);
			
			if (boxesToMove.Count > 0)
			{
				//LogResult("Empty cells", string.Join(", ", emptySpaceCells));
				foreach (Vector2Int emptySpaceCell in emptySpaceCells)
				{
					// Crawl backwards from each empty space cell, moving boxes as we go
					int i = 0;
					while (true)
					{
						Vector2Int moveFromCell = emptySpaceCell - direction * (i + 1);
						Vector2Int moveToCell = emptySpaceCell - direction * i;
						if (boxesToMove.Contains(moveFromCell))
						{
							char boxChar = _map.GetCellValue(moveFromCell);
							//Log("Moving " + boxChar + " from " + moveFromCell + " to " + moveToCell);
							_map.SetCellValue(moveToCell, boxChar);
							_map.HighlightCellView(moveToCell, _boxHighlight);
							i++;
						}
						else
						{
							// Moved all boxes, set the last cell to empty
							//Log("Clearing last cell at " + moveFromCell);
							_map.SetCellValue(moveToCell, SPACE);
							_map.HighlightCellView(moveToCell, Color.white);
							break;
						}
					}
				}
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
					char cell = _map.GetCellValue(x, y);
					if (cell == BOX || cell == BOX_L)
					{
						int boxGPS = x + 100 * y;
						totalGPS += boxGPS;
					}
				}
			}

			return totalGPS;
		}

		[Button("Debug Play Map")]
		private void DebugPlayMap()
		{
			InitializeMap(false);
		}

		[Button("Debug Play Map (double-width)")]
		private void DebugPlayMapDoubleWidth()
		{
			InitializeMap(true);
		}

		[Button("Debug Move Up")]
		private void DebugMoveUp()
		{
			DebugMove(DIR_N);
		}

		[Button("Debug Move Down")]
		private void DebugMoveDown()
		{
			DebugMove(DIR_S);
		}

		[Button("Debug Move Left")]
		private void DebugMoveLeft()
		{
			DebugMove(DIR_W);
		}

		[Button("Debug Move Right")]
		private void DebugMoveRight()
		{
			DebugMove(DIR_E);
		}

		private void DebugMove(char directionChar)
		{
			if (CanRobotMoveInDirection(directionChar, out Vector2Int direction, out List<Vector2Int> boxesToMove, out List<Vector2Int> emptySpaceCells))
			{
				MoveRobotAndBoxesInDirection(directionChar, direction, boxesToMove, emptySpaceCells);
			}

			EditorApplication.QueuePlayerLoopUpdate();
		}
	}
}
