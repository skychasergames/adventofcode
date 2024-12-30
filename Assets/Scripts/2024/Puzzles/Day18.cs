using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace AoC2024
{
	public class Day18 : PuzzleBase
	{
		[SerializeField] private Vector2Int _exampleMapSize = new Vector2Int(7, 7);		// "0 to 6"
		[SerializeField] private Vector2Int _puzzleMapSize = new Vector2Int(71, 71);	// "0 to 70"
		[SerializeField] private int _exampleFallingByteCount = 12;
		[SerializeField] private int _puzzleFallingByteCount = 1024;
		
		[SerializeField] private DijkstraGrid _map = null;
		[SerializeField] private Color _pathHighlight = Color.yellow;
		[SerializeField] private Color _corruptedHighlight = Color.black;
		
		private EditorCoroutine _executePuzzleCoroutine = null;
		
		protected override void ExecutePuzzle1()
		{
			InitializeMap();
			
			// Simulate initial falling bytes
			List<Vector2Int> fallingBytes = GetFallingBytes();
			for (int i = 0; i < (_isExample ? _exampleFallingByteCount : _puzzleFallingByteCount); i++)
			{
				Vector2Int coords = fallingBytes[i];
				DijkstraGridCell cell = _map.GetCellValue(coords);
				cell.SetBlocked(true);
				_map.HighlightCellView(coords, _corruptedHighlight);
			}
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
		}

		protected override void ExecutePuzzle2()
		{
			
		}
		
		[Button]
		private void ResetMap()
		{
			if (_executePuzzleCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
				_executePuzzleCoroutine = null;
			}
		
			_map.Initialize(0);
		}

		private void InitializeMap()
		{
			ResetMap();
			
			_map.Initialize(_isExample ? _exampleMapSize : _puzzleMapSize);
		}

		private List<Vector2Int> GetFallingBytes()
		{
			return _inputDataLines.Select(line => ParseVector2Int(line, ",")).ToList();
		}

		private IEnumerator ExecutePuzzle()
		{
			List<DijkstraGridCell> unvisitedCells = new List<DijkstraGridCell>(_map.cells.Cast<DijkstraGridCell>());
			List<DijkstraGridCell> tentativeCells = new List<DijkstraGridCell>();
			
			Vector2Int startCoord = Vector2Int.zero;
			DijkstraGridCell startCell = _map.GetCellValue(startCoord);
			DijkstraGridCell currentCell = startCell;
			currentCell.SetBestTotalDistanceToReachCell(0, null);

			Vector2Int endCoord = new Vector2Int(_map.columns-1, _map.rows-1);
			DijkstraGridCell endCell = _map.GetCellValue(endCoord);
			
			while (true)
			{
				foreach (DijkstraGridCell neighbour in _map.GetOrthogonalNeighbourValues(currentCell.Coords)
					.Where(neighbour => !neighbour.IsBlocked && unvisitedCells.Contains(neighbour)))
				{
					int totalDistanceToReachNeighbour = currentCell.BestTotalDistanceToReachCell + 1;
					if (totalDistanceToReachNeighbour < neighbour.BestTotalDistanceToReachCell)
					{
						neighbour.SetBestTotalDistanceToReachCell(totalDistanceToReachNeighbour, currentCell);
						
						if (!tentativeCells.Contains(neighbour))
						{
							tentativeCells.Add(neighbour);
						}
					}
				}

				_map.HighlightCellView(currentCell.Coords, Color.cyan);
				
				unvisitedCells.Remove(currentCell);
				tentativeCells.Remove(currentCell);

				if (currentCell == endCell)
				{
					break;
				}
				
				currentCell = tentativeCells.OrderBy(cell => cell.BestTotalDistanceToReachCell).First();
				
				EditorApplication.QueuePlayerLoopUpdate();
				yield return null;
			}
			
			// Assemble and highlight the path we created
			List<Vector2Int> bestPath = new List<Vector2Int>();
			DijkstraGridCell pathCell = endCell;
			do
			{
				bestPath.Add(pathCell.Coords);
				pathCell = pathCell.PreviousCellInPath;
			}
			while (pathCell != startCell);
				
			bestPath.Add(pathCell.Coords);
				
			foreach (Vector2Int cell in bestPath)
			{
				_map.HighlightCellView(cell.x, cell.y, _pathHighlight);
			}

			EditorApplication.QueuePlayerLoopUpdate();

			LogResult("Shortest path to exit", bestPath.Count-1);
		}
	}
}
