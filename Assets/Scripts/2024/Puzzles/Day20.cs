using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace AoC2024
{
	public class Day20 : PuzzleBase
	{
		[SerializeField] private DijkstraGrid _map = null;
		[SerializeField] private Color _wallColor = Color.black;
		[SerializeField] private Color _currentWallCheatColor = Color.magenta;
		[SerializeField] private Color _oldWallCheatColor = Color.grey;
		[SerializeField] private Color _currentPathCheatStartColor = new Color(1.0f, 0.6f, 0.7f);
		[SerializeField] private Color _currentPathCheatEndColor = new Color(0.8f, 0.4f, 1.0f);
		[SerializeField] private Color _oldPathCheatColor = new Color(0.8f, 0.8f, 0.8f);
		[SerializeField] private Color _startColor = Color.yellow;
		[SerializeField] private Color _endColor = Color.green;
		[SerializeField] private int _cheatScoreThreshold = 100;
		[SerializeField] private int _maxCheatDistance = 20;
		[SerializeField] private float _cheatInterval = 0.1f;
		[SerializeField] private int _skipCheatInterval = 100;
		
		private EditorCoroutine _executePuzzleCoroutine = null;
		
		private Vector2Int _startCoord = Vector2Int.zero;
		private Vector2Int _endCoord = Vector2Int.zero;
		private List<Vector2Int> _path = null; 
		private List<Vector2Int> _internalWalls = null; 
		private int _fullRaceLength = 0;
		private Dictionary<int, int> _numCheatsByScore = new Dictionary<int, int>();

		protected override void ExecutePuzzle1()
		{
			InitializeMap();
			FindShortestPath();
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(FindBestSingleWallCheats(), this);
		}

		protected override void ExecutePuzzle2()
		{
			InitializeMap();
			FindShortestPath();
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(FindBestCheats(), this);
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
			_path = new List<Vector2Int>();
			_internalWalls = new List<Vector2Int>();
			_numCheatsByScore = new Dictionary<int, int>();
		}

		private void InitializeMap()
		{
			ResetMap();

			_map.Initialize(_inputDataLines[0].Length, _inputDataLines.Length);
			
			// Parse map
			for (int y = 0; y < _inputDataLines.Length; y++)
			{
				for (int x = 0; x < _inputDataLines[0].Length; x++)
				{
					DijkstraGridCell cell = _map.GetCellValue(x, y);
					switch (_inputDataLines[y][x])
					{
					case '#':
						cell.SetBlocked(true);
						_map.HighlightCellView(x, y, _wallColor);

						if (x > 0 && x < _inputDataLines[0].Length - 1 &&
						    y > 0 && y < _inputDataLines.Length - 1)
						{
							_internalWalls.Add(new Vector2Int(x, y));
						}
						
						break;
					
					case 'S':
						_startCoord = new Vector2Int(x, y);
						_map.HighlightCellView(x, y, _startColor);
						break;
					
					case 'E':
						_endCoord = new Vector2Int(x, y);
						_map.HighlightCellView(x, y, _endColor);
						break;
					}
				}
			}
		}

		private void FindShortestPath()
		{
			List<DijkstraGridCell> unvisitedCells = new List<DijkstraGridCell>(_map.cells.Cast<DijkstraGridCell>());
			List<DijkstraGridCell> tentativeCells = new List<DijkstraGridCell>(_map.cells.Cast<DijkstraGridCell>());
			
			DijkstraGridCell startCell = _map.GetCellValue(_startCoord);
			DijkstraGridCell currentCell = startCell;
			currentCell.SetBestTotalDistanceToReachCell(0, null);
			
			DijkstraGridCell endCell = _map.GetCellValue(_endCoord);
			
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

				unvisitedCells.Remove(currentCell);
				tentativeCells.Remove(currentCell);

				if (currentCell == endCell)
				{
					break;
				}

				if (tentativeCells.Count == 0)
				{
					Log("No connection through to end cell");
					return;
				}
				
				currentCell = tentativeCells.OrderBy(cell => cell.BestTotalDistanceToReachCell).First();
			}

			_fullRaceLength = endCell.BestTotalDistanceToReachCell;
			LogResult("Full race length without cheats", _fullRaceLength + " picoseconds");
			
			_path = new List<Vector2Int>();
			DijkstraGridCell pathCell = endCell;
			do
			{
				_path.Add(pathCell.Coords);
				pathCell = pathCell.PreviousCellInPath;
			}
			while (pathCell != startCell);
			
			_path.Add(pathCell.Coords);
			_path.Reverse();
		}

		private IEnumerator FindBestSingleWallCheats()
		{
			EditorWaitForSeconds cheatInterval = new EditorWaitForSeconds(_cheatInterval);
			int intervalsSkipped = 0;

			foreach (Vector2Int cheatCoord in _internalWalls)
			{
				_map.HighlightCellView(cheatCoord, _currentWallCheatColor);
				EditorApplication.QueuePlayerLoopUpdate();

				List<int> cellDistances = _map.GetOrthogonalNeighbourValues(cheatCoord)
					.Where(cell => !cell.IsBlocked)
					.Select(cell => cell.BestTotalDistanceToReachCell)
					.OrderByDescending(cell => cell)
					.ToList();

				if (cellDistances.Count >= 2)
				{
					int cheatScore = (cellDistances.First() - cellDistances.Last()) - 2;	// -2 because normal movement takes 2 picoseconds
					if (cheatScore > 0) 
					{
						_numCheatsByScore.AddIfUnique(cheatScore, 0);
						_numCheatsByScore[cheatScore]++;
						LogResult("Cheat score", cheatScore);
					}
				}

				intervalsSkipped++;
				if (intervalsSkipped > _skipCheatInterval)
				{
					intervalsSkipped = 0;
					yield return cheatInterval;
				}
				
				_map.HighlightCellView(cheatCoord, _oldWallCheatColor);
				EditorApplication.QueuePlayerLoopUpdate();
			}

			int totalCheatsAboveScoreThreshold = 0;
			Log("Total number of cheats:");
			foreach (KeyValuePair<int, int> pair in _numCheatsByScore.OrderBy(pair => pair.Key))
			{
				Log("- " + pair.Value + " cheats that save " + pair.Key + " picoseconds");
				if (pair.Key >= _cheatScoreThreshold)
				{
					totalCheatsAboveScoreThreshold += pair.Value;
				}
			}

			LogResult("Total cheats that save at least " + _cheatScoreThreshold + " picoseconds", totalCheatsAboveScoreThreshold);
		}
		
		private IEnumerator FindBestCheats()
		{
			EditorWaitForSeconds cheatInterval = new EditorWaitForSeconds(_cheatInterval);
			int intervalsSkipped = 0;

			// Check for start and end cells that are at least 4 steps away (the shortest cheat - cutting a U-turn)
			const int minDistance = 4;
			for (int cheatStartIndex = 0; cheatStartIndex < _path.Count - minDistance; cheatStartIndex++)
			{
				DijkstraGridCell cheatStartCell = _map.GetCellValue(_path[cheatStartIndex]);
				_map.HighlightCellView(cheatStartCell.Coords, _currentPathCheatStartColor);
				
				for (int cheatEndIndex = cheatStartIndex + minDistance; cheatEndIndex < _path.Count; cheatEndIndex++)
				{
					DijkstraGridCell cheatEndCell = _map.GetCellValue(_path[cheatEndIndex]);
					_map.HighlightCellView(cheatEndCell.Coords, _currentPathCheatEndColor);
					EditorApplication.QueuePlayerLoopUpdate();
					
					Vector2Int distanceBetweenCells = cheatStartCell.Coords - cheatEndCell.Coords;
					int cheatDistance = Mathf.Abs(distanceBetweenCells.x) + Mathf.Abs(distanceBetweenCells.y);
					if (cheatDistance <= _maxCheatDistance)
					{
						int cheatScore = (cheatEndCell.BestTotalDistanceToReachCell - cheatStartCell.BestTotalDistanceToReachCell) - cheatDistance;
						if (cheatScore > 0) 
						{
							_numCheatsByScore.AddIfUnique(cheatScore, 0);
							_numCheatsByScore[cheatScore]++;
							//LogResult("Cheat score", cheatScore);
						}
					}

					intervalsSkipped++;
					if (intervalsSkipped > _skipCheatInterval)
					{
						intervalsSkipped = 0;
						yield return cheatInterval;
					}
				
					_map.HighlightCellView(cheatEndCell.Coords, Color.white);
					EditorApplication.QueuePlayerLoopUpdate();
				}
				
				_map.HighlightCellView(cheatStartCell.Coords, _oldPathCheatColor);
				EditorApplication.QueuePlayerLoopUpdate();
			}

			int totalCheatsAboveScoreThreshold = 0;
			Log("Total number of cheats:");
			foreach (KeyValuePair<int, int> pair in _numCheatsByScore.OrderBy(pair => pair.Key))
			{
				Log("- " + pair.Value + " cheats that save " + pair.Key + " picoseconds");
				if (pair.Key >= _cheatScoreThreshold)
				{
					totalCheatsAboveScoreThreshold += pair.Value;
				}
			}

			LogResult("Total cheats that save at least " + _cheatScoreThreshold + " picoseconds", totalCheatsAboveScoreThreshold);
		}
	}
}
