using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

public class Day5 : PuzzleBase
{
	[SerializeField] private IntGrid _grid = null;
	[SerializeField] private int _gridSize = 10;
	[SerializeField] private Color[] _highlightColors = { Color.yellow, Color.red };
	
	protected override void ExecutePuzzle1()
	{
		// Initialize board
		ResetBoard();

		foreach (string line in _inputDataLines)
		{
			string[] coords = SplitString(line, " -> ");
			int[] startCoords = ParseIntArray(SplitString(coords[0], ","));
			int[] endCoords = ParseIntArray(SplitString(coords[1], ","));
			
			if (startCoords[0] == endCoords[0])
			{
				// Vertical line
				int x = startCoords[0];
				int startY = Mathf.Min(startCoords[1], endCoords[1]);
				int endY = Mathf.Max(startCoords[1], endCoords[1]);
				for (int y = startY; y <= endY; y++)
				{
					int cellValue = _grid.cells[x, y] + 1;
					_grid.SetCellValue(x, y, cellValue);
					_grid.HighlightCellView(x, y, _highlightColors[Mathf.Min(cellValue - 1, _highlightColors.Length - 1)]);
				}
			}
			else if (startCoords[1] == endCoords[1])
			{
				// Horizontal line
				int y = startCoords[1];
				int startX = Mathf.Min(startCoords[0], endCoords[0]);
				int endX = Mathf.Max(startCoords[0], endCoords[0]);
				for (int x = startX; x <= endX; x++)
				{
					int cellValue = _grid.cells[x, y] + 1;
					_grid.SetCellValue(x, y, cellValue);
					_grid.HighlightCellView(x, y, _highlightColors[Mathf.Min(cellValue - 1, _highlightColors.Length - 1)]);
				}
			}
		}

		int cellsWithOverlap = 0;
		foreach (int cellValue in _grid.cells)
		{
			if (cellValue > 1)
			{
				cellsWithOverlap++;
			}
		}
		
		LogResult("Cells with overlap", cellsWithOverlap);
	}

	[Button("Reset Board")]
	private void ResetBoard()
	{
		_grid.Initialize(_gridSize, _gridSize);
	}

	protected override void ExecutePuzzle2()
	{
		
	}
}
