using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class Day10 : PuzzleBase
{
	[SerializeField] private TextMeshProUGUI _textMesh = null;
	
	[Button("Clear Text")]
	private void ClearText()
	{
		_textMesh.SetText("");
	}
	
	private readonly Dictionary<char, char> _chunkPairs = new Dictionary<char, char>
	{
		{ '(', ')' },
		{ '[', ']' },
		{ '{', '}' },
		{ '<', '>' },
	};

	private readonly Dictionary<char, int> _syntaxErrorScore = new Dictionary<char, int>
	{
		{ ')', 3 },
		{ ']', 57 },
		{ '}', 1197 },
		{ '>', 25137 },
	};
	
	protected override void ExecutePuzzle1()
	{
		StringBuilder displayStringBuilder = new StringBuilder();
		int totalSyntaxErrorScore = 0;
		for (int i = 0; i < _inputDataLines.Length; i++)
		{
			string line = _inputDataLines[i];
			ValidateLine(line, out string lineDisplayString,
				
				// Valid line callback
				(isLineComplete) =>
				{
					Log("Line " + i + " is valid (" + (isLineComplete ? "complete" : "incomplete") + ")");
				},
				
				// Corrupted line callback
				(illegalChar, expectedChar) =>
				{
					Log("Line " + i + " encountered illegal character `" + illegalChar + "' (expected `" + expectedChar + "')");
					totalSyntaxErrorScore += _syntaxErrorScore[illegalChar];
				}
			);

			displayStringBuilder.AppendLine(lineDisplayString);
		}

		_textMesh.SetText(displayStringBuilder);
		
		LogResult("Total syntax error score", totalSyntaxErrorScore);
	}

	private void ValidateLine(string line, out string displayString, Action<bool> validLineCallback, Action<char, char> corruptedLineCallback)
	{
		StringBuilder displayStringBuilder = new StringBuilder();

		Stack<char> openChunks = new Stack<char>();
		foreach (char c in line)
		{
			if (_chunkPairs.Any(pair => pair.Key == c))
			{
				// Opening char
				openChunks.Push(c);
				displayStringBuilder.Append(c);
			}
			else
			{
				// Closing char
				char lastOpeningChar = openChunks.Pop();
				char expectedClosingChar = _chunkPairs[lastOpeningChar];
				if (c == expectedClosingChar)
				{
					// Chunk is valid - continue checking
					displayStringBuilder.Append(c);
					continue;
				}
				else
				{
					// Chunk is invalid - line is invalid
					displayStringBuilder.Insert(0, "<color=yellow>");
					displayStringBuilder.Append("</color>");
					displayStringBuilder.Append($"<color=red>{c}</color>");
					displayString = displayStringBuilder.ToString();
					corruptedLineCallback(c, expectedClosingChar);
					return;
				}
			}
		}
		
		// Line was valid!
		validLineCallback(openChunks.Count == 0);
		displayString = displayStringBuilder.ToString();
	}

	protected override void ExecutePuzzle2()
	{
		
	}
}
