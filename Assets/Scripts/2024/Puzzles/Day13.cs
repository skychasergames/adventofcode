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
		private const long UNIT_CONVERSION_ERROR = 10000000000000;

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
			long totalTokenCost = 0;
			
			for (int line = 0; line < _inputDataLines.Length; line += 3)
			{
				// I'm so bad at maths :notlikethis:
				
				// Solve as a system of equations
				ParseClawMachine(line, out Vector2Int buttonAMovement, out Vector2Int buttonBMovement, out Vector2Int prizePosition);
				long buttonAMovementX = buttonAMovement.x;
				long buttonAMovementY = buttonAMovement.y;
				long buttonBMovementX = buttonBMovement.x;
				long buttonBMovementY = buttonBMovement.y;
				long prizePositionX = prizePosition.x + UNIT_CONVERSION_ERROR;
				long prizePositionY = prizePosition.y + UNIT_CONVERSION_ERROR;

				// <Original equations>
				// buttonAMovementX + buttonBMovementX = prizePositionX
				// buttonAMovementY + buttonBMovementY = prizePositionY
				
				// <Eliminate Button B>
				//  buttonBMovementY * (buttonAMovementX + buttonBMovementX) =  buttonBMovementY * prizePositionX
				// -buttonBMovementX * (buttonAMovementY + buttonBMovementY) = -buttonBMovementX * prizePositionY
				
				//  buttonBMovementY * buttonAMovementX =  buttonBMovementY * prizePositionX
				// -buttonBMovementX * buttonAMovementY = -buttonBMovementX * prizePositionY
				
				// <Add the equations>
				//  buttonBMovementY * buttonAMovementX + -buttonBMovementX * buttonAMovementY = (buttonBMovementY * prizePositionX + -buttonBMovementX * prizePositionY)
				
				// <Solve for buttonAPresses>
				// buttonAPresses = (buttonBMovementY * prizePositionX + -buttonBMovementX * prizePositionY) / (buttonBMovementY * buttonAMovementX + -buttonBMovementX * buttonAMovementY)
				
				double buttonAPressesFractional = (double)(buttonBMovementY * prizePositionX + -buttonBMovementX * prizePositionY) / (buttonBMovementY * buttonAMovementX + -buttonBMovementX * buttonAMovementY);

				// <If buttonAPresses is a fraction, then there's no valid solution for this claw machine!>
				if (Math.Abs(buttonAPressesFractional - (long)buttonAPressesFractional) > 0.0001)
				{
					continue;
				}
				
				long buttonAPresses = (long)buttonAPressesFractional;
				
				// <Plug buttonAPresses back into one of the original equations to get buttonBPresses>
				// buttonAMovementX + buttonBMovementX = prizePositionX
				
				// buttonAMovementX * buttonAPresses + buttonBMovementX = prizePositionX
				
				// buttonBMovementX = prizePositionX - buttonAMovementX * buttonAPresses
				
				// buttonBPresses = (prizePositionX - buttonAMovementX * buttonAPresses) / buttonBMovementX
				
				long buttonBPresses = (prizePositionX - buttonAMovementX * buttonAPresses) / buttonBMovementX;

				long tokenCost = BUTTON_A_COST * buttonAPresses + BUTTON_B_COST * buttonBPresses;
				Log("Found prize for " + tokenCost + " tokens (" + buttonBPresses + " presses of Button B)");
				totalTokenCost += tokenCost;
			}
			
			LogResult("Total token cost", totalTokenCost);
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
