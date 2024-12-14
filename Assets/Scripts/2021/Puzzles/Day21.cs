using System.Collections.Generic;
using UnityEngine;

namespace AoC2021
{
	public class Day21 : PuzzleBase
	{
		[SerializeField] private int _scoreToWinPuzzle1 = 1000;
		[SerializeField] private int _scoreToWinPuzzle2 = 21;
		[SerializeField] private int _boardLength = 10;
		[SerializeField] private int _rollsPerPlayerTurn = 3;
		[SerializeField] private int _puzzle1DieSides = 100;
		[SerializeField] private int _puzzle2DieSides = 3;
	
		protected override void ExecutePuzzle1()
		{
			IDie die = new DeterministicDie(_puzzle1DieSides);
		
			int player1StartPos = int.Parse(SplitString(_inputDataLines[0], ":")[1]);
			int player2StartPos = int.Parse(SplitString(_inputDataLines[1], ":")[1]);
			Player player1 = new Player(1, player1StartPos);
			Player player2 = new Player(2, player2StartPos);

			// Play the game!
			Player currentPlayer = player1;
			Player otherPlayer = player2;
			int totalRolls = 0;
			while (true)
			{
				int distanceToMove = 0;
				for (int i = 0; i < _rollsPerPlayerTurn; i++)
				{
					distanceToMove += die.Roll();
					totalRolls++;
				}

				int newPosition = currentPlayer.Position + distanceToMove;
			
				// Wrap around board
				while (newPosition > _boardLength)
				{
					newPosition -= _boardLength;
				}

				currentPlayer.StopAtPosition(newPosition);
				Log("Player " + currentPlayer.PlayerNumber + " moved to " + newPosition + ". Current score: " + currentPlayer.Score);

				if (currentPlayer.Score >= _scoreToWinPuzzle1)
				{
					Log("Game ended after " + totalRolls + " rolls!");
					Log("Player " + currentPlayer.PlayerNumber + " won with score " + currentPlayer.Score);
					Log("Player " + otherPlayer.PlayerNumber + " lost with score " + otherPlayer.Score);

					int result = otherPlayer.Score * totalRolls;
					LogResult("Result", result);
					break;
				}

				otherPlayer = currentPlayer;
				currentPlayer = (currentPlayer == player1 ? player2 : player1);
			}
		}

		private class Player
		{
			public int PlayerNumber { get; }
			public int Position { get; private set; }
			public int Score { get; private set; }

			public Player(int playerNumber, int startPosition)
			{
				PlayerNumber = playerNumber;
				Position = startPosition;
			}

			public void StopAtPosition(int newPosition)
			{
				Position = newPosition;
				Score += newPosition;
			}
		}

		private interface IDie
		{
			int Roll();
		}

		private class DeterministicDie : IDie
		{
			private int _sides;
			private int _lastRoll;

			public DeterministicDie(int sides)
			{
				_sides = sides;
				_lastRoll = 0;
			}
		
			public int Roll()
			{
				_lastRoll++;
				if (_lastRoll > _sides)
				{
					_lastRoll = 1;
				}

				return _lastRoll;
			}
		}

		protected override void ExecutePuzzle2()
		{
			int player1StartPos = int.Parse(SplitString(_inputDataLines[0], ":")[1]);
			int player2StartPos = int.Parse(SplitString(_inputDataLines[1], ":")[1]);

			gameStates.Clear();
			(ulong, ulong) result = CalculateGameState(player1StartPos-1, player2StartPos-1, 0, 0);

			LogResult("Player 1 wins", result.Item1);
			LogResult("Player 2 wins", result.Item2);
		}

		// Day 21 beat me. Honestly, I don't get any of this :sadge:
		// All credit goes to Jonathan Paulson (https://www.youtube.com/watch?v=a6ZdJEntKkk)
		private Dictionary<(int, int, int, int), (ulong, ulong)> gameStates = new Dictionary<(int, int, int, int), (ulong, ulong)>();
		private (ulong, ulong) CalculateGameState(int player1Pos, int player2Pos, int player1Score, int player2Score)
		{
			if (player1Score >= _scoreToWinPuzzle2)
			{
				return (1, 0);
			}
			if (player2Score >= _scoreToWinPuzzle2)
			{
				return (0, 1);
			}

			if (gameStates.TryGetValue((player1Pos, player2Pos, player1Score, player2Score), out (ulong, ulong) gameStateCount))
			{
				return gameStateCount;
			}

			(ulong, ulong) newGameState = (0, 0); 
			for (int roll1 = 1; roll1 <= _puzzle2DieSides; roll1++)
			{
				for (int roll2 = 1; roll2 <= _puzzle2DieSides; roll2++)
				{
					for (int roll3 = 1; roll3 <= _puzzle2DieSides; roll3++)
					{
						int newPlayer1Pos = (player1Pos + roll1 + roll2 + roll3) % 10;
						int newPlayer2Pos = player1Score + newPlayer1Pos + 1;

						(ulong, ulong) recursiveGameState = CalculateGameState(player2Pos, newPlayer1Pos, player2Score, newPlayer2Pos);
						newGameState = (newGameState.Item1 + recursiveGameState.Item2, newGameState.Item2 + recursiveGameState.Item1);
					}
				}
			}

			gameStates.Add((player1Pos, player2Pos, player1Score, player2Score), newGameState);
			return newGameState;
		}
	}
}
