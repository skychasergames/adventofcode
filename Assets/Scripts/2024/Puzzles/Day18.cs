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
		[SerializeField] private Color _newCorruptedHighlight = Color.red;
		[SerializeField] private int _cyclesPerUpdateLoop = 100;
		[SerializeField] private float _byteInterval = 0.5f;

		private List<Vector2Int> _fallingBytes = null;
		
		private EditorCoroutine _executePuzzleCoroutine = null;
		private bool _hasPathAvailable = true;
		private List<Vector2Int> _currentShortestPath = new List<Vector2Int>();

		protected override void ExecutePuzzle1()
		{
			InitializeMap();
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(FindShortestPath(), this);
		}

		protected override void ExecutePuzzle2()
		{
			InitializeMap();
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle2Coroutine(), this);
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
			
			// Simulate initial falling bytes
			_fallingBytes = _inputDataLines.Select(line => ParseVector2Int(line, ",")).ToList();
			for (int i = 0; i < (_isExample ? _exampleFallingByteCount : _puzzleFallingByteCount); i++)
			{
				Vector2Int coords = _fallingBytes[i];
				DijkstraGridCell cell = _map.GetCellValue(coords);
				cell.SetBlocked(true);
				_map.HighlightCellView(coords, _corruptedHighlight);
			}
		}
		
		private IEnumerator FindShortestPath()
		{
			List<DijkstraGridCell> unvisitedCells = new List<DijkstraGridCell>(_map.cells.Cast<DijkstraGridCell>());
			List<DijkstraGridCell> tentativeCells = new List<DijkstraGridCell>();
			
			Vector2Int startCoord = Vector2Int.zero;
			DijkstraGridCell startCell = _map.GetCellValue(startCoord);
			DijkstraGridCell currentCell = startCell;
			currentCell.SetBestTotalDistanceToReachCell(0, null);

			Vector2Int endCoord = new Vector2Int(_map.columns-1, _map.rows-1);
			DijkstraGridCell endCell = _map.GetCellValue(endCoord);

			int cyclesSinceLastUpdate = 0;
			
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

				if (tentativeCells.Count == 0)
				{
					Log("No connection through to end cell");
					_hasPathAvailable = false;
					yield break;
				}
				
				currentCell = tentativeCells.OrderBy(cell => cell.BestTotalDistanceToReachCell).First();
				
				// Prevent Unity from locking up during long executions
				cyclesSinceLastUpdate++;
				if (cyclesSinceLastUpdate >= _cyclesPerUpdateLoop)
				{
					cyclesSinceLastUpdate = 0;
					
					EditorApplication.QueuePlayerLoopUpdate();
					yield return null;
				}
			}
			
			// Assemble and highlight the path we created
			_currentShortestPath = new List<Vector2Int>();
			DijkstraGridCell pathCell = endCell;
			do
			{
				_currentShortestPath.Add(pathCell.Coords);
				pathCell = pathCell.PreviousCellInPath;
			}
			while (pathCell != startCell);
				
			_currentShortestPath.Add(pathCell.Coords);
				
			foreach (Vector2Int cell in _currentShortestPath)
			{
				_map.HighlightCellView(cell.x, cell.y, _pathHighlight);
			}

			EditorApplication.QueuePlayerLoopUpdate();

			LogResult("Shortest path to exit", _currentShortestPath.Count-1);
		}

		private IEnumerator ExecutePuzzle2Coroutine()
		{
			EditorWaitForSeconds byteInterval = new EditorWaitForSeconds(_byteInterval);
			_hasPathAvailable = true;

			int currentByte = _isExample ? _exampleFallingByteCount : _puzzleFallingByteCount;
			Vector2Int currentByteCoords = _fallingBytes[currentByte];
			
			while (_hasPathAvailable)
			{
				// Reset all empty cells' highlight states and distance scores
				for (int y = 0; y < _map.rows; y++)
				{
					for (int x = 0; x < _map.columns; x++)
					{
						if (!_map.GetCellValue(x, y).IsBlocked)
						{
							_map.GetCellValue(x, y).SetBestTotalDistanceToReachCell(int.MaxValue, null);
							_map.HighlightCellView(x, y, Color.white);
						}
					}
				}
				
				// Check for shortest path
				yield return FindShortestPath();

				EditorApplication.QueuePlayerLoopUpdate();
				yield return byteInterval;
				
				if (_hasPathAvailable)
				{
					// Drop bytes onto map until path is blocked
					do
					{
						currentByte++;
						currentByteCoords = _fallingBytes[currentByte];
						DijkstraGridCell lastByteCell = _map.GetCellValue(currentByteCoords);
						lastByteCell.SetBlocked(true);
						_map.HighlightCellView(currentByteCoords, _newCorruptedHighlight);

						EditorApplication.QueuePlayerLoopUpdate();
						yield return byteInterval;
					}
					while (!_currentShortestPath.Contains(currentByteCoords));
				}
				else
				{
					Log("Path blocked at " + currentByteCoords + " after " + currentByte + " bytes");
					break;
				}
			}
		}
	}
}
