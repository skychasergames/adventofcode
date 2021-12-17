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

	private readonly Dictionary<char, int> _autocompleteScore = new Dictionary<char, int>
	{
		{ ')', 1 },
		{ ']', 2 },
		{ '}', 3 },
		{ '>', 4 },
	};
	
	protected override void ExecutePuzzle1()
	{
		ClearText();

		StringBuilder displayStringBuilder = new StringBuilder();
		
		int totalSyntaxErrorScore = 0;
		for (int i = 0; i < _inputDataLines.Length; i++)
		{
			string line = _inputDataLines[i];
			ValidateLine(line,
				
				// Valid line callback
				(isLineComplete, closingCharsNeededIfIncomplete) =>
				{
					Log("Line " + i + " is valid (" + (isLineComplete ? "complete" : "incomplete") + ")");
					
					displayStringBuilder.AppendLine(line);
				},
				
				// Corrupted line callback
				(illegalChar, expectedChar, illegalCharIndex) =>
				{
					Log("Line " + i + " encountered illegal character `" + illegalChar + "' (expected `" + expectedChar + "')");
					
					totalSyntaxErrorScore += _syntaxErrorScore[illegalChar];

					StringBuilder lineDisplayStringBuilder = new StringBuilder()
						.Append("<color=yellow>")
						.Append(line.Substring(0, illegalCharIndex))
						.Append("</color><color=red>")
						.Append(illegalChar)
						.Append("</color><color=grey>")
						.Append(line.Substring(illegalCharIndex + 1))
						.Append("</color>");
					displayStringBuilder.AppendLine(lineDisplayStringBuilder.ToString());
				}
			);
		}

		_textMesh.SetText(displayStringBuilder);
		
		LogResult("Total syntax error score", totalSyntaxErrorScore);
	}

	private void ValidateLine(string line, Action<bool, string> validLineCallback, Action<char, char, int> corruptedLineCallback)
	{
		Stack<char> openChunks = new Stack<char>();
		for (int i = 0; i < line.Length; i++)
		{
			char c = line[i];
			if (_chunkPairs.Any(pair => pair.Key == c))
			{
				// Opening char
				openChunks.Push(c);
			}
			else
			{
				// Closing char
				char lastOpeningChar = openChunks.Pop();
				char expectedClosingChar = _chunkPairs[lastOpeningChar];
				if (c == expectedClosingChar)
				{
					// Chunk is valid - continue checking
					continue;
				}
				else
				{
					// Chunk is invalid - line is invalid
					corruptedLineCallback(c, expectedClosingChar, i);
					return;
				}
			}
		}

		// Line was valid!
		bool isLineComplete = openChunks.Count == 0;
		
		// Finish it if incomplete
		StringBuilder closingCharsNeededIfIncomplete = new StringBuilder();
		while (openChunks.Count > 0)
		{
			char openingChar = openChunks.Pop();
			char closingChar = _chunkPairs[openingChar];
			closingCharsNeededIfIncomplete.Append(closingChar);
		}

		validLineCallback(isLineComplete, closingCharsNeededIfIncomplete.ToString());
	}

	protected override void ExecutePuzzle2()
	{
		ClearText();
		
		StringBuilder displayStringBuilder = new StringBuilder();
		
		List<ulong> lineScores = new List<ulong>();
		for (int i = 0; i < _inputDataLines.Length; i++)
		{
			string line = _inputDataLines[i];
			ValidateLine(line,
				
				// Valid line callback
				(isLineComplete, closingCharsNeededIfIncomplete) =>
				{
					Log("Line " + i + " is valid (" + (isLineComplete ? "complete" : "incomplete") + ")");

					StringBuilder lineDisplayStringBuilder = new StringBuilder(line);
					
					if (!isLineComplete)
					{
						lineDisplayStringBuilder.Append("<color=green>")
							.Append(closingCharsNeededIfIncomplete)
							.Append("</color>");
						
						line += closingCharsNeededIfIncomplete;

						ulong lineScore = 0;
						foreach (char c in closingCharsNeededIfIncomplete)
						{
							lineScore *= 5;
							lineScore += (ulong)_autocompleteScore[c];
						}
						
						lineScores.Add(lineScore);
					}
					
					displayStringBuilder.AppendLine(lineDisplayStringBuilder.ToString());
				},
				
				// Corrupted line callback
				(illegalChar, expectedChar, illegalCharIndex) =>
				{
					Log("Line " + i + " encountered illegal character `" + illegalChar + "' (expected `" + expectedChar + "')");
					
					StringBuilder lineDisplayStringBuilder = new StringBuilder()
						.Append("<color=grey>")
						.Append(line.Substring(0, illegalCharIndex))
						.Append("</color><color=red>")
						.Append(illegalChar)
						.Append("</color><color=grey>")
						.Append(line.Substring(illegalCharIndex + 1))
						.Append("</color>");
					displayStringBuilder.AppendLine(lineDisplayStringBuilder.ToString());
				}
			);
		}

		_textMesh.SetText(displayStringBuilder);

		lineScores.Sort();
		
		int middleIndex = Mathf.FloorToInt(lineScores.Count / 2f);
		ulong middleScore = lineScores[middleIndex];
		LogResult("Middle autocorrect score", middleScore);
	}
}
