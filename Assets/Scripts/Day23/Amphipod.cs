using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Amphipod : MonoBehaviour, IPointerClickHandler
{
	public enum AmphipodState
	{
		Uninitialized,
		Unselected,
		Selected
	}
	
	[SerializeField] private SpriteRenderer _graphic = null;
	[SerializeField] private TMPro.TextMeshPro _amphipodTypeLabel = null;
	[SerializeField] private AmphipodSpace _startingSpace = null;
	
	private Day23 _day23 = null;

	public Day23.AmphipodType AmphipodType { get; private set; }
	public AmphipodState CurrentState { get; private set; } = AmphipodState.Uninitialized;
	public AmphipodSpace CurrentSpace { get; private set; }

	private void Reset()
	{
		_day23 = FindObjectOfType<Day23>();
	}

	public void Uninitialize()
	{
		CurrentState = AmphipodState.Uninitialized;
		_amphipodTypeLabel.text = "?";
	}
	
	public void Initialize(Day23 day23, Day23.AmphipodType amphipodType)
	{
		_day23 = day23;
		AmphipodType = amphipodType;
		_amphipodTypeLabel.text = amphipodType.ToString().Substring(0, 1);
		CurrentState = AmphipodState.Unselected;
		MoveToSpace(_startingSpace);
	}

	private void Update()
	{
		if (CurrentState != AmphipodState.Selected)
		{
			return;
		}
	    
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			_day23.MoveTo(this, CurrentSpace.UpAdjacent);
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			_day23.MoveTo(this, CurrentSpace.DownAdjacent);
		}
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			_day23.MoveTo(this, CurrentSpace.LeftAdjacent);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			_day23.MoveTo(this, CurrentSpace.RightAdjacent);
		}
	}

	public void ToggleSelected(bool isSelected, Color graphicColor)
	{
		CurrentState = isSelected ? AmphipodState.Selected : AmphipodState.Unselected;
		_graphic.color = graphicColor;
	}

    public void MoveToSpace(AmphipodSpace newSpace)
    {
	    CurrentSpace = newSpace;
	    transform.position = newSpace.transform.position;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
	    _day23.OnAmphipodClicked(this);
    }
}
