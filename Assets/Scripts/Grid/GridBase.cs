using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GridBase<TValue> : MonoBehaviour
{
	[SerializeField] protected CellView _cellViewPrefab = null;

	public int columns { get; protected set; }
	public int rows { get; protected set; }
	public TValue[,] cells { get; protected set; }
	public CellView[,] cellViews { get; protected set; }

	public virtual void Initialize(string[] gridData, string delimiter)
	{
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

		cellViews = new CellView[columns, rows];
		for (int row = 0; row < rows; row++)
		{
			for (int column = 0; column < columns; column++)
			{
				CellView cellView = Instantiate(_cellViewPrefab, transform);
				cellView.SetText(cells[column, row].ToString());
				cellViews[column, row] = cellView;
			}
		}
	}

	protected abstract TValue ParseValue(string value);

	public virtual void HighlightCellView(int column, int row, Color color)
	{
		cellViews[column, row].SetBackgroundColor(color);
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
}
