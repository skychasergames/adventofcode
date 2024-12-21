using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntGrid : GridBase<int, StaticCellCollection<int>, StaticCellCollection<CellView>>
{
	protected override int ParseValue(string value)
	{
		return int.Parse(value);
	}

	protected override bool Compare(int a, int b)
	{
		return a == b;
	}

	public void IncrementCellValue(Vector2Int cell)
	{
		IncrementCellValue(cell.x, cell.y);
	}

	public void IncrementCellValue(int column, int row)
	{
		SetCellValue(column, row, cells[column, row] + 1);
	}

	public void DecrementCellValue(Vector2Int cell)
	{
		DecrementCellValue(cell.x, cell.y);
	}

	public void DecrementCellValue(int column, int row)
	{
		SetCellValue(column, row, cells[column, row] - 1);
	}
}
