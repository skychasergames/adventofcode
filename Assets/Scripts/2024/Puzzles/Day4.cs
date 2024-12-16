using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace AoC2024
{
	public class Day4 : PuzzleBase
	{
		[SerializeField] private CharGrid _grid = null;
		[SerializeField] private Color _highlightColor = Color.green;

		private List<(int, int)> _searchVectors = new List<(int, int)>
		{
			( 0, -1), // up
			( 1, -1), // right-up
			( 1,  0), // right
			( 1,  1), // right-down
			( 0,  1), // down
			(-1,  1), // left-down
			(-1,  0), // left
			(-1, -1), // left-up
		};
		
		[Button("Reset Grid")]
		private void ResetGrid()
		{
			_grid.ClearCellViews();
		}

		protected override void ExecutePuzzle1()
		{
			_grid.Initialize(_inputDataLines);

			const string word = "XMAS";
			int totalXmases = 0;
			for (int y = 0; y < _grid.rows; y++)
			{
				for (int x = 0; x < _grid.columns; x++)
				{
					totalXmases += HighlightWordsStartingAtCell(word, x, y);
				}
			}

			LogResult("Total XMASes found", totalXmases);
		}

		protected override void ExecutePuzzle2()
		{
			
		}

		/// Highlights any occurrences of the word starting at the given coordinates.
		/// Returns the number of occurrences highlighted. 
		private int HighlightWordsStartingAtCell(string word, int x, int y)
		{
			int numWordsStartingAtCell = 0;
			if (_grid.GetCellValue(x, y) == word[0])
			{
				foreach ((int x, int y) searchVector in _searchVectors)
				{
					if (CheckForWordInDirection(word, x, y, searchVector.x, searchVector.y))
					{
						HighlightWordInDirection(word, x, y, searchVector.x, searchVector.y);
						numWordsStartingAtCell++;
					}
				}
			}

			return numWordsStartingAtCell;
		}

		private bool CheckForWordInDirection(string word, int startX, int startY, int directionX, int directionY)
		{
			for (int i = 1; i < word.Length; i++)
			{
				if (!_grid.CellExists(startX + directionX * i, startY + directionY * i) ||
				    _grid.GetCellValue(startX + directionX * i, startY + directionY * i) != word[i])
				{
					return false;
				}
			}

			return true;
		}

		private void HighlightWordInDirection(string word, int startX, int startY, int directionX, int directionY)
		{
			for (int i = 0; i < word.Length; i++)
			{
				_grid.HighlightCellView(startX + directionX * i, startY + directionY * i, _highlightColor);
			}
		}
	}
}
