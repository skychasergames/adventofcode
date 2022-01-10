using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmphipodSpace : MonoBehaviour
{
	[SerializeField] private AmphipodSpace _upAdjacent = null;
	[SerializeField] private AmphipodSpace _downAdjacent = null;
	[SerializeField] private AmphipodSpace _leftAdjacent = null;
	[SerializeField] private AmphipodSpace _rightAdjacent = null;
	
	public AmphipodSpace UpAdjacent => _upAdjacent;
	public AmphipodSpace DownAdjacent => _downAdjacent;
	public AmphipodSpace LeftAdjacent => _leftAdjacent;
	public AmphipodSpace RightAdjacent => _rightAdjacent;
	
	public bool IsOccupied { get; private set; }

	public void Initialize(bool isOccupied)
	{
		IsOccupied = isOccupied;
	}

	public void ClearSpace()
	{
		IsOccupied = false;
	}

	public void OccupySpace()
	{
		IsOccupied = true;
	}
}
