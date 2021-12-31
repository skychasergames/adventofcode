using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DijkstraGrid : GridBase<DijkstraGridCell>
{
	protected override DijkstraGridCell ParseValue(string value)
	{
		return new DijkstraGridCell(int.Parse(value));
	}

	protected override bool Compare(DijkstraGridCell a, DijkstraGridCell b)
	{
		return a == b;
	}

	protected override void InitializeCell(int column, int row, DijkstraGridCell cell)
	{
		cell.SetCoords(column, row);
	}
}

public class DijkstraGridCell
{
	public Vector2Int Coords { get; private set; }
	public int Distance { get; }
	public int BestTotalDistanceToReachCell { get; private set; } = int.MaxValue;
	public DijkstraGridCell PreviousCellInPath { get; private set; }

	public DijkstraGridCell(int distance)
	{
		Distance = distance;
	}

	public void SetCoords(int column, int row)
	{
		Coords = new Vector2Int(column, row);
	}

	public void SetBestTotalDistanceToReachCell(int newBestTotalDistance, DijkstraGridCell comingFromCell)
	{
		BestTotalDistanceToReachCell = newBestTotalDistance;
		PreviousCellInPath = comingFromCell;
	}

	public override string ToString()
	{
		return Distance.ToString();
	}
}
