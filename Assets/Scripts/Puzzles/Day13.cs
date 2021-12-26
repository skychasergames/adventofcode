using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

public class Day13 : PuzzleBase
{
	[SerializeField] private BoolGrid _grid = null;
	[SerializeField] private int _exampleGridWidth = 11;
	[SerializeField] private int _exampleGridHeight = 15;
	[SerializeField] private int _puzzleGridWidth = 1000;
	[SerializeField] private int _puzzleGridHeight = 1000;
	[SerializeField] private Color _highlightColorDot = Color.white;
	[SerializeField] private float _foldInterval = 0.5f;
	
	private EditorCoroutine _executePuzzleCoroutine = null;

	[Button]
	private void CalculateGridSize()
	{
		CalculateGridSize(_exampleData, out _exampleGridWidth, out _exampleGridHeight);
		CalculateGridSize(_puzzleData, out _puzzleGridWidth, out _puzzleGridHeight);
		
		void CalculateGridSize(TextAsset data, out int gridWidth, out int gridHeight)
		{
			ParseInputData(data);

			gridWidth = _inputDataLines.Where(line => char.IsDigit(line[0]))
				.Select(line => int.Parse(SplitString(line, ",")[0]))
				.Max() + 1;

			gridHeight = _inputDataLines.Where(line => char.IsDigit(line[0]))
				.Select(line => int.Parse(SplitString(line, ",")[1]))
				.Max() + 1;
		}
	}

	[Button]
	private void ResetGrid()
	{
		_grid.Initialize(
			_isExample ? _exampleGridWidth : _puzzleGridWidth,
			_isExample ? _exampleGridHeight : _puzzleGridHeight
		);

		if (_executePuzzleCoroutine != null)
		{
			EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
		}
	}
	
	protected override void ExecutePuzzle1()
	{
		// Initialize grid
		ResetGrid();

		IEnumerable<string> dotCoordLines = _inputDataLines.Where(line => char.IsDigit(line[0]));
		IEnumerable<string> firstFoldOnly = new List<string> { _inputDataLines.First(line => char.IsLetter(line[0])) }; 
		_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(dotCoordLines, firstFoldOnly), this);
	}

	private IEnumerator ExecutePuzzle(IEnumerable<string> dotCoordLines, IEnumerable<string> foldingInstructionLines)
	{
		// Parse dot coords
		foreach (string line in _inputDataLines.Where(line => char.IsDigit(line[0])))
		{
			int[] coords = ParseIntArray(SplitString(line, ","));
			_grid.SetCellValue(coords[0], coords[1], true);
			_grid.HighlightCellView(coords[0], coords[1], _highlightColorDot);
		}

		EditorWaitForSeconds interval = new EditorWaitForSeconds(_foldInterval);
		yield return interval;
		
		// Parse folding instructions
		// eg. "fold along y=7"  =>  string[] { "y", "7" }
		IEnumerable<string[]> folds = foldingInstructionLines
			.Select(line => SplitString(line, " ").Last())
			.Select(line => SplitString(line, "="));
		
		foreach (string[] fold in folds)
		{
			string axis = fold[0];
			int foldIndex = int.Parse(fold[1]);
			Log("Folding along " + axis + " at " + foldIndex);

			if (axis.Equals("x"))
			{
				for (int row = 0; row < _grid.rows; row++)
				{
					for (int sourceColumn = foldIndex + 1; sourceColumn < _grid.columns; sourceColumn++)
					{
						if (_grid.cells[sourceColumn, row] == true)
						{
							// Flip along the fold column
							// eg. fold along x=5, 7 => 3 
							int destinationColumn = foldIndex - (sourceColumn - foldIndex);
							_grid.SetCellValue(destinationColumn, row, true);
							_grid.HighlightCellView(destinationColumn, row, _highlightColorDot);
						}
					}
				}
				
				// Remove all cells beyond the fold line
				_grid.ResizeGrid(foldIndex, _grid.rows);
			}
			else
			{
				for (int sourceRow = foldIndex + 1; sourceRow < _grid.rows; sourceRow++)
				{
					for (int column = 0; column < _grid.columns; column++)
					{
						if (_grid.cells[column, sourceRow] == true)
						{
							// Flip along the fold row
							// eg. fold along y=5, 7 => 3
							int destinationRow = foldIndex - (sourceRow - foldIndex);
							_grid.SetCellValue(column, destinationRow, true);
							_grid.HighlightCellView(column, destinationRow, _highlightColorDot);
						}
					}
				}
				
				// Remove all cells beyond the fold line
				_grid.ResizeGrid(_grid.columns, foldIndex);
			}

			yield return interval;
		}

		LogResult("Visible dots after folding", _grid.cells.Cast<bool>().Count(cell => cell == true));
		_executePuzzleCoroutine = null;
	}

	protected override void ExecutePuzzle2()
	{
		// Initialize grid
		ResetGrid();

		IEnumerable<string> dotCoordLines = _inputDataLines.Where(line => char.IsDigit(line[0]));
		IEnumerable<string> foldLines = _inputDataLines.Where(line => char.IsLetter(line[0])); 
		_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(dotCoordLines, foldLines), this);
	}
}
