using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

public class Day21 : PuzzleBase
{
	[SerializeField] private int _scoreToWin = 1000;
	[SerializeField] private float _playerInterval = 2f;
	[SerializeField] private int _boardLength = 10;
	[SerializeField] private int _deterministicDieSides = 100;
	[SerializeField] private int _rollsPerPlayerTurn = 3;
	
	private EditorCoroutine _executePuzzleCoroutine = null;

	[Button]
	private void ResetBoard()
	{
		if (_executePuzzleCoroutine != null)
		{
			EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
		}
	}
	
	protected override void ExecutePuzzle1()
	{
		// Initialize
		ResetBoard();

		IDie die = new DeterministicDie(_deterministicDieSides);
		_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(die), this);
	}

	private IEnumerator ExecutePuzzle(IDie die)
	{
		EditorWaitForSeconds playerInterval = new EditorWaitForSeconds(_playerInterval);
		
		EditorApplication.QueuePlayerLoopUpdate();
		yield return playerInterval;

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

			if (currentPlayer.Score >= _scoreToWin)
			{
				Log("Game ended after " + totalRolls + " rolls!");
				Log("Player " + currentPlayer.PlayerNumber + " won with score " + currentPlayer.Score);
				Log("Player " + otherPlayer.PlayerNumber + " lost with score " + otherPlayer.Score);

				int result = otherPlayer.Score * totalRolls;
				LogResult("Result", result);
				break;
			}

			yield return playerInterval;
			
			otherPlayer = currentPlayer;
			currentPlayer = (currentPlayer == player1 ? player2 : player1);
		}
		
		_executePuzzleCoroutine = null;
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
		
	}
}
