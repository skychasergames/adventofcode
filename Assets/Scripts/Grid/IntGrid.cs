using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntGrid : GridBase<int>
{
	protected override int ParseValue(string value)
	{
		return int.Parse(value);
	}

	protected override bool Compare(int a, int b)
	{
		return a == b;
	}
}
