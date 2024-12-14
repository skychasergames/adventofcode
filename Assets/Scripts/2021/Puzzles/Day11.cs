using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

public class Day11 : PuzzleBase
{
	[SerializeField] private IntGrid _octopusGrid = null;
	[SerializeField] private Color _newHighlightColorFlash = Color.green;
	[SerializeField] private Color _oldHighlightColorFlash = Color.yellow;
	[SerializeField] private int _iterations = 100;
	[SerializeField] private int _flashThreshold = 10;
	[SerializeField] private float _flashInterval = 0.5f;
	[SerializeField] private float _subflashInterval = 0.1f;
	[SerializeField] private bool _isDebugMode = false;

	private EditorCoroutine _executePuzzleCoroutine = null;
	private bool _advanceExecution = false;

	[Button]
	private void ClearGrid()
	{
		_octopusGrid.Initialize(0);

		if (_executePuzzleCoroutine != null)
		{
			EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
			_executePuzzleCoroutine = null;
		}
	}

	[Button, ShowIf(nameof(_isDebugMode))]
	private void DebugAdvanceExecution()
	{
		_advanceExecution = true;
	}
	
	protected override void ExecutePuzzle1()
	{
		// Reset, if necessary
		ClearGrid();
		
		// Initialize with input data
		_octopusGrid.Initialize(_inputDataLines);
		
		// Execute puzzle!
		_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(false), this);
	}

	private IEnumerator ExecutePuzzle(bool executeUntilSynchronized)
	{
		EditorWaitForSeconds interval = new EditorWaitForSeconds(_flashInterval);
		EditorWaitForSeconds subflashInterval = new EditorWaitForSeconds(_subflashInterval);
		
		int totalFlashes = 0;
		
		EditorApplication.QueuePlayerLoopUpdate();
		yield return WaitToAdvanceExecution(interval);

		int numCellsThatFlashedThisStep = 0;
		if (executeUntilSynchronized)
		{
			int numCells = _octopusGrid.cells.Length;
			int stepsElapsed = 0;
			while (numCellsThatFlashedThisStep != numCells)
			{
				yield return ExecutePuzzleStep(stepsElapsed);
				stepsElapsed++;
			}
			
			Log("Synchronized after " + stepsElapsed + " steps");
		}
		else
		{
			for (int i = 0; i < _iterations; i++)
			{
				yield return ExecutePuzzleStep(i);
			}

			LogResult("Total flashes after " + _iterations + " steps", totalFlashes);
		}

		_executePuzzleCoroutine = null;
		
		// --- Local method ---
		IEnumerator ExecutePuzzleStep(int i)
		{
			List<Vector2Int> allCellsThatFlashedThisStep = new List<Vector2Int>();
			
			// Increment ALL octopi
			for (int row = 0; row < _octopusGrid.rows; row++)
			{
				for (int column = 0; column < _octopusGrid.columns; column++)
				{
					int newValue = _octopusGrid.GetCellValue(column, row) + 1;
					_octopusGrid.SetCellValue(column, row, newValue);
					
					if (newValue >= _flashThreshold)
					{
						_octopusGrid.HighlightCellView(column, row, _newHighlightColorFlash);
						allCellsThatFlashedThisStep.Add(new Vector2Int(column, row));
						totalFlashes++;
					}
				}
			}
			
			// For every octopus that flashed, increment adjacent octopi
			if (allCellsThatFlashedThisStep.Count > 0)
			{
				EditorApplication.QueuePlayerLoopUpdate();
				yield return WaitToAdvanceExecution(subflashInterval);

				yield return PropagateFlashes(allCellsThatFlashedThisStep, true);
			}

			numCellsThatFlashedThisStep = allCellsThatFlashedThisStep.Count;
			LogResult("Completed step", i + 1);
			
			EditorApplication.QueuePlayerLoopUpdate();
			yield return WaitToAdvanceExecution(interval);

			// Every octopus that flashed returns to 0
			foreach (Vector2Int cell in allCellsThatFlashedThisStep)
			{
				_octopusGrid.SetCellValue(cell, 0);
				_octopusGrid.HighlightCellView(cell, Color.white);
			}
			
			// --- Local method ---
			IEnumerator PropagateFlashes(List<Vector2Int> cellsThatJustFlashed, bool isFirstIteration)
			{
				List<Vector2Int> cellsThatWillFlash = new List<Vector2Int>();
				
				foreach (Vector2Int cellThatJustFlashed in cellsThatJustFlashed)
				{
					// For each cell that just flashed, get all (up to) 8 of its neighbours, and increment them
					foreach (Vector2Int neighbourCell in _octopusGrid.GetAllNeighbourCoords(cellThatJustFlashed))
					{
						int newValue = _octopusGrid.GetCellValue(neighbourCell) + 1;
						_octopusGrid.SetCellValue(neighbourCell, newValue);
						
						// Track cells which should now flash
						if (!allCellsThatFlashedThisStep.Contains(neighbourCell) && !cellsThatWillFlash.Contains(neighbourCell) && newValue >= _flashThreshold)
						{
							cellsThatWillFlash.Add(neighbourCell);
							totalFlashes++;
						}
					}
				}

				// Update cell colours for cells that previously flashed and cells that should now flash
				foreach (Vector2Int cell in cellsThatJustFlashed)
				{
					_octopusGrid.HighlightCellView(cell, _oldHighlightColorFlash);
				}
				
				foreach (Vector2Int cell in cellsThatWillFlash)
				{
					_octopusGrid.HighlightCellView(cell, _newHighlightColorFlash);
				}

				if (cellsThatWillFlash.Count > 0)
				{
					EditorApplication.QueuePlayerLoopUpdate();
					yield return WaitToAdvanceExecution(subflashInterval);
					
					if (!isFirstIteration)
					{
						cellsThatJustFlashed.Clear();
					}

					allCellsThatFlashedThisStep.AddRange(cellsThatWillFlash);
					
					// Wooo recursion
					yield return PropagateFlashes(cellsThatWillFlash, false);
				}
			}
		}
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

	protected override void ExecutePuzzle2()
	{
		// Reset, if necessary
		ClearGrid();
		
		// Initialize with input data
		_octopusGrid.Initialize(_inputDataLines);
		
		// Execute puzzle!
		_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(true), this);
	}
}
