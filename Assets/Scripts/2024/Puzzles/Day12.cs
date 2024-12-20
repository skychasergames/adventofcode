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
					LogResult(plot.coords + " perimeter", plot.GetPerimeter());
					return plot.GetPerimeter();
				});
				int cost = area * perimeter;
				
				Log("Region of plant type " + region.plantType + " with " + area + " plots, perimeter " + perimeter + ", and cost " + cost);
				totalCost += cost;
			}

			LogResult("Total cost", totalCost);
		}

		protected override void ExecutePuzzle2()
		{
			
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
			public List<GardenPlot> plots;
			public char plantType;
		}

		private class GardenPlot
		{
			public Vector2Int coords;
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
					Vector2Int coords = new Vector2Int(x, y);
					
					// Skip if this plot has already been grouped
					if (regions.Any(region => region.plots.Any(plot => plot.coords == coords)))
					{
						continue;
					}
					
					GardenPlot homePlot = new GardenPlot { coords = coords };
					
					// Recursively group all orthogonal neighbours of the same type
					Region region = new Region
					{
						plots = new List<GardenPlot>(),
						plantType = _gardenPlots.GetCellValue(homePlot.coords)
					};
					
					Color regionColor = Random.ColorHSV(0f, 1f, 0.25f, 0.75f, 1f, 1f);
					
					HighlightAndAddMatchingNeighbours(homePlot);
					void HighlightAndAddMatchingNeighbours(GardenPlot plot)
					{
						region.plots.Add(plot);
						_gardenPlots.HighlightCellView(plot.coords, regionColor);

						plot.extendsNorth = CheckNeighbour(plot.coords + new Vector2Int(0, -1));
						plot.extendsEast = CheckNeighbour(plot.coords + new Vector2Int(1, 0));
						plot.extendsSouth = CheckNeighbour(plot.coords + new Vector2Int(0, 1));
						plot.extendsWest = CheckNeighbour(plot.coords + new Vector2Int(-1, 0));
						
						
						HighlightFences();
						
						// Local method, recursive
						bool CheckNeighbour(Vector2Int neighbourCoords)
						{
							if (_gardenPlots.CellExists(neighbourCoords) && _gardenPlots.GetCellValue(neighbourCoords) == region.plantType)
							{
								if (region.plots.All(groupedPlot => groupedPlot.coords != neighbourCoords))
								{
									HighlightAndAddMatchingNeighbours(new GardenPlot { coords = neighbourCoords });
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
								_fencesHorizontal.HighlightCellView(plot.coords, Color.white);
							}

							if (!plot.extendsEast)
							{
								_fencesVertical.HighlightCellView(plot.coords.x + 1, plot.coords.y, Color.white);
							}

							if (!plot.extendsSouth)
							{
								_fencesHorizontal.HighlightCellView(plot.coords.x, plot.coords.y + 1, Color.white);
							}
							
							if (!plot.extendsWest)
							{
								_fencesVertical.HighlightCellView(plot.coords, Color.white);
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
