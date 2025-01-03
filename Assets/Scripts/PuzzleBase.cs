using System;
using UnityEngine;
using NaughtyAttributes;
using UnityEditor;

public abstract class PuzzleBase : MonoBehaviour
{
	[SerializeField] protected TextAsset _exampleData = null;
	[SerializeField] protected TextAsset _puzzleData = null;

	protected string[] _inputDataLines = null;
	protected bool _isExample = false;
	
	[Button("Test Puzzle 1")]
	protected virtual void OnTestPuzzle1Button()
	{
		_isExample = true;
		ParseInputData(_exampleData);
		ExecutePuzzle1();
	}
	
	[Button("Execute Puzzle 1")]
	protected virtual void OnExecutePuzzle1Button()
	{
		_isExample = false;
		ParseInputData(_puzzleData);
		ExecutePuzzle1();
	}
	
	[Button("Test Puzzle 2")]
	protected virtual void OnTestPuzzle2Button()
	{
		_isExample = true;
		ParseInputData(_exampleData);
		ExecutePuzzle2();
	}
	
	[Button("Execute Puzzle 2")]
	protected virtual void OnExecutePuzzle2Button()
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

	protected int[] ParseIntArray(string input)
	{
		int[] result = new int[input.Length];
		
		for (int i = 0; i < input.Length; i++)
		{
			result[i] = int.Parse(input[i].ToString());
		}

		return result;
	}

	protected int[] ParseIntArray(string input, string delimiter)
	{
		string[] strings = SplitString(input, delimiter);
		int[] result = new int[strings.Length];
		for (int i = 0; i < result.Length; i++)
		{
			result[i] = int.Parse(strings[i]);
		}

		return result;
	}

	protected Vector2Int ParseVector2Int(string input, string delimiter)
	{
		string[] strings = SplitString(input, delimiter);
		return new Vector2Int(int.Parse(strings[0]), int.Parse(strings[1]));
	}

	protected void Log(string label, bool beep = false)
	{
		Debug.Log("[" + name + "] " + label);

		if (beep)
		{
			EditorApplication.Beep();
		}
	}
	
	protected void LogResult(string label, object result, bool beep = false)
	{
		Debug.Log("[" + name + "] " + label + ": " + result);

		if (beep)
		{
			EditorApplication.Beep();
		}
	}

	protected void LogError(string label, bool beep = false)
	{
		Debug.LogError("[" + name + "] " + label);

		if (beep)
		{
			EditorApplication.Beep();
		}
	}
	
	protected void LogError(string label, object result, bool beep = false)
	{
		Debug.LogError("[" + name + "] " + label + ": " + result);

		if (beep)
		{
			EditorApplication.Beep();
		}
	}
}
