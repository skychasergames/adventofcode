using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridBase<TValue> : MonoBehaviour
{
	[SerializeField] protected CellView _cellViewPrefab = null;

	public int columns { get; protected set; }
	public int rows { get; protected set; }
	public TValue[,] cells { get; protected set; }
	
	protected CellView[,] cellViews { get; set; }
	protected bool _disableRendering = false;

	public virtual void Initialize(int numColumns, int numRows)
	{
		while (transform.childCount > 0)
		{
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
		
		columns = numColumns;
		rows = numRows;
		_disableRendering = (columns >= 100 || rows >= 100);
		
		cells = new TValue[columns, rows];
		
		if (!_disableRendering)
		{
			cellViews = new CellView[columns, rows];
			for (int row = 0; row < rows; row++)
			{
				for (int column = 0; column < columns; column++)
				{
					CellView cellView = Instantiate(_cellViewPrefab, transform);
					cellViews[column, row] = cellView;
					cellView.SetText("");
				}
			}
		}
	}

	public virtual void Initialize(string[] gridData, string delimiter)
	{
		while (transform.childCount > 0)
		{
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
		
		columns = PuzzleBase.SplitString(gridData[0], delimiter).Length;
		rows = gridData.Length;
		
		cells = new TValue[columns, rows];
		for (int row = 0; row < rows; row++)
		{
			string lineData = gridData[row];
			string[] lineCellData = PuzzleBase.SplitString(lineData, delimiter);
			for (int column = 0; column < columns; column++)
			{
				string cellData = lineCellData[column];
				cells[column, row] = ParseValue(cellData);
			}
		}

		if (!_disableRendering)
		{
			cellViews = new CellView[columns, rows];
			for (int row = 0; row < rows; row++)
			{
				for (int column = 0; column < columns; column++)
				{
					CellView cellView = Instantiate(_cellViewPrefab, transform);
					cellViews[column, row] = cellView;

					if (columns >= 100)
					{
						cellView.gameObject.SetActive(false);
					}
					else
					{
						cellView.SetText("");
					}
				}
			}
		}
	}

	protected abstract TValue ParseValue(string value);

	public virtual void HighlightCellView(int column, int row, Color color)
	{
		if (!_disableRendering)
		{
			cellViews[column, row].SetBackgroundColor(color);
		}
	}
	
	public virtual void HighlightRow(int row, Color color)
	{
		for (int column = 0; column < rows; column++)
		{
			HighlightCellView(column, row, color);
		}
	}
	
	public virtual void HighlightColumn(int column, Color color)
	{
		for (int row = 0; row < rows; row++)
		{
			HighlightCellView(column, row, color);
		}
	}

	public virtual void SetCellValue(int column, int row, TValue value)
	{
		cells[column, row] = value;

		if (!_disableRendering)
		{
			cellViews[column, row].SetText(value.ToString());
		}
	}
}
