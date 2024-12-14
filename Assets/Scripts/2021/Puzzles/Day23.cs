using System.Collections;
using System.Collections.Generic;
using System.IO;
using NaughtyAttributes;
using UnityEngine;
using System.Linq;
using System.Text;
using Unity.EditorCoroutines.Editor;

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
	[SerializeField] private float _AIStepInterval = 0.2f;
	[SerializeField] private float _AIMoveInterval = 0.5f;
	[SerializeField] private float _AIEnergyBands = 1000;
	[SerializeField] private int _AICorrectRoomFitness = 100;
	[SerializeField] private int _AIEmptyRoomFitness = 10;
	
	private List<Amphipod> _currentAmphipods = new List<Amphipod>();
	private List<AmphipodSpace> _currentSpaces = new List<AmphipodSpace>();
	private Dictionary<AmphipodType, List<AmphipodSpace>> _currentRoomsByType = new Dictionary<AmphipodType, List<AmphipodSpace>>();
	private Amphipod _selectedAmphipod = null;
	private int _totalEnergyUsed = 0;
	
	private List<GameInstance> _gameInstancesToRun = new List<GameInstance>();
	private Dictionary<int, List<GameState>> _previousGameStatesByBand = new Dictionary<int, List<GameState>>();
	private EditorCoroutine _aiCoroutine = null;
	private int _bestTotalEnergyUsed = int.MaxValue;
		
	public EditorWaitForSeconds AIStepInterval { get; private set; }
	public EditorWaitForSeconds AIMoveInterval { get; private set; }

	[ShowNativeProperty] public int GameInstancesToRun { get; private set; }
	[ShowNativeProperty] public int PreviousGameStates { get; private set; }

	[Button]
	private void ResetVisualization()
	{
		if (_aiCoroutine != null)
		{
			EditorCoroutineUtility.StopCoroutine(_aiCoroutine);
			_aiCoroutine = null;
		}
		
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
		
		_currentAmphipods = new List<Amphipod>(_amphipods);
		_currentSpaces = new List<AmphipodSpace>(_spaces);
		if (isPuzzle2)
		{
			_currentAmphipods.AddRange(_puzzle2AdditionalAmphipods);
			_currentSpaces.AddRange(_puzzle2AdditionalSpaces);
		}

		_currentRoomsByType = new Dictionary<AmphipodType, List<AmphipodSpace>>();
		foreach (AmphipodSpace space in _currentSpaces)
		{
			if (space.SpaceType == AmphipodSpaceType.Room)
			{
				if (!_currentRoomsByType.ContainsKey(space.RoomType))
				{
					_currentRoomsByType.Add(space.RoomType, new List<AmphipodSpace>());
				}

				_currentRoomsByType[space.RoomType].Add(space);
			}
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

		if (_enableAI)
		{
			AIStepInterval = new EditorWaitForSeconds(_AIStepInterval);
			AIMoveInterval = new EditorWaitForSeconds(_AIMoveInterval);
			_bestTotalEnergyUsed = int.MaxValue;
			_energyUsedLabel.text = "AI starting...";
			Debug.Log("AI starting...");
			
			_gameInstancesToRun.Clear();
			GameInstancesToRun = 0;
			
			_previousGameStatesByBand.Clear();
			PreviousGameStates = 0;
			
			Dictionary<Amphipod, AmphipodSpace> spacesByAmphipod = _currentAmphipods.ToDictionary(amphipod => amphipod, amphipod => amphipod.CurrentSpace);
			GameInstance firstGameInstance = new GameInstance(this, new List<Move>(), new GameState(spacesByAmphipod, 0));
			_aiCoroutine = EditorCoroutineUtility.StartCoroutine(firstGameInstance.Continue(), this);
		}
		else
		{
			_energyUsedLabel.text = "Click an Amphipod and use arrow keys to move";
		}
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

			if (!_enableAI)
			{
				_energyUsedLabel.text = _totalEnergyUsed.ToString();
			}
		}
	}

	public IEnumerator ExecuteMove(Move move)
	{
		//OnAmphipodClicked(move.Amphipod);
		foreach (AmphipodSpace space in move.Steps)
		{
			MoveTo(move.Amphipod, space);

			if (AIStepInterval.WaitTime > 0)
			{
				yield return AIStepInterval;
			}
		}
	}

	private bool GameStateMatchesPreviousState(GameState newState, int energyBand)
	{
		if (_previousGameStatesByBand.TryGetValue(energyBand, out List<GameState> gameStates))
		{
			return gameStates.Any(previousState => CompareGameStates(previousState, newState));
		}

		return false;
	}

	private bool CompareGameStates(GameState a, GameState b)
	{
		if (a.TotalEnergyUsed != b.TotalEnergyUsed)
		{
			return false;
		}
		
		for (int i = 0; i < a.SpacesByAmphipod.Count; i++)
		{
			if (a.SpacesByAmphipod[_currentAmphipods[i]] != b.SpacesByAmphipod[_currentAmphipods[i]])
			{
				return false;
			}
		}

		return true;
	}

	public void RecordGameState(GameState gameState, int energyBand)
	{
		if (_previousGameStatesByBand.TryGetValue(energyBand, out List<GameState> gameStates))
		{
			gameStates.Add(gameState);
		}
		else
		{
			_previousGameStatesByBand.Add(energyBand, new List<GameState> { gameState });
		}
		PreviousGameStates++;
	}
	
	public void DivergeGameInstance(GameInstance newGameInstance)
	{
		_gameInstancesToRun.Add(newGameInstance);
		GameInstancesToRun++;
	}
	
	public IEnumerator GameInstanceFinished(GameInstance finishedInstance)
	{
		GameInstancesToRun -= _gameInstancesToRun.RemoveAll(instance => instance.GameState.TotalEnergyUsed >= _bestTotalEnergyUsed);
		
		if (IsGameWon())
		{
			if (_totalEnergyUsed < _bestTotalEnergyUsed)
			{
				_bestTotalEnergyUsed = _totalEnergyUsed;
				_energyUsedLabel.text = _bestTotalEnergyUsed.ToString();
				Debug.Log("New best completed with score " + _totalEnergyUsed + "! " + _gameInstancesToRun.Count + " instances remaining.");
			}
			else
			{
				//Debug.Log("Completed with score " + _totalEnergyUsed + ". " + _gameInstancesToRun.Count + " instances remaining.");
			}
		}
		else
		{
			//Debug.Log("Game instance failed. " + _gameInstancesToRun.Count + " instances remaining.");
		}

		if (_gameInstancesToRun.Count > 0)
		{
			// Start next instance
			GameInstance nextInstance = _gameInstancesToRun
				.OrderByDescending(instance => instance.Moves.Sum(move => move.Fitness))
				.First();
			
			_gameInstancesToRun.Remove(nextInstance);
			GameInstancesToRun--;
			
			yield return ResumeGameInstance(nextInstance);
		}
		else
		{
			Debug.Log("Simulation finished.");
			EditorCoroutineUtility.StopCoroutine(_aiCoroutine);
			_aiCoroutine = null;
			yield break;
		}
	}

	private IEnumerator ResumeGameInstance(GameInstance gameInstance)
	{
		// Reset game state
		foreach (Amphipod amphipod in _currentAmphipods)
		{
			amphipod.Initialize(this, amphipod.AmphipodType);
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

		_totalEnergyUsed = 0;

		// Redo the initial moves
		foreach (Move move in gameInstance.Moves)
		{
			foreach (AmphipodSpace step in move.Steps)
			{
				MoveTo(move.Amphipod, step);
			}
		}

		// Pick up where we left off
		yield return gameInstance.Continue();
	}

	public class GameInstance
	{
		private Day23 _day23;
		public GameState GameState { get; private set; }
		public List<Move> Moves { get; }

		public GameInstance(Day23 day23, List<Move> moves, GameState gameState)
		{
			_day23 = day23;
			Moves = moves;
			GameState = gameState;
		}
		
		public IEnumerator Continue()
		{
			if (_day23.AIMoveInterval.WaitTime > 0)
			{
				yield return _day23.AIMoveInterval;
			}

			List<Move> allPossibleMoves = _day23.GetAllPossibleMoves();
			if (allPossibleMoves.Count == 0)
			{
				yield return _day23.GameInstanceFinished(this);
				yield break;
			}

			for (int i = 1; i < allPossibleMoves.Count; i++)
			{
				// Go backwards through moves so that we queue the "worse" moves to be calculated later
				Move alternateMove = allPossibleMoves[i];
				GameState alternateGameState = GenerateResultingGameStateForMove(alternateMove);
				int alternateStateEnergyBand = Mathf.FloorToInt(alternateGameState.TotalEnergyUsed / _day23._AIEnergyBands);

				if (alternateGameState.TotalEnergyUsed <= _day23._bestTotalEnergyUsed &&
				    !_day23.GameStateMatchesPreviousState(alternateGameState, alternateStateEnergyBand))
				{
					_day23.RecordGameState(alternateGameState, alternateStateEnergyBand);
					
					List<Move> alternateMoveList = new List<Move>(Moves);
					alternateMoveList.Add(alternateMove);
					
					GameInstance newGameInstance = new GameInstance(_day23, alternateMoveList, alternateGameState);
					_day23.DivergeGameInstance(newGameInstance);
				}
			}

			Move nextMove = allPossibleMoves[0];
			GameState nextGameState = GenerateResultingGameStateForMove(nextMove);
			int nextStateEnergyBand = Mathf.FloorToInt(nextGameState.TotalEnergyUsed / _day23._AIEnergyBands);

			if (nextGameState.TotalEnergyUsed <= _day23._bestTotalEnergyUsed &&
			    !_day23.GameStateMatchesPreviousState(nextGameState, nextStateEnergyBand))
			{
				_day23.RecordGameState(nextGameState, nextStateEnergyBand);
				
				GameState = nextGameState;
				Moves.Add(nextMove);
				
				yield return _day23.ExecuteMove(nextMove);
				yield return Continue();
			}
			else
			{
				// Abort if we've already seen this game state
				yield return _day23.GameInstanceFinished(this);
			}
		}

		private GameState GenerateResultingGameStateForMove(Move move)
		{
			Dictionary<Amphipod, AmphipodSpace> newSpacesByAmphipod = new Dictionary<Amphipod, AmphipodSpace>(GameState.SpacesByAmphipod);
			newSpacesByAmphipod[move.Amphipod] = move.Steps.Last();
			return new GameState(newSpacesByAmphipod, GameState.TotalEnergyUsed + move.EnergyCost);
		}
	}

	public class GameState
	{
		public Dictionary<Amphipod, AmphipodSpace> SpacesByAmphipod { get; }
		public int TotalEnergyUsed { get; private set; }

		public GameState(Dictionary<Amphipod, AmphipodSpace> spacesByAmphipod, int totalEnergyUsed)
		{
			SpacesByAmphipod = spacesByAmphipod;
			TotalEnergyUsed = totalEnergyUsed;
		}
	}

	public class Move
	{
		public Amphipod Amphipod { get; }
		public AmphipodSpace StartSpace { get; }
		public List<AmphipodSpace> Steps { get; }
		public int Fitness { get; }
		public int EnergyCost { get; }

		public Move(Amphipod amphipod, List<AmphipodSpace> steps, int fitness)
		{
			Amphipod = amphipod;
			StartSpace = amphipod.CurrentSpace;
			Steps = steps;
			Fitness = fitness;
			EnergyCost = steps.Count * EnergyByAmphipodColor[amphipod.AmphipodType];
		}
	}

	public List<Move> GetAllPossibleMoves()
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
			else if (startSpace.SpaceType == AmphipodSpaceType.Room)
			{
				if (startSpace.RoomType != amphipod.AmphipodType)
				{
					// Currently in an invalid room
					// Can move to matching room...
					if (CanRoomBeEntered(amphipod.AmphipodType))
					{
						Move routeToRoom = BuildRouteToRoom(amphipod);
						if (routeToRoom != null)
						{
							possibleMoves.Add(routeToRoom);
						}
					}

					// ...or a Hallway
					possibleMoves.AddRange(BuildAllRoutesToHallway(amphipod));
				}
				else if (!CanRoomBeEntered(amphipod.AmphipodType))
				{
					// Currently in matching room, but room contains invalid amphipods
					// Can move to a hallway only
					possibleMoves.AddRange(BuildAllRoutesToHallway(amphipod));
				}
			}
		}

		return possibleMoves;
	}

	public bool CanRoomBeEntered(AmphipodType roomType)
	{
		return _currentRoomsByType[roomType].All(room =>
			room.OccupyingAmphipod == null ||
			room.OccupyingAmphipod.AmphipodType == roomType
		);
	}

	public bool AmphipodShouldGoLeftFirst(Amphipod amphipod)
	{
		AmphipodSpace room = _currentRoomsByType[amphipod.AmphipodType].First();
		float difference = room.X - amphipod.X;
		if (difference > 0.5f)
		{
			return false;
		}

		if (difference < -0.5f)
		{
			return true;
		}

		// If we get here, it means the amphipod is already in/above their correct room
		// In order to better prioritize routes, A/B amphipods should go left and C/D amphipods should go right
		return (amphipod.AmphipodType == AmphipodType.Amber || amphipod.AmphipodType == AmphipodType.Bronze);
	}

	public Move BuildRouteToRoom(Amphipod amphipod)
	{
		AmphipodType amphipodType = amphipod.AmphipodType;
		AmphipodSpace currentSpace = amphipod.CurrentSpace;
		
		// Grant an increasing bonus to fitness based on how full target room is
		int correctRoomFitnessModifier = (_currentRoomsByType[amphipodType].Count + 1) * _AICorrectRoomFitness;
		
		Stack<AmphipodSpace> route = new Stack<AmphipodSpace>();
		bool goLeftFirst = AmphipodShouldGoLeftFirst(amphipod);
		bool checkFirstDirection = true;
		while (true)
		{
			// If above the valid room, move down into it
			AmphipodSpace down = currentSpace.DownAdjacent;
			if (down != null && down.SpaceType == AmphipodSpaceType.Room && down.RoomType == amphipodType)
			{
				if (down.IsFree)
				{
					// Move down
					correctRoomFitnessModifier -= _AICorrectRoomFitness;
					route.Push(down);
					currentSpace = down;
					continue;
				}

				// Stopped on a valid room tile
				return new Move(amphipod, route.Reverse().ToList(), correctRoomFitnessModifier);
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
			if (checkFirstDirection)
			{
				AmphipodSpace adjacent = goLeftFirst ? currentSpace.LeftAdjacent : currentSpace.RightAdjacent;
				if (adjacent != null && adjacent.IsFree)
				{
					route.Push(adjacent);
					currentSpace = adjacent;
					continue;
				}
				
				// Reached a dead end going left. Try going right instead.
				currentSpace = amphipod.CurrentSpace;
				route.Clear();
				checkFirstDirection = false;
			}
			else
			{
				AmphipodSpace adjacent = goLeftFirst ? currentSpace.RightAdjacent : currentSpace.LeftAdjacent;
				if (adjacent != null && adjacent.IsFree)
				{
					route.Push(adjacent);
					currentSpace = adjacent;
					continue;
				}
				
				// Reached a dead end going right. Can't reach room.
				return null;
			}
		}
	}

	public List<Move> BuildAllRoutesToHallway(Amphipod amphipod)
	{
		List<Move> allRoutesToHallway = new List<Move>();
		
		AmphipodType amphipodType = amphipod.AmphipodType;
		AmphipodSpace currentSpace = amphipod.CurrentSpace;
		int targetRoomX = _currentRoomsByType[amphipodType].First().X;

		// Grant an increasing bonus to fitness if moving out of room
		int emptyRoomFitnessModifier = 0;
		if (currentSpace.SpaceType == AmphipodSpaceType.Room &&
		    currentSpace.RoomType != amphipodType)
		{
			foreach (AmphipodSpace room in _currentRoomsByType[amphipodType])
			{
				if (room.IsFree)
				{
					emptyRoomFitnessModifier += _AIEmptyRoomFitness;
				}
				else if (room == currentSpace)
				{
					emptyRoomFitnessModifier += _AIEmptyRoomFitness;
					break;
				}
				else
				{
					break;
				}
			}
		}

		Stack<AmphipodSpace> route = new Stack<AmphipodSpace>();
		bool goLeftFirst = AmphipodShouldGoLeftFirst(amphipod);
		bool checkFirstDirection = true;
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
			if (checkFirstDirection)
			{
				AmphipodSpace adjacent = goLeftFirst ? currentSpace.LeftAdjacent : currentSpace.RightAdjacent;
				if (adjacent != null && adjacent.IsFree)
				{
					route.Push(adjacent);
					if (adjacent.SpaceType == AmphipodSpaceType.Hallway)
					{
						int fitness = 8 - Mathf.Abs(adjacent.X - targetRoomX) + emptyRoomFitnessModifier;
						allRoutesToHallway.Add(new Move(amphipod, route.Reverse().ToList(), fitness));
					}
					
					currentSpace = adjacent;
					continue;
				}
				
				// Reached a dead end going left. Try going right instead.
				currentSpace = amphipod.CurrentSpace;
				route.Clear();
				checkFirstDirection = false;
			}
			else
			{
				AmphipodSpace adjacent = goLeftFirst ? currentSpace.RightAdjacent : currentSpace.LeftAdjacent;
				if (adjacent != null && adjacent.IsFree)
				{
					route.Push(adjacent);
					if (adjacent.SpaceType == AmphipodSpaceType.Hallway)
					{
						int fitness = 8 - Mathf.Abs(adjacent.X - targetRoomX) + emptyRoomFitnessModifier;
						allRoutesToHallway.Add(new Move(amphipod, route.Reverse().ToList(), fitness));
					}
					
					currentSpace = adjacent;
					continue;
				}
				
				// Reached a dead end going right. Done!
				return allRoutesToHallway;
			}
		}
	}

	public bool IsGameWon()
	{
		foreach (Amphipod amphipod in _currentAmphipods)
		{
			if (amphipod.CurrentSpace.SpaceType != AmphipodSpaceType.Room || 
			    amphipod.CurrentSpace.RoomType != amphipod.AmphipodType)
			{
				return false;
			}
		}

		return true;
	}

	protected override void ExecutePuzzle2()
	{
		ExecutePuzzle(true);
	}
}
