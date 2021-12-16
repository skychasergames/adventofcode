using System.Collections.Generic;
using System.Linq;
using System.Text;
using NaughtyAttributes;
using UnityEngine;

public class Day9 : PuzzleBase
{
	[SerializeField] private IntGrid _heightmap = null;
	[SerializeField] private Color _highlightColorLowPoint = Color.yellow;
	
	[Button("Clear Heightmap")]
	private void ClearHeightmap()
	{
		_heightmap.Initialize(0);
	}
	
	protected override void ExecutePuzzle1()
	{
		_heightmap.Initialize(_inputDataLines);

		List<int> lowPoints = new List<int>();
		for (int row = 0; row < _heightmap.rows; row++)
		{
			for (int column = 0; column < _heightmap.columns; column++)
			{
				if (IsLowPoint(column, row))
				{
					lowPoints.Add(_heightmap.cells[column, row]);
					_heightmap.HighlightCellView(column, row, _highlightColorLowPoint);
				}
			}
		}

		float totalRiskLevel = lowPoints.Sum() + lowPoints.Count;
		LogResult("Total risk level of low points", totalRiskLevel);
	}

	private bool IsLowPoint(int column, int row)
	{
		int height = _heightmap.cells[column, row];
		return _heightmap.GetOrthogonalNeighbours(column, row).All(neighbourHeight => height < neighbourHeight);
	}

	protected override void ExecutePuzzle2()
	{
		
	}
}
