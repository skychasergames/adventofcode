using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class GridBase<TValue> : MonoBehaviour
{
	public enum RenderingCullingMode
	{
		DisableAllRendering,
		LimitCellViews
	}
	
	
	[SerializeField] protected CellView _cellViewPrefab = null;
	[SerializeField] protected RenderingCullingMode _renderingCullingMode = RenderingCullingMode.LimitCellViews;
	[SerializeField] protected bool _onlyRenderCellsWithValues = false;
	[SerializeField] protected int _disableRenderingAboveCellCount = 10000;
	[SerializeField] protected Vector2 _cellSize = new Vector2(25, 25);

	public int columns { get; protected set; }
	public int rows { get; protected set; }
	public TValue[,] cells { get; protected set; }
	
	protected CellView[,] _cellViews { get; set; }
	protected int _cellViewCount = 0;
	protected bool _enableRendering = true;
	
	protected abstract TValue ParseValue(string value);
	protected abstract bool Compare(TValue a, TValue b);
	
	#region Initialization
	public virtual void Initialize(int numColumnsAndRows)
	{
		Initialize(numColumnsAndRows, numColumnsAndRows);
	}
	
	public virtual void Initialize(int numColumns, int numRows)
	{
		columns = numColumns;
		rows = numRows;
		cells = new TValue[columns, rows];
		
		_enableRendering = true;
		ClearCellViews();
		CreateCellViews();
	}

	public virtual void Initialize(string[] gridData, string delimiter = "")
	{
		if (!string.IsNullOrEmpty(delimiter))
		{
			columns = PuzzleBase.SplitString(gridData[0], delimiter).Length;
		}
		else
		{
			// If there's no delimiter, we'll turn each individual char of the line into a cell
			columns = gridData[0].Length;
		}

		rows = gridData.Length;
		
		cells = new TValue[columns, rows];
		for (int row = 0; row < rows; row++)
		{
			string lineData = gridData[row];
			
			string[] lineCellData;
			if (!string.IsNullOrEmpty(delimiter))
			{
				lineCellData = PuzzleBase.SplitString(lineData, delimiter);
			}
			else
			{
				// If there's no delimiter, split line into individual chars
				// eg. "12345" becomes { "1", "2", "3", "4", "5" }  
				lineCellData = lineData.ToCharArray().Select(c => c.ToString()).ToArray();
			}

			for (int column = 0; column < columns; column++)
			{
				string cellData = lineCellData[column];
				TValue cell = ParseValue(cellData);
				cells[column, row] = cell;
				InitializeCell(column, row, cell);
			}
		}
		
		_enableRendering = true;
		ClearCellViews();
		CreateCellViews();
	}

	protected virtual void InitializeCell(int column, int row, TValue cell)
	{
		// Do additional setup stuff if required
	}

	public virtual void ClearCellViews()
	{
		while (transform.childCount > 0)
		{
			DestroyImmediate(transform.GetChild(0).gameObject);
		}
		
		_cellViews = new CellView[columns, rows];
		_cellViewCount = 0;
	}

	protected virtual void CreateCellViews()
	{
		switch (_renderingCullingMode)
		{
		case RenderingCullingMode.DisableAllRendering:
			_enableRendering = (rows * columns) <= _disableRenderingAboveCellCount;
			if (!_enableRendering)
			{
				Debug.LogWarning("[Grid] Grid size exceeds maximum CellView count, rendering is disabled.");
			}
			break;
		
		case RenderingCullingMode.LimitCellViews:
			break;
		}
		
		for (int row = 0; row < rows; row++)
		{
			for (int column = 0; column < columns; column++)
			{
				TryCreateCellView(column, row);
				_cellViews[column, row]?.SetText(cells[column, row].ToString());
			}
		}
	}
	
	protected virtual void TryCreateCellView(int column, int row)
	{
		CellView cellView = _cellViews[column, row];
		if (cellView == null)
		{
			// Try create cell view
			if (_enableRendering &&
			    (!_onlyRenderCellsWithValues || !Compare(cells[column, row], default)))
			{
				cellView = Instantiate(_cellViewPrefab, transform);
				_cellViews[column, row] = cellView;
				_cellViewCount++;
			
				RectTransform cellViewRt = cellView.transform as RectTransform;
				if (cellViewRt != null)
				{
					cellViewRt.pivot = new Vector2(0, 1);
					cellViewRt.sizeDelta = _cellSize;
					cellViewRt.anchoredPosition = new Vector2(column * _cellSize.x, row * -_cellSize.y);
				}
			
				_enableRendering = _cellViewCount <= _disableRenderingAboveCellCount;
				if (!_enableRendering)
				{
					Debug.LogWarning("[Grid] CellView limit reached, Grid may not display correctly!");
				}
			}
		}
	}

	public void ResizeGrid(int newColumns, int newRows)
	{
		TValue[,] newCells = new TValue[newColumns, newRows];
		for (int row = 0; row < Mathf.Min(rows, newRows); row++)
		{
			for (int column = 0; column < Mathf.Min(columns, newColumns); column++)
			{
				newCells[column, row] = cells[column, row];
			}
		}

		// Remove CellViews that are outside the new grid area
		if (newColumns < columns)
		{
			for (int row = 0; row < rows; row++)
			{
				for (int column = newColumns; column < columns; column++)
				{
					CellView cellView = _cellViews[column, row];
					if (cellView != null)
					{
						DestroyImmediate(cellView.gameObject);
						_cellViewCount--;
					}
				}
			}
		}

		if (newRows < rows)
		{
			for (int row = newRows; row < rows; row++)
			{
				for (int column = 0; column < columns; column++)
				{
					CellView cellView = _cellViews[column, row];
					if (cellView != null)
					{
						DestroyImmediate(cellView.gameObject);
						_cellViewCount--;
					}
				}
			}
		}

		columns = newColumns;
		rows = newRows;
		cells = newCells;
	}
	#endregion

	#region Cell Data & Views
	public TValue GetCellValue(int column, int row)
	{
		return cells[column, row];
	}

	public TValue GetCellValue(Vector2Int cell)
	{
		return GetCellValue(cell.x, cell.y);
	}

	public virtual void SetCellValue(int column, int row, TValue value)
	{
		cells[column, row] = value;
		
		TryCreateCellView(column, row);
		_cellViews[column, row]?.SetText(value.ToString());
	}

	public void SetCellValue(Vector2Int cell, TValue value)
	{
		SetCellValue(cell.x, cell.y, value);
	}

	public virtual void HighlightCellView(int column, int row, Color color)
	{
		TryCreateCellView(column, row);
		_cellViews[column, row]?.SetBackgroundColor(color);
	}
	
	public void HighlightCellView(Vector2Int cell, Color color)
	{
		HighlightCellView(cell.x, cell.y, color);
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
	#endregion

	#region Neighbours
	public List<TValue> GetOrthogonalNeighbourValues(int column, int row)
	{
		return GetOrthogonalNeighbourCoords(column, row).Select(coord => cells[coord.x, coord.y]).ToList();
	}

	public List<TValue> GetOrthogonalNeighbourValues(Vector2Int cell)
	{
		return GetOrthogonalNeighbourValues(cell.x, cell.y);
	}

	public List<Vector2Int> GetOrthogonalNeighbourCoords(int column, int row)
	{
		List<Vector2Int> neighbours = new List<Vector2Int>();
		
		// Left
		if (column > 0)
		{
			neighbours.Add(new Vector2Int(column - 1, row));
		}
		
		// Right
		if (column < columns - 1)
		{
			neighbours.Add(new Vector2Int(column + 1, row));
		}
		
		// Above
		if (row > 0)
		{
			neighbours.Add(new Vector2Int(column, row - 1));
		}
		
		// Below
		if (row < rows - 1)
		{
			neighbours.Add(new Vector2Int(column, row + 1));
		}

		return neighbours;
	}

	public List<Vector2Int> GetOrthogonalNeighbourCoords(Vector2Int cell)
	{
		return GetOrthogonalNeighbourCoords(cell.x, cell.y);
	}

	public List<TValue> GetDiagonalNeighbourValues(int column, int row)
	{
		return GetDiagonalNeighbourCoords(column, row).Select(coord => cells[coord.x, coord.y]).ToList();
	}

	public List<TValue> GetDiagonalNeighbourValues(Vector2Int cell)
	{
		return GetDiagonalNeighbourValues(cell.x, cell.y);
	}

	public List<Vector2Int> GetDiagonalNeighbourCoords(int column, int row)
	{
		List<Vector2Int> neighbours = new List<Vector2Int>();
		
		// Up and left
		if (column > 0 && row > 0)
		{
			neighbours.Add(new Vector2Int(column - 1, row - 1));
		}
		
		// Up and right
		if (column < columns - 1 && row > 0)
		{
			neighbours.Add(new Vector2Int(column + 1, row - 1));
		}
		
		// Down and left
		if (column > 0 && row < rows - 1)
		{
			neighbours.Add(new Vector2Int(column - 1, row + 1));
		}
		
		// Down and right
		if (column < columns - 1 && row < rows - 1)
		{
			neighbours.Add(new Vector2Int(column + 1, row + 1));
		}

		return neighbours;
	}

	public List<Vector2Int> GetDiagonalNeighbourCoords(Vector2Int cell)
	{
		return GetDiagonalNeighbourCoords(cell.x, cell.y);
	}

	public List<TValue> GetAllNeighbourValues(int column, int row)
	{
		return GetAllNeighbourCoords(column, row).Select(coord => cells[coord.x, coord.y]).ToList();
	}

	public List<TValue> GetAllNeighbourValues(Vector2Int cell)
	{
		return GetAllNeighbourValues(cell.x, cell.y);
	}
	
	public List<Vector2Int> GetAllNeighbourCoords(int column, int row)
	{
		List<Vector2Int> neighbours = new List<Vector2Int>();
		neighbours.AddRange(GetOrthogonalNeighbourCoords(column, row));
		neighbours.AddRange(GetDiagonalNeighbourCoords(column, row));
		return neighbours;
	}
	
	public List<Vector2Int> GetAllNeighbourCoords(Vector2Int cell)
	{
		return GetAllNeighbourCoords(cell.x, cell.y);
	}

	public Vector2Int GetCoordsOfCellValue(TValue cellValue)
	{
		for (int row = 0; row < rows; row++)
		{
			for (int column = 0; column < columns; column++)
			{
				if (Compare(cells[column, row], cellValue))
				{
					return new Vector2Int(column, row);
				}
			}
		}

		throw new NullReferenceException("Cell value " + cellValue + " was not found in grid");
	}
	#endregion
}
