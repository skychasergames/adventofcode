using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace AoC2021
{
	public class Day4 : PuzzleBase
	{
		[SerializeField] private IntGrid _bingoGridPrefab = null;
		[SerializeField] private Transform _bingoGridParent = null;
		[SerializeField] private int _bingoGridHeight = 5;
		[SerializeField] private float _numberRevealInterval = 0.25f;
		[SerializeField] private Color _highlightColorCell = Color.yellow;
		[SerializeField] private Color _highlightColorBingo = Color.green;
		[SerializeField] private Color _highlightColorSquidBingo = Color.cyan;
	
		private EditorCoroutine _playBingoCoroutine = null;
	
		protected override void ExecutePuzzle1()
		{
			// Reset if necessary
			ResetBoards();
		
			// Parse numbers
			int[] numbers = ParseIntArray(SplitString(_inputDataLines[0], ","));

			// Parse bingo grids
			List<IntGrid> grids = ParseBingoGridsFromData();
		
			// Play bingo!
			_playBingoCoroutine = EditorCoroutineUtility.StartCoroutine(PlayBingo(numbers, grids), this);
		}
	
		[Button("Reset Boards")]
		private void ResetBoards()
		{
			if (_playBingoCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(_playBingoCoroutine);
			}
		
			while (_bingoGridParent.childCount > 0)
			{
				DestroyImmediate(_bingoGridParent.GetChild(0).gameObject);
			}
		}

		private List<IntGrid> ParseBingoGridsFromData()
		{
			List<IntGrid> grids = new List<IntGrid>();
			for (int rootRow = 1; rootRow < _inputDataLines.Length; rootRow += _bingoGridHeight)
			{
				IntGrid grid = Instantiate(_bingoGridPrefab, _bingoGridParent);
				grid.name = "Bingo Grid " + (grids.Count + 1);
				List<string> gridData = new List<string>();
			
				for (int subRow = 0; subRow < _bingoGridHeight; subRow++)
				{
					string line = _inputDataLines[rootRow + subRow];
					gridData.Add(line);
				}
			
				grid.Initialize(gridData.ToArray(), " ");
				grids.Add(grid);
			}

			return grids;
		}

		private IEnumerator PlayBingo(int[] numbers, List<IntGrid> grids)
		{
			IntGrid winningGrid = null;
			int bingoColumn = -1;
			int bingoRow = -1;
			int finalNumber = -1;
			EditorWaitForSeconds interval = new EditorWaitForSeconds(_numberRevealInterval);
		
			// Initialize state data
			Dictionary<IntGrid, bool[,]> cellStatesPerGrid = grids.ToDictionary(grid => grid, grid => new bool[grid.columns,grid.rows]);
		
			// Call one number at a time, marking it off on every board
			foreach (int number in numbers)
			{
				foreach (IntGrid grid in grids)
				{
					// Mark off number
					for (int row = 0; row < grid.rows; row++)
					{
						for (int column = 0; column < grid.columns; column++)
						{
							if (grid.cells[column, row] == number)
							{
								cellStatesPerGrid[grid][column, row] = true;
								grid.HighlightCellView(column, row, _highlightColorCell);
							
								EditorApplication.QueuePlayerLoopUpdate();
							}
						}
					}

					// Check for bingo
					if (CheckGridForBingo(grid, cellStatesPerGrid[grid], out bingoColumn, out bingoRow))
					{
						// We have a winner!
						winningGrid = grid;
						finalNumber = number;
						break;
					}
				}

				if (winningGrid != null)
				{
					break;
				}

				yield return interval;
			}

			if (winningGrid != null)
			{
				LogResult("That's a bingo!", winningGrid);
			
				// Highlight bingo column/row green
				if (bingoColumn >= 0)
				{
					winningGrid.HighlightColumn(bingoColumn, _highlightColorBingo);
				}
				else if (bingoRow >= 0)
				{
					winningGrid.HighlightRow(bingoRow, _highlightColorBingo);
				}
				else
				{
					LogError("No valid bingo column or row");
				}

				// Calculate the final score
				// Score = (sum of all unmarked numbers) * last number called
				int finalScore = SumOfUnmarkedCells(winningGrid, cellStatesPerGrid[winningGrid]) * finalNumber;
				LogResult("Winning board score", finalScore);
			}
			else
			{
				LogError("No boards got bingo!?");
			}

			_playBingoCoroutine = null;
		}

		private bool CheckGridForBingo(IntGrid grid, bool[,] cellStates, out int bingoColumn, out int bingoRow)
		{
			bingoColumn = -1;
			bingoRow = -1;
		
			for (int row = 0; row < grid.rows; row++)
			{
				if (IsRowBingo())
				{
					bingoRow = row;
					return true;
				}
					
				bool IsRowBingo()
				{
					for (int column = 0; column < grid.columns; column++)
					{
						if (!cellStates[column, row])
						{
							return false;
						}
					}

					return true;
				}
			}

			for (int column = 0; column < grid.rows; column++)
			{
				if (IsColumnBingo())
				{
					bingoColumn = column;
					return true;
				}
					
				bool IsColumnBingo()
				{
					for (int row = 0; row < grid.rows; row++)
					{
						if (!cellStates[column, row])
						{
							return false;
						}
					}

					return true;
				}
			}

			return false;
		}
	
		private int SumOfUnmarkedCells(IntGrid grid, bool[,] cellStates)
		{
			int sum = 0;
			for (int row = 0; row < grid.rows; row++)
			{
				for (int column = 0; column < grid.columns; column++)
				{
					if (!cellStates[column, row])
					{
						sum += grid.cells[column, row];
					}
				}
			}
		
			return sum;
		}

		protected override void ExecutePuzzle2()
		{
			// Reset if necessary
			ResetBoards();
		
			// Parse numbers
			int[] numbers = ParseIntArray(SplitString(_inputDataLines[0], ","));

			// Parse bingo grids
			List<IntGrid> grids = ParseBingoGridsFromData();
		
			// Play bingo!
			_playBingoCoroutine = EditorCoroutineUtility.StartCoroutine(PlayBingoToLose(numbers, grids), this);
		}
	
		private IEnumerator PlayBingoToLose(int[] numbers, List<IntGrid> grids)
		{
			IntGrid losingGrid = null;
			int lastBingoColumn = -1;
			int lastBingoRow = -1;
			int finalNumber = -1;
			EditorWaitForSeconds interval = new EditorWaitForSeconds(_numberRevealInterval);
		
			// Initialize state data
			Dictionary<IntGrid, bool[,]> cellStatesPerGrid = grids.ToDictionary(grid => grid, grid => new bool[grid.columns,grid.rows]);
		
			// Call one number at a time, marking it off on every board
			foreach (int number in numbers)
			{
				foreach (IntGrid grid in grids)
				{
					// Mark off number
					for (int row = 0; row < grid.rows; row++)
					{
						for (int column = 0; column < grid.columns; column++)
						{
							if (grid.cells[column, row] == number)
							{
								cellStatesPerGrid[grid][column, row] = true;
								grid.HighlightCellView(column, row, _highlightColorCell);

								EditorApplication.QueuePlayerLoopUpdate();
							}
						}
					}
				}

				// Check each board for bingo. If it's a bingo, give it to the giant squid (discard).
				for (int i = grids.Count - 1; i >= 0; i--)
				{
					IntGrid grid = grids[i];
					if (CheckGridForBingo(grid, cellStatesPerGrid[grid], out lastBingoColumn, out lastBingoRow))
					{
						// We have a winner! ... which we don't want.
						if (grids.Count == 1)
						{
							// Hold up, this is the last board - we've done it! We're gonna lose!
							losingGrid = grid;
							finalNumber = number;
						}
						else
						{
							grids.Remove(grid);
							cellStatesPerGrid.Remove(grid);
						
							// Highlight bingo column/row blue
							if (lastBingoColumn >= 0)
							{
								grid.HighlightColumn(lastBingoColumn, _highlightColorSquidBingo);
							}
							else if (lastBingoRow >= 0)
							{
								grid.HighlightRow(lastBingoRow, _highlightColorSquidBingo);
							}
							else
							{
								LogError("No valid bingo column or row");
							}
						} 
					}
				}

				if (losingGrid != null)
				{
					break;
				}

				yield return interval;
			}

			if (losingGrid != null)
			{
				LogResult("And the Participation Medal goes to", losingGrid);
			
				// Highlight bingo column/row green
				if (lastBingoColumn >= 0)
				{
					losingGrid.HighlightColumn(lastBingoColumn, _highlightColorBingo);
				}
				else if (lastBingoRow >= 0)
				{
					losingGrid.HighlightRow(lastBingoRow, _highlightColorBingo);
				}
				else
				{
					LogError("No valid bingo column or row");
				}

				// Calculate the final score
				// Score = (sum of all unmarked numbers) * last number called
				int finalScore = SumOfUnmarkedCells(losingGrid, cellStatesPerGrid[losingGrid]) * finalNumber;
				LogResult("Winning board score", finalScore);
			}
			else
			{
				LogError("No boards got bingo!?");
			}

			_playBingoCoroutine = null;
		}
	}
}
