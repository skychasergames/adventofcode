using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

public class Day15 : PuzzleBase
{
	[SerializeField] private DijkstraGrid _map = null;
	[SerializeField] private Color _highlightColor = Color.yellow;

	private EditorCoroutine _puzzleCoroutine = null;
	
	[Button]
	private void ClearMap()
	{
		if (_puzzleCoroutine != null)
		{
			EditorCoroutineUtility.StopCoroutine(_puzzleCoroutine);
			_puzzleCoroutine = null;
		}
		
		_map.Initialize(0);
	}
	
	/*
	https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm#Algorithm
	
	Let the node at which we are starting at be called the initial node. Let the distance of node Y be the distance from the initial node to Y. Dijkstra's algorithm will initially start with infinite distances and will try to improve them step by step.

    1. Mark all nodes unvisited. Create a set of all the unvisited nodes called the unvisited set.
    2. Assign to every node a tentative distance value: set it to zero for our initial node and to infinity for all other nodes. The tentative distance of a node v is the length of the shortest path discovered so far between the node v and the starting node. Since initially no path is known to any other vertex than the source itself (which is a path of length zero), all other tentative distances are initially set to infinity. Set the initial node as current.
    3. For the current node, consider all of its unvisited neighbors and calculate their tentative distances through the current node. Compare the newly calculated tentative distance to the current assigned value and assign the smaller one. For example, if the current node A is marked with a distance of 6, and the edge connecting it with a neighbor B has length 2, then the distance to B through A will be 6 + 2 = 8. If B was previously marked with a distance greater than 8 then change it to 8. Otherwise, the current value will be kept.
    4. When we are done considering all of the unvisited neighbors of the current node, mark the current node as visited and remove it from the unvisited set. A visited node will never be checked again.
    5. If the destination node has been marked visited (when planning a route between two specific nodes) or if the smallest tentative distance among the nodes in the unvisited set is infinity (when planning a complete traversal; occurs when there is no connection between the initial node and remaining unvisited nodes), then stop. The algorithm has finished.
    6. Otherwise, select the unvisited node that is marked with the smallest tentative distance, set it as the new current node, and go back to step 3.
	*/

	protected override void ExecutePuzzle1()
	{
		ClearMap();

		// 1. Mark all nodes unvisited. Create a set of all the unvisited nodes called the unvisited set.
		_map.Initialize(_inputDataLines);
		List<DijkstraGridCell> unvisitedCells = new List<DijkstraGridCell>(_map.cells.Cast<DijkstraGridCell>());

		// 2. Assign to every node a tentative distance value: set it to zero for our initial node and to infinity for
		//    all other nodes. The tentative distance of a node v is the length of the shortest path discovered so far
		//    between the node v and the starting node. Since initially no path is known to any other vertex than the
		//    source itself (which is a path of length zero), all other tentative distances are initially set to
		//    infinity. Set the initial node as current.
		Vector2Int startCoord = Vector2Int.zero;
		DijkstraGridCell startCell = _map.GetCellValue(startCoord);
		DijkstraGridCell currentCell = startCell;
		currentCell.SetBestTotalDistanceToReachCell(0, null);

		Vector2Int endCoord = new Vector2Int(_map.columns - 1, _map.rows - 1);
		DijkstraGridCell endCell = _map.GetCellValue(endCoord);
		
		while (true)
		{
			// 3. For the current node, consider all of its unvisited neighbors and calculate their tentative distances
			//    through the current node. Compare the newly calculated tentative distance to the current assigned
			//    value and assign the smaller one. For example, if the current node A is marked with a distance of 6,
			//    and the edge connecting it with a neighbor B has length 2, then the distance to B through A will be
			//    6 + 2 = 8. If B was previously marked with a distance greater than 8 then change it to 8. Otherwise,
			//    the current value will be kept.
			IEnumerable neighbours = _map.GetOrthogonalNeighbourValues(currentCell.Coords)
				.Where(neighbour => unvisitedCells.Contains(neighbour));
			foreach (DijkstraGridCell neighbour in neighbours)
			{
				int totalRiskToReachNeighbour = currentCell.BestTotalDistanceToReachCell + neighbour.Distance;
				if (totalRiskToReachNeighbour < neighbour.BestTotalDistanceToReachCell)
				{
					neighbour.SetBestTotalDistanceToReachCell(totalRiskToReachNeighbour, currentCell);
				}
			}

			// 4. When we are done considering all of the unvisited neighbors of the current node, mark the current
			//    node as visited and remove it from the unvisited set. A visited node will never be checked again.
			unvisitedCells.Remove(currentCell);

			// 5. If the destination node has been marked visited (when planning a route between two specific nodes) or
			//    if the smallest tentative distance among the nodes in the unvisited set is infinity (when planning a
			//    complete traversal; occurs when there is no connection between the initial node and remaining
			//    unvisited nodes), then stop. The algorithm has finished.
			if (!unvisitedCells.Contains(endCell))
			{
				Debug.Log("Best route found! " + endCell.BestTotalDistanceToReachCell);
				
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
					_map.HighlightCellView(cell.x, cell.y, _highlightColor);
				}
				
				break;
			}

			if (unvisitedCells.All(cell => cell.BestTotalDistanceToReachCell == int.MaxValue))
			{
				Debug.Log("No connection through to end cell :(");
				break;
			}
		
			//EditorApplication.QueuePlayerLoopUpdate();
			//yield return null;

			// 6. Otherwise, select the unvisited node that is marked with the smallest tentative distance, set it as
			//    the new current node, and go back to step 3.
			currentCell = unvisitedCells.OrderBy(cell => cell.BestTotalDistanceToReachCell).First();
		}
	}

	protected override void ExecutePuzzle2()
	{
		
	}
}
