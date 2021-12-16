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

		List<Vector2Int> lowPointCoords = FindLowPointCoords();
		foreach (Vector2Int lowPoint in lowPointCoords)
		{
			_heightmap.HighlightCellView(lowPoint.x, lowPoint.y, _highlightColorLowPoint);
		}

		float totalRiskLevel = lowPointCoords.Select(coord => _heightmap.cells[coord.x, coord.y]).Sum() + lowPointCoords.Count;
		LogResult("Total risk level of low points", totalRiskLevel);
	}

	private List<Vector2Int> FindLowPointCoords()
	{
		List<Vector2Int> lowPoints = new List<Vector2Int>();
		for (int row = 0; row < _heightmap.rows; row++)
		{
			for (int column = 0; column < _heightmap.columns; column++)
			{
				if (IsLowPoint(column, row))
				{
					lowPoints.Add(new Vector2Int(column, row));
				}
			}
		}

		return lowPoints;
	}

	private bool IsLowPoint(int column, int row)
	{
		int height = _heightmap.cells[column, row];
		return _heightmap.GetOrthogonalNeighbourValues(column, row).All(neighbourHeight => height < neighbourHeight);
	}

	protected override void ExecutePuzzle2()
	{
		_heightmap.Initialize(_inputDataLines);
		
		List<List<Vector2Int>> basins = new List<List<Vector2Int>>();
		
		List<Vector2Int> lowPointCoords = FindLowPointCoords();
		foreach (Vector2Int lowPoint in lowPointCoords)
		{
			// Calculate the basin
			List<Vector2Int> checkedCells = new List<Vector2Int>();
			List<Vector2Int> cellsInBasin = new List<Vector2Int>();
			checkedCells.Add(lowPoint);
			GrowBasin(lowPoint);

			// Basin calculation complete :ez:
			basins.Add(cellsInBasin);
			
			Color basinColor = Random.ColorHSV(0f, 1f, 0.25f, 0.75f, 1f, 1f);
			foreach (Vector2Int cell in cellsInBasin)
			{
				_heightmap.HighlightCellView(cell.x, cell.y, basinColor);
			}
			
			// --- Local method ---
			void GrowBasin(Vector2Int cell)
			{
				cellsInBasin.Add(cell);
				foreach (Vector2Int neighbour in _heightmap.GetOrthogonalNeighbourCoords(cell.x, cell.y).Where(neighbour => !checkedCells.Contains(neighbour)))
				{
					checkedCells.Add(neighbour);
					if (_heightmap.cells[neighbour.x, neighbour.y] < 9)
					{
						// Wooo recursion
						GrowBasin(neighbour);
					}
				}
			}
		}
		
		// Use the 3 largest basins, multiply their sizes together
		int result = 1;
		foreach (var basin in basins.OrderByDescending(basin => basin.Count).Take(3))
		{
			Log("Chonky basin, includes " + basin.Count + " cells");
			result *= basin.Count;
		}

		LogResult("Final result", result);
	}
}
