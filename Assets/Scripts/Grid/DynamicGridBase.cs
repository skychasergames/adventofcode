using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class DynamicGridBase<TValue, TCellCollection, TCellViewCollection> : GridBase<TValue, TCellCollection, TCellViewCollection>
	where TCellCollection : DynamicCellCollection<TValue>
	where TCellViewCollection : DynamicCellCollection<CellView>
{
	public void InsertRowAt(int insertRow, TValue value = default)
	{
		// Move existing cell views down
		Vector2 cellOffset = new Vector2(0, -_cellSize.y);
		for (int column = 0; column < columns; column++)
		{
			for (int row = insertRow; row < rows; row++)
			{
				CellView cellView = _cellViews[column, row];
				if (cellView != null)
				{
					RectTransform cellViewRt = cellView.transform as RectTransform;
					if (cellViewRt != null)
					{
						cellViewRt.anchoredPosition += cellOffset;
					}
				}
			}
		}

		// Insert new cells
		cells.InsertRowAt(insertRow, value);
		
		// Create new cell views
		_cellViews.InsertRowAt(insertRow);
		for (int column = 0; column < columns; column++)
		{
			TryCreateCellView(column, insertRow);
			_cellViews[column, insertRow]?.SetText(cells[column, insertRow].ToString());
		}
	}

	public void InsertColumnAt(int insertColumn, TValue value = default)
	{
		// Move existing cell views over
		Vector2 cellOffset = new Vector2(_cellSize.x, 0);
		for (int column = insertColumn; column < columns; column++)
		{
			for (int row = 0; row < rows; row++)
			{
				CellView cellView = _cellViews[column, row];
				if (cellView != null)
				{
					RectTransform cellViewRt = cellView.transform as RectTransform;
					if (cellViewRt != null)
					{
						cellViewRt.anchoredPosition += cellOffset;
					}
				}
			}
		}
		
		// Insert new cells
		cells.InsertColumnAt(insertColumn, value);
		
		// Create new cell views
		_cellViews.InsertColumnAt(insertColumn);
		for (int row = 0; row < rows; row++)
		{
			TryCreateCellView(insertColumn, row);
			_cellViews[insertColumn, row]?.SetText(cells[insertColumn, row].ToString());
		}
	}
}
