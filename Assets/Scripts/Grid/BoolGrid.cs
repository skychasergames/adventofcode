using System;
using System.IO;
using System.Linq;

public class BoolGrid : GridBase<bool, StaticCellCollection<bool>, StaticCellCollection<CellView>>
{
	protected override bool ParseValue(string value)
	{
		return bool.Parse(value);
	}

	protected override bool Compare(bool a, bool b)
	{
		return a == b;
	}

	public void Initialize(string[] gridData, char[] trueChars, char[] falseChars)
	{
		cells = (StaticCellCollection<bool>)Activator.CreateInstance(typeof(StaticCellCollection<bool>), gridData[0].Length, gridData.Length);

		for (int row = 0; row < rows; row++)
		{
			string lineData = gridData[row];
			
			for (int column = 0; column < columns; column++)
			{
				bool cell;
				if (trueChars.Contains(lineData[column]))
				{
					cell = true;
				}
				else if (falseChars.Contains(lineData[column]))
				{
					cell = false;
				}
				else
				{
					throw new InvalidDataException("Character `" + lineData[column] + "` does not appear in either trueChars or falseChars array");
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
