using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharGrid : GridBase<char, StaticCellCollection<char>, StaticCellCollection<CellView>>
{
	protected override char ParseValue(string value)
	{
		return value[0];
	}

	protected override bool Compare(char a, char b)
	{
		return a == b;
	}
	
	public void Initialize(string[] gridData, string delimiter = "", char[] ignoreChars = null)
	{
		int numColumns;
		if (!string.IsNullOrEmpty(delimiter))
		{
			numColumns = PuzzleBase.SplitString(gridData[0], delimiter).Length;
		}
		else
		{
			// If there's no delimiter, we'll turn each individual char of the line into a cell
			numColumns = gridData[0].Length;
		}

		int numRows = gridData.Length;
		
		cells = (StaticCellCollection<char>)Activator.CreateInstance(typeof(StaticCellCollection<char>), numColumns, numRows);
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

			for (int column = 0; column < numColumns; column++)
			{
				string cellData = lineCellData[column];
				char cell = ParseValue(cellData);
				if (ignoreChars != null && ignoreChars.Length > 0 && ignoreChars.Contains(cell))
				{
					cell = ' ';
				}
				
				cells[column, row] = cell;
				InitializeCell(column, row, cell);
			}
		}
		
		_enableRendering = true;
		ClearCellViews();
		CreateCellViews();
	}
}
