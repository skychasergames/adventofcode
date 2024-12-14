using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AoC2021
{
	public enum AmphipodType
	{
		Amber,
		Bronze,
		Copper,
		Desert
	}

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

		public AmphipodType AmphipodType { get; private set; }
		public AmphipodState CurrentState { get; private set; } = AmphipodState.Uninitialized;
		public AmphipodSpace CurrentSpace { get; private set; } = null;
		public int X => CurrentSpace.X;

		private void Reset()
		{
			_day23 = FindObjectOfType<Day23>();
		}

		public void Uninitialize()
		{
			MoveToSpace(_startingSpace);
			CurrentState = AmphipodState.Uninitialized;
			_amphipodTypeLabel.text = "?";
		}

		public void Initialize(Day23 day23, AmphipodType amphipodType)
		{
			_day23 = day23;
			AmphipodType = amphipodType;
			MoveToSpace(_startingSpace);
			CurrentState = AmphipodState.Unselected;
			_amphipodTypeLabel.text = amphipodType.ToString().Substring(0, 1);
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
			else if (Input.GetKeyDown(KeyCode.R))
			{
				bool canRoomBeEntered = _day23.CanRoomBeEntered(AmphipodType);
				Debug.Log("Can room be entered? " + canRoomBeEntered);
				if (canRoomBeEntered)
				{
					Day23.Move routeToRoom = _day23.BuildRouteToRoom(this);
					Debug.Log("-> Can build path to room? " + (routeToRoom != null));
					if (routeToRoom != null)
					{
						Debug.Log("-> Steps to reach room: " + routeToRoom.Steps.Count);
					}
				}
			}
			else if (Input.GetKeyDown(KeyCode.H))
			{
				if (CurrentSpace.SpaceType == AmphipodSpaceType.Room)
				{
					List<Day23.Move> allMovesForAmphipod = _day23.BuildAllRoutesToHallway(this);
					Debug.Log("Total valid moves to hallway: " + allMovesForAmphipod.Count);
				}
				else
				{
					Debug.Log("Already in the hallway.");
				}
			}
			else if (Input.GetKeyDown(KeyCode.M))
			{
				List<Day23.Move> allPossibleMoves = _day23.GetAllPossibleMoves();
				Debug.Log("ALL valid moves: " + allPossibleMoves.Count);
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
}