using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

public class Day20 : PuzzleBase
{
	[SerializeField] private DynamicBoolGrid _grid = null;
	[SerializeField] private Color _lightPixelColor = Color.white;
	[SerializeField] private Color _darkPixelColor = Color.black;
	[SerializeField] private int _enhanceIterations = 2;
	[SerializeField] private float _enhanceInterval = 2f;
	
	private EditorCoroutine _executePuzzleCoroutine = null;

	[Button]
	private void ResetGrid()
	{
		_grid.Initialize(0, 0);

		if (_executePuzzleCoroutine != null)
		{
			EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
		}
	}
	
	protected override void ExecutePuzzle1()
	{
		// Initialize grid
		ResetGrid();
		
		_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
	}

	private IEnumerator ExecutePuzzle()
	{
		// Parse "algorithm" and "image"
		string algorithm = _inputDataLines[0];
		string[] imageData = _inputDataLines.Skip(1).Take(_inputDataLines.Length - 1).ToArray();

		_grid.Initialize(imageData[0].Length, imageData.Length);

		for (int row = 0; row < _grid.rows; row++)
		{
			for (int column = 0; column < _grid.columns; column++)
			{
				bool isLightPixel = imageData[row][column] == '#';
				_grid.SetCellValue(column, row, isLightPixel);
				_grid.HighlightCellView(column, row, isLightPixel ? _lightPixelColor : _darkPixelColor);
			}
		}
		
		EditorWaitForSeconds interval = new EditorWaitForSeconds(_enhanceInterval);
		yield return interval;
		
		EditorApplication.QueuePlayerLoopUpdate();
		yield return interval;
		
		for (int i = 0; i < _enhanceIterations; i++)
		{
			// Ensure there's at least 2 layers of empty space around the "image"
			// During each loop, 1 will have been used, so we need 1 more
			// We may need to pad with light pixels, if the first bit of the algorithm is a light pixel
			bool padWithLightPixels = algorithm[0] == '#' && i % 2 == 1;
			PadGrid(padWithLightPixels);
		
			EditorApplication.QueuePlayerLoopUpdate();
			yield return interval;
			
			// Build new image by iterating through all cells except for the outermost layer of padding
			StaticCellCollection<bool> newImageState = new StaticCellCollection<bool>(_grid.columns, _grid.rows);
			for (int pixelRow = 0; pixelRow < _grid.rows; pixelRow++)
			{
				for (int pixelColumn = 0; pixelColumn < _grid.columns; pixelColumn++)
				{
					StringBuilder blockBinaryBuilder = new StringBuilder();
					for (int row = pixelRow - 1; row <= pixelRow + 1; row++)
					{
						for (int column = pixelColumn - 1; column <= pixelColumn + 1; column++)
						{
							if (column < 0 || column >= _grid.columns - 1 || row < 0 || row >= _grid.rows - 1)
							{
								blockBinaryBuilder.Append(padWithLightPixels ? '1' : '0');
							}
							else
							{
								blockBinaryBuilder.Append(_grid.cells[column, row] == true ? '1' : '0');
							}
						}
					}

					int algorithmIndex = ConvertBinaryToDec(blockBinaryBuilder.ToString());
					newImageState[pixelColumn, pixelRow] = (algorithm[algorithmIndex] == '#');
				}
			}

			for (int row = 0; row < _grid.rows; row++)
			{
				for (int column = 0; column < _grid.columns; column++)
				{
					bool isLightPixel = newImageState[column, row];
					_grid.SetCellValue(column, row, isLightPixel);
					_grid.HighlightCellView(column, row, isLightPixel ? _lightPixelColor : _darkPixelColor);
				}
			}
			
			EditorApplication.QueuePlayerLoopUpdate();
			yield return interval;
		}

		LogResult("Light Pixel Count", _grid.cells.Cast<bool>().Count(cell => cell == true));
		_executePuzzleCoroutine = null;
	}

	private void PadGrid(bool padWithLightPixels)
	{
		_grid.InsertColumnAt(0, padWithLightPixels);
		_grid.InsertColumnAt(_grid.columns, padWithLightPixels);
		_grid.InsertRowAt(0, padWithLightPixels);
		_grid.InsertRowAt(_grid.rows, padWithLightPixels);

		Color paddingColor = padWithLightPixels ? _lightPixelColor : _darkPixelColor;
		_grid.HighlightColumn(0, paddingColor);
		_grid.HighlightColumn(_grid.columns - 1, paddingColor);
		_grid.HighlightRow(0, paddingColor);
		_grid.HighlightRow(_grid.rows - 1, paddingColor);
	}
	
	private static int ConvertBinaryToDec(string binary)
	{
		return Convert.ToInt32(binary, 2);
	}

	protected override void ExecutePuzzle2()
	{
		
	}
}
