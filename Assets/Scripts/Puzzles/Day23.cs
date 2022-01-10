using System.Collections.Generic;
using System.IO;
using NaughtyAttributes;
using UnityEngine;
using System.Linq;

public class Day23 : PuzzleBase
{
	public enum AmphipodType
	{
		Amber,
		Bronze,
		Copper,
		Desert
	}
	
	public static readonly Dictionary<AmphipodType, int> EnergyByAmphipodColor = new Dictionary<AmphipodType, int>
	{
		{ AmphipodType.Amber, 1 },
		{ AmphipodType.Bronze, 10 },
		{ AmphipodType.Copper, 100 },
		{ AmphipodType.Desert, 1000 },
	};

	[SerializeField] private List<Amphipod> _amphipods = new List<Amphipod>();
	[SerializeField] private List<AmphipodSpace> _spaces = new List<AmphipodSpace>();
	
	[SerializeField] private Color _graphicColorNormal = Color.white;
	[SerializeField] private Color _graphicColorSelected = Color.yellow;
	[SerializeField] private TMPro.TextMeshPro _energyUsedLabel = null;
	
	private Amphipod _currentAmphipod = null;
	private int _totalEnergyUsed = 0;
	
	[Button("Clear Visualization")]
	private void ResetVisualization()
	{
		if (_currentAmphipod != null)
		{
			_currentAmphipod.ToggleSelected(false, _graphicColorNormal);
		}

		_currentAmphipod = null;
		_totalEnergyUsed = 0;
		_energyUsedLabel.text = "Click an 'Execute Puzzle' button on Day23 to start";

		foreach (Amphipod amphipod in _amphipods)
		{
			amphipod.Uninitialize();
		}
	}

	protected override void ExecutePuzzle1()
	{
		ResetVisualization();

		string firstAmphipodRow = _inputDataLines[2];
		string secondAmphipodRow = _inputDataLines[3];
		char[] amphipodChars = firstAmphipodRow.Where(char.IsLetter).Concat(secondAmphipodRow.Where(char.IsLetter)).ToArray();
		for (int a = 0; a < amphipodChars.Length; a++)
		{
			AmphipodType amphipodType;
			switch (amphipodChars[a])
			{
			case 'A': amphipodType = AmphipodType.Amber; break;
			case 'B': amphipodType = AmphipodType.Bronze; break;
			case 'C': amphipodType = AmphipodType.Copper; break;
			case 'D': amphipodType = AmphipodType.Desert; break;
			default:
				throw new InvalidDataException("Invalid Amphipod Char: " + amphipodChars[a]);
			}
			
			_amphipods[a].Initialize(this, amphipodType);
		}

		foreach (AmphipodSpace space in _spaces)
		{
			space.Initialize(_amphipods.Any(a => a.CurrentSpace == space));
		}
	}

	public void OnAmphipodClicked(Amphipod amphipod)
	{
		if (_currentAmphipod != null)
		{
			_currentAmphipod.ToggleSelected(false, _graphicColorNormal);
		}

		_currentAmphipod = amphipod;
		_currentAmphipod.ToggleSelected(true, _graphicColorSelected);
	}

	public void MoveTo(Amphipod amphipod, AmphipodSpace newSpace)
	{
		if (newSpace != null)
		{
			amphipod.CurrentSpace.ClearSpace();
			newSpace.OccupySpace();
			amphipod.MoveToSpace(newSpace);

			int energyUsed = EnergyByAmphipodColor[amphipod.AmphipodType];
			_totalEnergyUsed += energyUsed;
			_energyUsedLabel.text = _totalEnergyUsed.ToString();
		}
	}

	protected override void ExecutePuzzle2()
	{
		
	}
}
