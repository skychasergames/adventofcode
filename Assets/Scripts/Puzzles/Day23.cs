using System.Collections.Generic;
using System.IO;
using NaughtyAttributes;
using UnityEngine;
using System.Linq;
using System.Text;

public class Day23 : PuzzleBase
{
	public static readonly Dictionary<AmphipodType, int> EnergyByAmphipodColor = new Dictionary<AmphipodType, int>
	{
		{ AmphipodType.Amber, 1 },
		{ AmphipodType.Bronze, 10 },
		{ AmphipodType.Copper, 100 },
		{ AmphipodType.Desert, 1000 },
	};

	[SerializeField] private List<Amphipod> _amphipods = new List<Amphipod>();
	[SerializeField] private List<AmphipodSpace> _spaces = new List<AmphipodSpace>();
	[SerializeField] private List<Amphipod> _puzzle2AdditionalAmphipods = new List<Amphipod>();
	[SerializeField] private List<AmphipodSpace> _puzzle2AdditionalSpaces = new List<AmphipodSpace>();
	[SerializeField] private string _puzzle2AdditionalAmphipodTypes = "DCBADBAC";
	
	[SerializeField] private Color _graphicColorNormal = Color.white;
	[SerializeField] private Color _graphicColorSelected = Color.yellow;
	[SerializeField] private TMPro.TextMeshPro _energyUsedLabel = null;
	
	[SerializeField] private bool _enableAI = false;
	
	private List<Amphipod> _currentAmphipods = new List<Amphipod>();
	private List<AmphipodSpace> _currentSpaces = new List<AmphipodSpace>();
	private Amphipod _selectedAmphipod = null;
	private int _totalEnergyUsed = 0;
	
	[Button]
	private void ResetVisualization()
	{
		if (_selectedAmphipod != null)
		{
			_selectedAmphipod.ToggleSelected(false, _graphicColorNormal);
		}

		_selectedAmphipod = null;
		_totalEnergyUsed = 0;
		_energyUsedLabel.text = "Click an 'Execute Puzzle' button on Day23 to start";

		foreach (Amphipod amphipod in _amphipods.Union(_puzzle2AdditionalAmphipods))
		{
			amphipod.Uninitialize();
		}
		
		_puzzle2AdditionalSpaces.ForEach(space => space.Initialize(null, true));
	}

	private void Awake()
	{
		ResetVisualization();
	}

	protected override void ExecutePuzzle1()
	{
		ExecutePuzzle(false);
	}

	private void ExecutePuzzle(bool isPuzzle2)
	{
		ResetVisualization();

		_puzzle2AdditionalAmphipods.ForEach(amphipod => amphipod.gameObject.SetActive(isPuzzle2));
		_puzzle2AdditionalSpaces.ForEach(space => space.gameObject.SetActive(isPuzzle2));
		
		_currentAmphipods = _amphipods;
		_currentSpaces = _spaces;
		if (isPuzzle2)
		{
			_currentAmphipods.AddRange(_puzzle2AdditionalAmphipods);
			_currentSpaces.AddRange(_puzzle2AdditionalSpaces);
		}

		StringBuilder amphipodCharsBuilder = new StringBuilder();
		foreach (char c in _inputDataLines[2].Where(char.IsLetter))
		{
			amphipodCharsBuilder.Append(c);
		}

		if (isPuzzle2)
		{
			amphipodCharsBuilder.Append(_puzzle2AdditionalAmphipodTypes);
		}
		
		foreach (char c in _inputDataLines[3].Where(char.IsLetter))
		{
			amphipodCharsBuilder.Append(c);
		}

		string amphipodChars = amphipodCharsBuilder.ToString();
		for (int a = 0; a < amphipodChars.Length; a++)
		{
			var amphipodType = amphipodChars[a] switch
			{
				'A' => AmphipodType.Amber,
				'B' => AmphipodType.Bronze,
				'C' => AmphipodType.Copper,
				'D' => AmphipodType.Desert,
				_ => throw new InvalidDataException("Invalid Amphipod Char: " + amphipodChars[a])
			};

			_currentAmphipods[a].Initialize(this, amphipodType);
		}

		foreach (AmphipodSpace space in _currentSpaces)
		{
			Amphipod occupyingAmphipod = null;
			if (_currentAmphipods.Any(a => a.CurrentSpace == space))
			{
				occupyingAmphipod = _currentAmphipods.First(a => a.CurrentSpace == space);
			}

			space.Initialize(occupyingAmphipod, false);
		}

		_energyUsedLabel.text = "Click an Amphipod and use arrow keys to move";
	}

	public void OnAmphipodClicked(Amphipod amphipod)
	{
		if (_selectedAmphipod != null)
		{
			_selectedAmphipod.ToggleSelected(false, _graphicColorNormal);
		}

		_selectedAmphipod = amphipod;
		_selectedAmphipod.ToggleSelected(true, _graphicColorSelected);
	}

	public void MoveTo(Amphipod amphipod, AmphipodSpace newSpace)
	{
		if (newSpace != null && newSpace.gameObject.activeInHierarchy && newSpace.IsFree)
		{
			amphipod.CurrentSpace.ClearSpace();
			newSpace.OccupySpace(amphipod);
			amphipod.MoveToSpace(newSpace);

			int energyUsed = EnergyByAmphipodColor[amphipod.AmphipodType];
			_totalEnergyUsed += energyUsed;
			_energyUsedLabel.text = _totalEnergyUsed.ToString();
		}
	}

	public class Move
	{
		public Amphipod Amphipod { get; }
		public AmphipodSpace StartSpace { get; }
		public List<AmphipodSpace> Steps { get; }

		public Move(Amphipod amphipod, List<AmphipodSpace> steps)
		{
			Amphipod = amphipod;
			StartSpace = amphipod.CurrentSpace;
			Steps = steps;
		}
	}

	public List<Move> GetPossibleMoves()
	{
		List<Move> possibleMoves = new List<Move>();
		foreach (Amphipod amphipod in _currentAmphipods)
		{
			AmphipodSpace startSpace = amphipod.CurrentSpace;
			if (startSpace.SpaceType == AmphipodSpaceType.Hallway)
			{
				// Have to move to Room (matching amphipod type)
				if (CanRoomBeEntered(amphipod.AmphipodType))
				{
					Move routeToRoom = BuildRouteToRoom(amphipod);
					if (routeToRoom != null)
					{
						possibleMoves.Add(routeToRoom);
					}
				}
				
			}
			else if (startSpace.SpaceType == AmphipodSpaceType.Room && 
			         (startSpace.RoomType != amphipod.AmphipodType || !CanRoomBeEntered(amphipod.AmphipodType)))
			{
				// Can move to Hallway...
				possibleMoves.AddRange(BuildAllRoutesToHallway(amphipod));
				
				// ...or Room (matching amphipod type)
				if (CanRoomBeEntered(amphipod.AmphipodType))
				{
					Move routeToRoom = BuildRouteToRoom(amphipod);
					if (routeToRoom != null)
					{
						possibleMoves.Add(routeToRoom);
					}
				}
			}
		}

		return possibleMoves;
	}

	public bool CanRoomBeEntered(AmphipodType roomType)
	{
		foreach (AmphipodSpace room in _currentSpaces.Where(space => space.SpaceType == AmphipodSpaceType.Room && space.RoomType == roomType))
		{
			if (room.OccupyingAmphipod != null && room.OccupyingAmphipod.AmphipodType != roomType)
			{
				return false;
			}
		}

		return true;
	}

	public List<Move> BuildAllRoutesToHallway(Amphipod amphipod)
	{
		List<Move> allRoutesToHallway = new List<Move>();
		
		AmphipodType amphipodType = amphipod.AmphipodType;
		AmphipodSpace currentSpace = amphipod.CurrentSpace;
		Stack<AmphipodSpace> route = new Stack<AmphipodSpace>();
		bool goLeft = true;
		while (true)
		{
			// Start by moving up to a junction
			AmphipodSpace up = currentSpace.UpAdjacent;
			if (currentSpace.SpaceType == AmphipodSpaceType.Room &&
			    (currentSpace.RoomType != amphipodType || !CanRoomBeEntered(amphipodType)))
			{
				if (up.IsFree)
				{
					route.Push(up);
					currentSpace = up;
					continue;
				}
				
				// Path is blocked
				return allRoutesToHallway;
			}

			// If in the hallway, move left or right until at a valid position to stop
			if (goLeft)
			{
				AmphipodSpace left = currentSpace.LeftAdjacent;
				if (left != null && left.IsFree)
				{
					route.Push(left);
					if (left.SpaceType == AmphipodSpaceType.Hallway)
					{
						allRoutesToHallway.Add(new Move(amphipod, route.Reverse().ToList()));
					}
					
					currentSpace = left;
					continue;
				}
				
				// Reached a dead end going left. Try going right instead.
				currentSpace = amphipod.CurrentSpace;
				route.Clear();
				goLeft = false;
			}
			else
			{
				AmphipodSpace right = currentSpace.RightAdjacent;
				if (right != null && right.IsFree)
				{
					route.Push(right);
					if (right.SpaceType == AmphipodSpaceType.Hallway)
					{
						allRoutesToHallway.Add(new Move(amphipod, route.Reverse().ToList()));
					}
					
					currentSpace = right;
					continue;
				}
				
				// Reached a dead end going right. Done!
				return allRoutesToHallway;
			}
		}
	}

	public Move BuildRouteToRoom(Amphipod amphipod)
	{
		AmphipodType amphipodType = amphipod.AmphipodType;
		AmphipodSpace currentSpace = amphipod.CurrentSpace;
		Stack<AmphipodSpace> route = new Stack<AmphipodSpace>();
		bool goLeft = true;
		while (true)
		{
			// If above the valid room, move down into it
			AmphipodSpace down = currentSpace.DownAdjacent;
			if (down != null && down.SpaceType == AmphipodSpaceType.Room && down.RoomType == amphipodType)
			{
				if (down.IsFree)
				{
					// Move down
					route.Push(down);
					currentSpace = down;
					continue;
				}

				// Stopped on a valid room tile
				return new Move(amphipod, route.Reverse().ToList());
			}
			
			// If starting in an invalid room, start by moving up to junction
			AmphipodSpace up = currentSpace.UpAdjacent;
			if (currentSpace.SpaceType == AmphipodSpaceType.Room && currentSpace.RoomType != amphipodType)
			{
				if (up.IsFree)
				{
					route.Push(up);
					currentSpace = up;
					continue;
				}
				
				// Path is blocked
				return null;
			}

			// If in the hallway, move left or right until above a valid room
			if (goLeft)
			{
				AmphipodSpace left = currentSpace.LeftAdjacent;
				if (left != null && left.IsFree)
				{
					route.Push(left);
					currentSpace = left;
					continue;
				}
				
				// Reached a dead end going left. Try going right instead.
				currentSpace = amphipod.CurrentSpace;
				route.Clear();
				goLeft = false;
			}
			else
			{
				AmphipodSpace right = currentSpace.RightAdjacent;
				if (right != null && right.IsFree)
				{
					route.Push(right);
					currentSpace = right;
					continue;
				}
				
				// Reached a dead end going right. Can't reach room.
				return null;
			}
		}
	}

	protected override void ExecutePuzzle2()
	{
		ExecutePuzzle(true);
	}
}
