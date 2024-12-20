using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AoC2024
{
	public class Day12 : PuzzleBase
	{
		[SerializeField] private CharGrid _gardenPlots = null;
		[SerializeField] private BoolGrid _fencesHorizontal = null;
		[SerializeField] private BoolGrid _fencesVertical = null;
		
		protected override void ExecutePuzzle1()
		{
			_gardenPlots.Initialize(_inputDataLines);
			_fencesHorizontal.Initialize(_gardenPlots.columns, _gardenPlots.rows+1);
			_fencesVertical.Initialize(_gardenPlots.columns+1, _gardenPlots.rows);

			int totalCost = 0;
			
			List<Region> regions = GroupGardenPlotsByRegion();
			foreach (Region region in regions)
			{
				int area = region.plots.Count;
				int perimeter = region.plots.Sum(plot =>
				{
					LogResult(plot.Key + " perimeter", plot.Value.GetPerimeter());
					return plot.Value.GetPerimeter();
				});
				int cost = area * perimeter;
				
				Log("Region of plant type " + region.plantType + " with " + area + " plots, perimeter " + perimeter + ", and cost " + cost);
				totalCost += cost;
			}

			LogResult("Total cost", totalCost);
		}

		protected override void ExecutePuzzle2()
		{
			_gardenPlots.Initialize(_inputDataLines);
			_fencesHorizontal.Initialize(_gardenPlots.columns, _gardenPlots.rows+1);
			_fencesVertical.Initialize(_gardenPlots.columns+1, _gardenPlots.rows);

			int totalCost = 0;
			
			List<Region> regions = GroupGardenPlotsByRegion();
			foreach (Region region in regions)
			{
				int xMin = region.plots.Keys.Min(coords => coords.x);
				int xMax = region.plots.Keys.Max(coords => coords.x);
				int yMin = region.plots.Keys.Min(coords => coords.y);
				int yMax = region.plots.Keys.Max(coords => coords.y);

				int totalSides = 0;
				
				// Check row-by-row for contiguous north and/or south fences
				for (int y = yMin; y <= yMax; y++)
				{
					bool contiguousNorthFence = false;
					bool contiguousSouthFence = false;
					for (int x = xMin; x <= xMax; x++)
					{
						Vector2Int coords = new Vector2Int(x, y);
						if (region.plots.TryGetValue(coords, out GardenPlot plot))
						{
							CheckForContiguousFence(!plot.extendsNorth, ref contiguousNorthFence);
							CheckForContiguousFence(!plot.extendsSouth, ref contiguousSouthFence);
						}
						else
						{
							contiguousNorthFence = false;
							contiguousSouthFence = false;
						}
					}
				}
				
				// Check column-by-column for contiguous east and/or west fences
				for (int x = xMin; x <= xMax; x++)
				{
					bool contiguousEastFence = false;
					bool contiguousWestFence = false;
					for (int y = yMin; y <= yMax; y++)
					{
						Vector2Int coords = new Vector2Int(x, y);
						if (region.plots.TryGetValue(coords, out GardenPlot plot))
						{
							CheckForContiguousFence(!plot.extendsEast, ref contiguousEastFence);
							CheckForContiguousFence(!plot.extendsWest, ref contiguousWestFence);
						}
						else
						{
							contiguousEastFence = false;
							contiguousWestFence = false;
						}
					}
				}
				
				int area = region.plots.Count;
				int cost = area * totalSides;
				
				Log("Region of plant type " + region.plantType + " with " + area + " plots, " + totalSides + " sides, and cost " + cost);
				totalCost += cost;
				
				// Local method
				void CheckForContiguousFence(bool plotFence, ref bool contiguousFence)
				{
					if (plotFence && !contiguousFence)
					{
						contiguousFence = true;
						totalSides++;
					}

					contiguousFence = plotFence;
				}
			}

			LogResult("Total cost", totalCost);
		}
		
		[Button("Reset Map")]
		private void ResetMap()
		{
			_gardenPlots.ClearCellViews();
			_fencesHorizontal.ClearCellViews();
			_fencesVertical.ClearCellViews();
		}

		private struct Region
		{
			public Dictionary<Vector2Int, GardenPlot> plots;
			public char plantType;
		}

		private class GardenPlot
		{
			public bool extendsNorth;
			public bool extendsEast;
			public bool extendsSouth;
			public bool extendsWest;

			public int GetPerimeter()
			{
				return
					(extendsNorth ? 0 : 1) +
					(extendsEast ? 0 : 1) +
					(extendsSouth ? 0 : 1) +
					(extendsWest ? 0 : 1);
			}
		}

		private List<Region> GroupGardenPlotsByRegion()
		{
			List<Region> regions = new List<Region>();
			for (int y = 0; y < _gardenPlots.rows; y++)
			{
				for (int x = 0; x < _gardenPlots.columns; x++)
				{
					Vector2Int homePlotCoords = new Vector2Int(x, y);
					
					// Skip if this plot has already been grouped
					if (regions.Any(region => region.plots.Any(plot => plot.Key == homePlotCoords)))
					{
						continue;
					}
					
					// Recursively group all orthogonal neighbours of the same type
					Region region = new Region
					{
						plots = new Dictionary<Vector2Int, GardenPlot>(),
						plantType = _gardenPlots.GetCellValue(homePlotCoords)
					};
					
					Color regionColor = Random.ColorHSV(0f, 1f, 0.25f, 0.75f, 1f, 1f);
					
					HighlightAndAddMatchingNeighbours(homePlotCoords);
					void HighlightAndAddMatchingNeighbours(Vector2Int coords)
					{
						GardenPlot plot = new GardenPlot();
						region.plots.Add(coords, plot);
						_gardenPlots.HighlightCellView(coords, regionColor);

						plot.extendsNorth = CheckNeighbour(coords + new Vector2Int(0, -1));
						plot.extendsEast = CheckNeighbour(coords + new Vector2Int(1, 0));
						plot.extendsSouth = CheckNeighbour(coords + new Vector2Int(0, 1));
						plot.extendsWest = CheckNeighbour(coords + new Vector2Int(-1, 0));
						
						HighlightFences();
						
						// Local method, recursive
						bool CheckNeighbour(Vector2Int neighbourCoords)
						{
							if (_gardenPlots.CellExists(neighbourCoords) && _gardenPlots.GetCellValue(neighbourCoords) == region.plantType)
							{
								if (!region.plots.ContainsKey(neighbourCoords))
								{
									HighlightAndAddMatchingNeighbours(neighbourCoords);
								}
								
								return true;
							}

							return false;
						}
						
						// Local method
						void HighlightFences()
						{
							if (!plot.extendsNorth)
							{
								_fencesHorizontal.HighlightCellView(coords, Color.white);
							}

							if (!plot.extendsEast)
							{
								_fencesVertical.HighlightCellView(coords.x + 1, coords.y, Color.white);
							}

							if (!plot.extendsSouth)
							{
								_fencesHorizontal.HighlightCellView(coords.x, coords.y + 1, Color.white);
							}
							
							if (!plot.extendsWest)
							{
								_fencesVertical.HighlightCellView(coords, Color.white);
							}
						}
					}

					regions.Add(region);
				}
			}

			return regions;
		}
	}
}
