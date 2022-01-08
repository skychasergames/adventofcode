using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

public class Scanner : MonoBehaviour
{
	[SerializeField] private Transform _beaconContainer = null;

	private Transform[] _beaconHolograms = null;
	private List<Transform> _beacons = null;
	private Orientation _currentOrientation = Orientation.PositiveZ0;
	private Vector3 offsetFromBeacon = Vector3.zero;

	public enum Orientation
	{
		PositiveZ0,
		PositiveZ90,
		PositiveZ180,
		PositiveZ270,
		NegativeZ0,
		NegativeZ90,
		NegativeZ180,
		NegativeZ270,
		PositiveX0,
		PositiveX90,
		PositiveX180,
		PositiveX270,
		NegativeX0,
		NegativeX90,
		NegativeX180,
		NegativeX270,
		PositiveY0,
		PositiveY90,
		PositiveY180,
		PositiveY270,
		NegativeY0,
		NegativeY90,
		NegativeY180,
		NegativeY270,
		Count
	}

	public void Initialize(Vector3Int[] localBeaconPositions, Transform beaconHologramPrefab)
	{
		_beaconHolograms = new Transform[localBeaconPositions.Length];
		for (int i = 0; i < localBeaconPositions.Length; i++)
		{
			Transform beaconHologram = Instantiate(beaconHologramPrefab, _beaconContainer);
			beaconHologram.name = beaconHologramPrefab.name + "_" + i;
			beaconHologram.localPosition = localBeaconPositions[i];
			_beaconHolograms[i] = beaconHologram;
		}

		SetOrientation(0);
	}

	public bool ValidPlacementFound { get; private set; } = false;
	public Transform[] OverlappingBeacons { get; private set; } = new Transform[0];

	public IEnumerator TryToFindBeaconOverlaps(List<Transform> lockedBeacons, int requiredOverlapCount, EditorWaitForSeconds orientationInterval)
	{
		ValidPlacementFound = false;

		for (int orientation = 0; orientation < (int)Orientation.Count; orientation++)
		{
			SetOrientation(orientation);
			
			foreach (Transform beaconHologram in _beaconHolograms)
			{
				offsetFromBeacon = beaconHologram.position - transform.position;

				foreach (Transform lockedBeacon in lockedBeacons)
				{
					transform.position = lockedBeacon.position - offsetFromBeacon;

					Transform[] overlappingBeacons = _beaconHolograms
						.Where(beacon => Physics.OverlapSphere(beacon.position, 1).Length > 1)
						.ToArray();
					
					if (overlappingBeacons.Length >= requiredOverlapCount)
					{
						ValidPlacementFound = true;
						OverlappingBeacons = overlappingBeacons;
						yield break;
					}
				}
			}

			yield return orientationInterval;
		}
	}

	public List<Transform> LockInCurrentPosition(Transform beaconPrefab)
	{
		_beacons = new List<Transform>();
		for (int i = 0; i < _beaconHolograms.Length; i++)
		{
			Transform beaconHologram = _beaconHolograms[i];
			if (!OverlappingBeacons.Contains(beaconHologram))
			{
				// Only create a new solid beacon if it doesn't overlap an existing beacon
				Transform beacon = Instantiate(beaconPrefab, _beaconContainer);
				beacon.name = beaconPrefab.name + "_" + i;
				beacon.localPosition = beaconHologram.localPosition;
				beacon.rotation = Quaternion.identity;
				_beacons.Add(beacon);
			}
			
			DestroyImmediate(beaconHologram.gameObject);
		}

		_beaconHolograms = null;
		return _beacons;
	}

	public void SetOrientation(int orientation)
	{
		_currentOrientation = (Orientation)orientation;
		
		string orientationString = _currentOrientation.ToString();
		int sign = orientationString.Substring(0, 8) == "Positive" ? 1 : -1;
		string axis = orientationString.Substring(8, 1);
		int roll = int.Parse(orientationString.Substring(9));

		_beaconContainer.forward = GetForwardVector() * sign;
		_beaconContainer.Rotate(Vector3.forward, roll, Space.Self);

		// --- Local method ---
		Vector3 GetForwardVector()
		{
			switch (axis)
			{
			case "Z": return Vector3.forward;
			case "X": return Vector3.right;
			case "Y": return Vector3.up;
			}

			throw new InvalidDataException("Invalid axis: " + axis);
		}
	}
	
	[Button]
	public void CycleOrientation()
	{
		int orientation = (int)_currentOrientation + 1;
		if (orientation >= (int)Orientation.Count)
		{
			orientation = 0;
		}

		SetOrientation(orientation);
	}
}
