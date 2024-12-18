using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;

namespace AoC2024
{
	public class Day10 : PuzzleBase
	{
		[SerializeField] private IntGrid _map = null;
		[SerializeField] private Color _trailheadColor = Color.yellow;
		[SerializeField] private Color _trailColor = Color.yellow;
		[SerializeField] private Color _peakColor = Color.green;
		[SerializeField] private float _mapInterval = 1f;
		[SerializeField] private float _trailheadIntervalExample = 1f;
		[SerializeField] private float _trailheadIntervalPuzzle = 0.01f;
		
		private EditorCoroutine _executePuzzleCoroutine = null;
		
		protected override void ExecutePuzzle1()
		{
			ResetMap();
			
			string[][] mapDataArrays = SplitMapsFromInputDataLines();
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(mapDataArrays), this);
		}

		protected override void ExecutePuzzle2()
		{
			
		}
		
		[Button("Reset Map")]
		private void ResetMap()
		{
			if (_executePuzzleCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
				_executePuzzleCoroutine = null;
			}

			_map.ClearCellViews();
		}

		private string[][] SplitMapsFromInputDataLines()
		{
			List<List<string>> mapDataLists = new List<List<string>>();
			int i = 0;
			mapDataLists.Add(new List<string>());
			foreach (string line in _inputDataLines)
			{
				if (line == "---")
				{
					i++;
					mapDataLists.Add(new List<string>());
					continue;
				}
				
				mapDataLists[i].Add(line);
			}
			
			return mapDataLists.Select(list => list.ToArray()).ToArray();
		}

		private IEnumerator ExecutePuzzle(string[][] mapDataArrays)
		{
			EditorWaitForSeconds mapInterval = new EditorWaitForSeconds(_mapInterval);
			EditorWaitForSeconds trailheadInterval = new EditorWaitForSeconds(_isExample ? _trailheadIntervalExample : _trailheadIntervalPuzzle);
			
			foreach (string[] mapData in mapDataArrays)
			{
				_map.Initialize(mapData);

				yield return mapInterval;

				int totalTrailheadScore = 0;
				
				foreach (Vector2Int trailheadCell in _map.GetCoordsOfCellValue(0))
				{
					// Calculate trailhead score and highlight trails
					int trailheadScore = GetTrailheadScoreAndHighlightTrails(trailheadCell, out List<Vector2Int> highlightedCells);
					totalTrailheadScore += trailheadScore;
					
					// Re-highlight trailhead
					_map.HighlightCellView(trailheadCell, _trailheadColor);
					
					LogResult("Trailhead score for " + trailheadCell, trailheadScore);
					
					EditorApplication.QueuePlayerLoopUpdate();
					yield return trailheadInterval;
					
					// Reset all highlights
					foreach (Vector2Int highlightedCell in highlightedCells)
					{
						_map.HighlightCellView(highlightedCell, Color.white);
					}
				}

				LogResult("Total trailhead score", totalTrailheadScore);
				
				yield return mapInterval;
			}

			_executePuzzleCoroutine = null;
		}

		private int GetTrailheadScoreAndHighlightTrails(Vector2Int trailhead, out List<Vector2Int> highlightedCells)
		{
			List<Vector2Int> tempHighlightedCells = new List<Vector2Int>(); // Using temp variable because you can't use out parameter in local functions
			HashSet<Vector2Int> peakCells = new HashSet<Vector2Int>();

			TraverseHikingTrail(1, trailhead);
			
			// Local method, recursive
			void TraverseHikingTrail(int currentStep, Vector2Int currentCell)
			{
				if (currentStep > 9)
				{
					// Found trail end
					peakCells.Add(currentCell);
					_map.HighlightCellView(currentCell, _peakColor);
					tempHighlightedCells.Add(currentCell);
					return;
				}

				_map.HighlightCellView(currentCell, _trailColor);
				tempHighlightedCells.Add(currentCell);

				foreach (Vector2Int neighbourCell in _map.GetOrthogonalNeighbourCoords(currentCell).Where(neighbourCell => _map.GetCellValue(neighbourCell) == currentStep))
				{
					TraverseHikingTrail(currentStep + 1, neighbourCell);
				}
			}

			highlightedCells = tempHighlightedCells;
			return peakCells.Count;
		}
	}
}