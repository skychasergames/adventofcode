using System;
using UnityEngine;
using NaughtyAttributes;

public abstract class PuzzleBase : MonoBehaviour
{
	[SerializeField] protected TextAsset _exampleData = null;
	[SerializeField] protected TextAsset _puzzleData = null;

	protected string[] _inputDataLines = null;
	protected bool _isExample = false;
	
	[Button("Test Puzzle 1")]
	protected void OnTestPuzzle1Button()
	{
		_isExample = true;
		ParseInputData(_exampleData);
		ExecutePuzzle1();
	}
	
	[Button("Execute Puzzle 1")]
	protected void OnExecutePuzzle1Button()
	{
		_isExample = false;
		ParseInputData(_puzzleData);
		ExecutePuzzle1();
	}
	
	[Button("Test Puzzle 2")]
	protected void OnTestPuzzle2Button()
	{
		_isExample = true;
		ParseInputData(_exampleData);
		ExecutePuzzle2();
	}
	
	[Button("Execute Puzzle 2")]
	protected void OnExecutePuzzle2Button()
	{
		_isExample = false;
		ParseInputData(_puzzleData);
		ExecutePuzzle2();
	}
	
	protected abstract void ExecutePuzzle1();
	protected abstract void ExecutePuzzle2();
	
	protected virtual void ParseInputData(TextAsset inputData)
	{
		if (inputData != null)
		{
			_inputDataLines = SplitString(inputData.text, null);
		}
		else
		{
			Debug.LogError("[" + name + "] Input data was null");
		}
	}

	public static string[] SplitString(string input, string delimiter)
	{
		string[] delimiters = { !string.IsNullOrEmpty(delimiter) ? delimiter : Environment.NewLine };
		return input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
	}

	protected int[] ParseIntArray(string[] input)
	{
		int[] result = new int[input.Length];
		
		for (int i = 0; i < input.Length; i++)
		{
			result[i] = int.Parse(input[i]);
		}

		return result;
	}

	protected void Log(string label)
	{
		Debug.Log("[" + name + "] " + label);
	}
	
	protected void LogResult(string label, object result)
	{
		Debug.Log("[" + name + "] " + label + ": " + result);
	}

	protected void LogError(string label)
	{
		Debug.LogError("[" + name + "] " + label);
	}
	
	protected void LogError(string label, object result)
	{
		Debug.LogError("[" + name + "] " + label + ": " + result);
	}
}
