using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DijkstraGrid : GridBase<DijkstraGridCell, StaticCellCollection<DijkstraGridCell>, StaticCellCollection<CellView>>
{
	protected override DijkstraGridCell ParseValue(string value)
	{
		return new DijkstraGridCell(int.Parse(value));
	}

	protected override bool Compare(DijkstraGridCell a, DijkstraGridCell b)
	{
		return a == b;
	}
	
	public override void Initialize(int numColumns, int numRows)
	{
		cells = (StaticCellCollection<DijkstraGridCell>)Activator.CreateInstance(typeof(StaticCellCollection<DijkstraGridCell>), numColumns, numRows);
		for (int row = 0; row < rows; row++)
		{
			for (int column = 0; column < numColumns; column++)
			{
				DijkstraGridCell cell = new DijkstraGridCell(false);
				cells[column, row] = cell;
				InitializeCell(column, row, cell);
			}
		}
		
		_enableRendering = true;
		ClearCellViews();
		CreateCellViews();
	}

	protected override void InitializeCell(int column, int row, DijkstraGridCell cell)
	{
		cell.SetCoords(column, row);
	}
}

public class DijkstraGridCell
{
	public Vector2Int Coords { get; private set; }
	public bool IsBlocked { get; private set; }
	public int Distance { get; private set; }
	public int BestTotalDistanceToReachCell { get; private set; } = int.MaxValue;
	public DijkstraGridCell PreviousCellInPath { get; private set; }

	public DijkstraGridCell(int distance)
	{
		Distance = distance;
	}

	public DijkstraGridCell(bool isBlocked)
	{
		IsBlocked = isBlocked;
	}

	public void SetBlocked(bool isBlocked)
	{
		IsBlocked = isBlocked;
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
