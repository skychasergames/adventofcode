using System.Collections;
using System.Linq;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

namespace AoC2021
{
	public class Day22 : PuzzleBase
	{
		[SerializeField] private float _stepInterval = 0.5f;
		[SerializeField] private BoundsInt _reactorDimensions = new BoundsInt(-50, -50, -50, 101, 101, 101);
		[SerializeField] private Vector3Int _dimensionsOffset = new Vector3Int(50, 50, 50);

		private EditorCoroutine _puzzleCoroutine = null; 
	
		[Button("Clear Visualization")]
		private void ClearVisualization()
		{
			if (_puzzleCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(_puzzleCoroutine);
				_puzzleCoroutine = null;
			}
		}

		protected override void ExecutePuzzle1()
		{
			ClearVisualization();

			_puzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
		}

		private IEnumerator ExecutePuzzle()
		{
			EditorWaitForSeconds stepInterval = new EditorWaitForSeconds(_stepInterval);
		
			bool[,,] cubes = new bool[_reactorDimensions.size.x, _reactorDimensions.size.y, _reactorDimensions.size.z];
			for (int x = _reactorDimensions.xMin; x < _reactorDimensions.xMax; x++)
			{
				for (int y = _reactorDimensions.yMin; y < _reactorDimensions.yMax; y++)
				{
					for (int z = _reactorDimensions.zMin; z < _reactorDimensions.zMax; z++)
					{
						int xi = x + _dimensionsOffset.x;
						int yi = y + _dimensionsOffset.y;
						int zi = z + _dimensionsOffset.z;
						cubes[xi,yi,zi] = false;
					}
				}
			}

			foreach (string[] lineData in _inputDataLines.Select(line => SplitString(line, " ")))
			{
				bool enable = lineData[0].Equals("on");
				string[] coordData = SplitString(lineData[1], ",");
				GetMinMaxCoordsFromData(coordData[0], out int xMin, out int xMax);
				GetMinMaxCoordsFromData(coordData[1], out int yMin, out int yMax);
				GetMinMaxCoordsFromData(coordData[2], out int zMin, out int zMax);

				if ((xMin < _reactorDimensions.xMin && xMax < _reactorDimensions.xMax) ||
				    (xMin > _reactorDimensions.xMin && xMax > _reactorDimensions.xMax) ||
				    (yMin < _reactorDimensions.yMin && yMax < _reactorDimensions.yMax) ||
				    (yMin > _reactorDimensions.yMin && yMax > _reactorDimensions.yMax) ||
				    (zMin < _reactorDimensions.zMin && zMax < _reactorDimensions.zMax) ||
				    (zMin > _reactorDimensions.zMin && zMax > _reactorDimensions.zMax)
				   )
				{
					Log("Step `" + lineData[0] + " " + lineData[1] + "' was entirely outside of reactor bounds");
					continue;
				}
			
				int cubesChanged = 0;
				for (int x = xMin; x <= xMax; x++)
				{
					for (int y = yMin; y <= yMax; y++)
					{
						for (int z = zMin; z <= zMax; z++)
						{
							int xi = x + _dimensionsOffset.x;
							int yi = y + _dimensionsOffset.y;
							int zi = z + _dimensionsOffset.z;
							if (_reactorDimensions.Contains(new Vector3Int(x,y,z)))
							{
								cubes[xi,yi,zi] = enable;
								cubesChanged++;
							}
							else
							{
								Debug.LogWarning($"Coord is outside grid: {x},{y},{z}");
							}
						}
					}
				}
			
				Log("Step `" + lineData[0] + " " + lineData[1] + "' changed " + cubesChanged + " states");

				yield return stepInterval;
			
				// --- Local method ---
				void GetMinMaxCoordsFromData(string data, out int min, out int max)
				{
					int[] minMax = ParseIntArray(SplitString(SplitString(data, "=")[1], ".."));
					min = minMax[0];
					max = minMax[1];
				}
			}

			int cubesOn = 0;
			foreach (bool cube in cubes)
			{
				if (cube == true)
				{
					cubesOn++;
				}
			}

			LogResult("Total Cubes On", cubesOn);
		
			_puzzleCoroutine = null;
		
		}

		protected override void ExecutePuzzle2()
		{
		
		}
	}
}
