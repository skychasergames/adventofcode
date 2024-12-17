using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace AoC2024
{
	public class Day8 : PuzzleBase
	{
		[SerializeField] private CharGrid _map = null;
		[SerializeField] private Color _antinodeColor = Color.red;
		
		[Button("Reset Map")]
		private void ResetMap()
		{
			_map.ClearCellViews();
		}


		protected override void ExecutePuzzle1()
		{
			_map.Initialize(_inputDataLines, null, new[] {'.'});

			Dictionary<char, List<Vector2Int>> antennaCellsByFrequency = GetAntennaCellsByFrequency();

			HashSet<Vector2Int> antinodeCells = new HashSet<Vector2Int>();

			// Iterate over antenna types
			foreach (List<Vector2Int> antennaCells in antennaCellsByFrequency.Values)
			{
				// Iterate over each antenna of type
				foreach (Vector2Int antennaCell in antennaCells)
				{
					foreach (Vector2Int otherAntennaCell in antennaCells.Where(otherAntennaCell => otherAntennaCell != antennaCell))
					{
						Vector2Int distance = otherAntennaCell - antennaCell;
						Vector2Int antinodeCell = antennaCell + distance * 2;
						if (_map.CellExists(antinodeCell))
						{
							antinodeCells.Add(antinodeCell);
							_map.HighlightCellView(antinodeCell, _antinodeColor);
						}
					}
				}
			}

			LogResult("Total antinode locations", antinodeCells.Count);
		}

		protected override void ExecutePuzzle2()
		{
			
		}

		private Dictionary<char, List<Vector2Int>> GetAntennaCellsByFrequency()
		{
			Dictionary<char, List<Vector2Int>> antennaCellsByFrequency = new Dictionary<char, List<Vector2Int>>();
			for (int y = 0; y < _map.rows; y++)
			{
				for (int x = 0; x < _map.columns; x++)
				{
					char c = _map.GetCellValue(x, y);
					if (c != ' ')
					{
						if (!antennaCellsByFrequency.ContainsKey(c))
						{
							antennaCellsByFrequency.Add(c, new List<Vector2Int>());
						}

						antennaCellsByFrequency[c].Add(new Vector2Int(x, y));
					}
				}
			}

			return antennaCellsByFrequency;
		}
	}
}
