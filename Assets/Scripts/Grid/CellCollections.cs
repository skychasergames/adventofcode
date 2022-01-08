using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CellCollectionBase<TValue> : IEnumerable
{
	protected CellCollectionBase(int numColumns, int numRows)
	{
		columns = numColumns;
		rows = numRows;
	}
	
	protected CellCollectionBase<CellView> _cellViews { get; set; }

	public abstract TValue this[int column, int row] { get; set; }
	public abstract int Length { get; }
	public abstract IEnumerable<TResult> Cast<TResult>();
	public abstract IEnumerator GetEnumerator();
	public int columns { get; protected set; }
	public int rows { get; protected set; }
}

public class StaticCellCollection<TValue> : CellCollectionBase<TValue>
{
	public StaticCellCollection(int numColumns, int numRows) : base(numColumns, numRows)
	{
		cells = new TValue[numColumns, numRows];
	}
	
	public TValue[,] cells { get; protected set; }
	
	public override TValue this[int column, int row]
	{
		get => cells[column, row];
		set => cells[column, row] = value;
	}

	public override int Length => cells.Length;
	
	public override IEnumerable<TResult> Cast<TResult>()
	{
		return cells.Cast<TResult>();
	}

	public override IEnumerator GetEnumerator()
	{
		return cells.GetEnumerator();
	}
}

public class DynamicCellCollection<TValue> : CellCollectionBase<TValue>
{
	public DynamicCellCollection(int numColumns, int numRows) : base(numColumns, numRows)
	{
		cellsByRow = new List<List<TValue>>();
		for (int row = 0; row < numRows; row++)
		{
			List<TValue> list = new List<TValue>();
			for (int column = 0; column < numColumns; column++)
			{
				list.Add(default);
			}
			
			cellsByRow.Add(list);
		}
	}
	
	public List<List<TValue>> cellsByRow { get; protected set; }

	public override int Length => cellsByRow.Sum(row => row.Count);
	
	public override IEnumerable<TResult> Cast<TResult>()
	{
		return cellsByRow.SelectMany(row => row).Cast<TResult>();
	}

	public override IEnumerator GetEnumerator()
	{
		return cellsByRow.GetEnumerator();
	}

	public override TValue this[int column, int row]
	{
		get => cellsByRow[row][column];
		set => cellsByRow[row][column] = value;
	}

	public void InsertRowAt(int row, TValue value = default)
	{
		List<TValue> list = new List<TValue>();
		for (int column = 0; column < columns; column++)
		{
			list.Add(value);
		}
		
		cellsByRow.Insert(row, list);
		rows++;
	}

	public void InsertColumnAt(int insertColumn, TValue value = default)
	{
		foreach (List<TValue> row in cellsByRow)
		{
			row.Insert(insertColumn, value);
		}
		
		columns++;
	}
}
