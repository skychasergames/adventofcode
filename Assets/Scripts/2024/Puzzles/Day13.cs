using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace AoC2024
{
	public class Day13 : PuzzleBase
	{
		private const string REGEX_PATTERN = @"X(\+|\=)(?<X>\d+), Y(\+|\=)(?<Y>\d+)";
		private const int BUTTON_A_COST = 3;
		private const int BUTTON_B_COST = 1;
		private const int MAX_PRESSES_PER_BUTTON = 100;

		protected override void ExecutePuzzle1()
		{
			int totalTokenCost = 0;
			
			for (int line = 0; line < _inputDataLines.Length; line += 3)
			{
				ParseClawMachine(line, out Vector2Int buttonAMovement, out Vector2Int buttonBMovement, out Vector2Int prizePosition);
				
				for (int buttonBPresses = MAX_PRESSES_PER_BUTTON; buttonBPresses >= 0; buttonBPresses--)
				{
					if (CanButtonCombinationGetPrize(buttonAMovement, buttonBMovement, prizePosition, buttonBPresses, out int tokenCost))
					{
						Log("Found prize for " + tokenCost + " tokens (" + buttonBPresses + " presses of Button B)");
						totalTokenCost += tokenCost;
						break;
					}
				}
			}
			LogResult("Total token cost", totalTokenCost);
		}

		protected override void ExecutePuzzle2()
		{
			
		}

		private void ParseClawMachine(int line, out Vector2Int buttonAMovement, out Vector2Int buttonBMovement, out Vector2Int prizePosition)
		{
			Regex regex = new Regex(REGEX_PATTERN);
			Match buttonAMatches = regex.Match(_inputDataLines[line]);
			Match buttonBMatches = regex.Match(_inputDataLines[line+1]);
			Match prizeMatches = regex.Match(_inputDataLines[line+2]);
			buttonAMovement = new Vector2Int(int.Parse(buttonAMatches.Result("${X}")), int.Parse(buttonAMatches.Result("${Y}")));
			buttonBMovement = new Vector2Int(int.Parse(buttonBMatches.Result("${X}")), int.Parse(buttonBMatches.Result("${Y}")));
			prizePosition = new Vector2Int(int.Parse(prizeMatches.Result("${X}")), int.Parse(prizeMatches.Result("${Y}")));
			//LogResult("Button A", buttonAMovement);
			//LogResult("Button B", buttonBMovement);
			//LogResult("Prize", prizePosition);
		}

		private bool CanButtonCombinationGetPrize(Vector2Int buttonAMovement, Vector2Int buttonBMovement, Vector2Int prizePosition, int buttonBPresses, out int tokenCost)
		{
			// Press Button B the predetermined number of times
			Vector2Int clawPosition = buttonBMovement * buttonBPresses;
			
			// Press Button A until claw reaches or overshoots prize
			int buttonAPresses = 0;
			while (buttonAPresses < MAX_PRESSES_PER_BUTTON && clawPosition.x < prizePosition.x && clawPosition.y < prizePosition.y)
			{
				buttonAPresses++;
				clawPosition += buttonAMovement;
			}

			tokenCost = BUTTON_A_COST * buttonAPresses + BUTTON_B_COST * buttonBPresses;
			return clawPosition == prizePosition;
		}
	}
}
