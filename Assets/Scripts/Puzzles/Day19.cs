using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NaughtyAttributes;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using System.Linq;

public class Day19 : PuzzleBase
{
	[SerializeField] private Scanner _scannerPrefab = null;
	[SerializeField] private Transform _beaconHologramPrefab = null;
	[SerializeField] private Transform _beaconPrefab = null;
	
	[SerializeField] private Transform _scannerContainer = null;
	[SerializeField] private int _requiredOverlappingBeaconCount = 12;
	
	[SerializeField] private float _scannerInterval = 0.2f;
	[SerializeField] private float _orientationInterval = 0.1f;

	private EditorCoroutine _puzzleCoroutine = null; 
	
	[Button("Clear Visualization")]
	private void ClearVisualization()
	{
		if (_puzzleCoroutine != null)
		{
			EditorCoroutineUtility.StopCoroutine(_puzzleCoroutine);
			_puzzleCoroutine = null;
		}
		
		while (_scannerContainer.childCount > 0)
		{
			DestroyImmediate(_scannerContainer.GetChild(0).gameObject);
		}
	}

	protected override void ExecutePuzzle1()
	{
		ClearVisualization();

		_puzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
	}

	private List<Scanner> ParseScanners()
	{
		List<Scanner> scanners = new List<Scanner>();
		Scanner currentScanner = null;
		List<Vector3Int> currentScannerBeaconCoords = new List<Vector3Int>();
		foreach (string line in _inputDataLines)
		{
			if (line.StartsWith("---"))
			{
				if (currentScanner != null)
				{
					// Finish current scanner
					currentScanner.Initialize(currentScannerBeaconCoords.ToArray(), _beaconHologramPrefab);
					currentScanner.gameObject.SetActive(false);
					scanners.Add(currentScanner);
				}
				
				// Start new scanner
				currentScanner = Instantiate(_scannerPrefab, _scannerContainer);
				currentScanner.name = _scannerPrefab.name + "_" + scanners.Count;
				currentScannerBeaconCoords = new List<Vector3Int>();
			}
			else
			{
				// Add beacon coord to current scanner
				string[] coordStrings = line.Split(',');
				currentScannerBeaconCoords.Add(
					new Vector3Int(
						int.Parse(coordStrings[0]),	
						int.Parse(coordStrings[1]),	
						int.Parse(coordStrings[2])	
					)	
				);
			}
		}

		if (currentScanner != null)
		{
			// Finish final scanner
			currentScanner.Initialize(currentScannerBeaconCoords.ToArray(), _beaconHologramPrefab);
			currentScanner.gameObject.SetActive(false);
			scanners.Add(currentScanner);
		}

		return scanners;
	}

	private IEnumerator ExecutePuzzle()
	{
		EditorWaitForSeconds scannerInterval = new EditorWaitForSeconds(_scannerInterval);
		EditorWaitForSeconds orientationInterval = new EditorWaitForSeconds(_orientationInterval);
		
		List<Scanner> remainingScanners = ParseScanners();
		List<Scanner> calculatedScanners = new List<Scanner>();
		List<Transform> lockedBeacons = new List<Transform>();
		
		Scanner firstScanner = remainingScanners[0];
		firstScanner.gameObject.SetActive(true);
		LockInScanner(firstScanner);

		while (remainingScanners.Count > 0)
		{
			yield return scannerInterval;
			
			// Find a scanner which overlaps at least 12 locked beacons
			Scanner overlappingScanner = null;
			foreach (Scanner checkScanner in remainingScanners)
			{
				checkScanner.gameObject.SetActive(true);

				// Check all permutations of beacon orientation and position
				yield return checkScanner.TryToFindBeaconOverlaps(lockedBeacons, _requiredOverlappingBeaconCount, orientationInterval);
				
				if (checkScanner.ValidPlacementFound)
				{
					// We found an overlapping scanner!
					overlappingScanner = checkScanner;
					break;
				}
				
				// This scanner doesn't overlap, turn it off again 
				checkScanner.gameObject.SetActive(false);
			}

			if (overlappingScanner != null)
			{
				LockInScanner(overlappingScanner);
				continue;
			}

			throw new UnityException("Failed to find any overlapping scanners.");
		}

		LogResult("Total beacons", lockedBeacons.Count);
		
		_puzzleCoroutine = null;
		
		// --- Local method ---
		void LockInScanner(Scanner scanner)
		{
			LogResult("Locking in scanner", scanner.name);
			List<Transform> newBeacons = scanner.LockInCurrentPosition(_beaconPrefab);
			lockedBeacons.AddRange(newBeacons);
			
			calculatedScanners.Add(scanner);
			remainingScanners.Remove(scanner);
		}
	}

	protected override void ExecutePuzzle2()
	{
		
	}
}
