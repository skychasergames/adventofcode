using System.Collections;
using System.Collections.Generic;
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
}
