using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NaughtyAttributes;
using TMPro;
using UnityEngine;

public class Day18 : PuzzleBase
{
	[SerializeField] private TextMeshProUGUI _textMesh = null;
	[SerializeField] protected TextAsset _exampleDataSingleExplode = null;
	[SerializeField] protected TextAsset _exampleDataReduction = null;
	[SerializeField] protected List<TextAsset> _exampleDataSumLists = null;
	[SerializeField] protected TextAsset _exampleDataMagnitude = null;	// Pop pop!

	[Button("Clear Text")]
	private void ClearText()
	{
		_textMesh.SetText("");
	}
	
	protected override void ExecutePuzzle1()
	{
		ClearText();

		if (_isExample)
		{
			Debug.Log("--- Single Explode ---\n");
			ParseInputData(_exampleDataSingleExplode);
			foreach (string line in _inputDataLines)
			{
				Pair pair = ParsePair(line);
				Debug.Log("Parsed Pair: " + pair);

				if (pair.LookForPairToExplode())
				{
					Debug.Log("Exploded Pair to: " + pair);
				}
			}
			
			Debug.Log("------ Reduction ------\n");
			ParseInputData(_exampleDataReduction);
			foreach (string line in _inputDataLines)
			{
				Pair pair = ParsePair(line);
				Debug.Log("Parsed Pair: " + pair);

				pair.Reduce();
			}
			
			Debug.Log("------ Sum List ------\n");
			foreach (TextAsset sumList in _exampleDataSumLists)
			{
				Debug.Log("Parsing new Sum List...");
				ParseInputData(sumList);

				// Parse the starting Pair
				Pair sumPair = ParsePair(_inputDataLines[0]);
				
				// Iterate through other Pairs, adding and reducing each in turn
				for (int i = 1; i < _inputDataLines.Length; i++)
				{
					Pair pairToAdd = ParsePair(_inputDataLines[i]);
					Debug.Log("Adding Pairs " + sumPair + " and " + pairToAdd);
					
					// Sum the two Pairs
					Pair newSumPair = new Pair();
					newSumPair.PushItem(sumPair);
					newSumPair.PushItem(pairToAdd);

					// Reduce if necessary
					newSumPair.Reduce();

					// Store the new sum
					sumPair = newSumPair;
				}
			}
			
			Debug.Log("------ Magnitude ------\nPop pop!");
			ParseInputData(_exampleDataMagnitude);
			foreach (string line in _inputDataLines)
			{
				Pair pair = ParsePair(line);
				Debug.Log("Parsed Pair: " + pair);
				
				// Reduce if necessary
				pair.Reduce();
				
				int magnitude = pair.GetMagnitude();
				Debug.Log("Magnitude: " + magnitude);
			}
			
			Debug.Log("------ Full Example ------\n");
			ParseInputData(_exampleData);
			{
				// Parse the starting Pair
				Pair sumPair = ParsePair(_inputDataLines[0]);

				// Iterate through other Pairs, adding and reducing each in turn
				for (int i = 1; i < _inputDataLines.Length; i++)
				{
					Pair pairToAdd = ParsePair(_inputDataLines[i]);
					Debug.Log("Adding Pairs " + sumPair + " and " + pairToAdd);

					// Sum the two Pairs
					Pair newSumPair = new Pair();
					newSumPair.PushItem(sumPair);
					newSumPair.PushItem(pairToAdd);

					// Reduce if necessary
					newSumPair.Reduce();

					// Store the new sum
					sumPair = newSumPair;
				}
				
				// Calculate magnitude of final sum
				int magnitude = sumPair.GetMagnitude();
				Debug.Log("Magnitude: " + magnitude);
			}
		}
		else	// -- if (_isExample) else
		{
			// Parse the starting Pair
			Pair sumPair = ParsePair(_inputDataLines[0]);

			// Iterate through other Pairs, adding and reducing each in turn
			for (int i = 1; i < _inputDataLines.Length; i++)
			{
				Pair pairToAdd = ParsePair(_inputDataLines[i]);
				Debug.Log("Adding Pairs " + sumPair + " and " + pairToAdd);

				// Sum the two Pairs
				Pair newSumPair = new Pair();
				newSumPair.PushItem(sumPair);
				newSumPair.PushItem(pairToAdd);

				// Reduce if necessary
				newSumPair.Reduce();

				// Store the new sum
				sumPair = newSumPair;
			}
				
			// Calculate magnitude of final sum
			int magnitude = sumPair.GetMagnitude();
			Debug.Log("Magnitude: " + magnitude);
		}
	}

	private Pair ParsePair(string pairString)
	{
		Stack<Pair> nestedPairs = new Stack<Pair>();
		
		foreach (char c in pairString)
		{
			if (c == '[')
			{
				if (nestedPairs.Count == 0)
				{
					// Start the first pair
					nestedPairs.Push(new Pair());
				}
				else
				{
					// Start a new nested Pair
					Pair parent = nestedPairs.Peek();
					Pair newPair = new Pair();
					parent.PushItem(newPair);
					nestedPairs.Push(newPair);
				}
			}
			else if (c == ']')
			{
				if (nestedPairs.Count == 1)
				{
					// If this was the last Pair, we're done
					// Return the outermost Pair.
					return nestedPairs.Pop();
				}
				
				// Otherwise, there's more to parse
				// End the current Pair and continue
				nestedPairs.Pop();
			}
			else if (int.TryParse(c.ToString(), out int value))
			{
				// Add a Number to the current Pair
				Pair parent = nestedPairs.Peek();
				parent.PushItem(new Number(value));
			}
		}

		throw new InvalidDataException("Failed to parse Pair string: " + pairString);
	}

	private abstract class PairItemBase
	{
		public Pair Parent { get; private set; } = null;

		// Reduction flags - Used for debug logging
		protected internal bool isExploding = false;
		protected internal bool isSplitting = false;
		
		protected internal void SetParent(Pair parent)
		{
			Parent = parent;
		}
		
		protected int GetDepth()
		{
			int depth = 0;
			Pair parent = Parent;
			while (parent != null)
			{
				parent = parent.Parent;
				depth++;
			}

			return depth;
		}

		protected internal virtual void ClearReductionFlags()
		{
			isExploding = false;
			isSplitting = false;
		}

		public abstract Number FindRightmostNumber();
		public abstract Number FindLeftmostNumber();
		public abstract bool LookForPairToExplode();
		public abstract bool LookForNumberToSplit();
		public abstract int GetMagnitude();
	}

	private class Pair : PairItemBase
	{
		public PairItemBase Left { get; private set; }
		public PairItemBase Right { get; private set; }
		
		public void PushItem(PairItemBase item)
		{
			if (Left == null)
			{
				Left = item;
				item.SetParent(this);
			}
			else if (Right == null)
			{
				Right = item;
				item.SetParent(this);
			}
			else
			{
				Debug.LogError("Tried to push 3+ items onto Pair: " + this);
			}
		}

		public void Reduce()
		{
			while (true)
			{
				ClearReductionFlags();
				
				if (LookForPairToExplode())
				{
					Debug.Log("Pair Exploded: " + this);
				}
				else if (LookForNumberToSplit())
				{
					Debug.Log("Pair Split: " + this);
				}
				else
				{
					Debug.Log("Pair Reduction complete: <b><color=white>" + this + "</color></b>");
					break;
				}
			}
		}
		
		protected internal override void ClearReductionFlags()
		{
			base.ClearReductionFlags();

			Left?.ClearReductionFlags();
			Right?.ClearReductionFlags();
		}

		public override bool LookForPairToExplode()
		{
			if (Left is Number leftNumber && Right is Number rightNumber && (GetDepth() >= 4))
			{
				Parent.ExplodeChild(this, leftNumber, rightNumber);
				return true;
			}
			
			return Left.LookForPairToExplode() || Right.LookForPairToExplode();
		}

		private void ExplodeChild(Pair explodingPair, Number leftNumber, Number rightNumber)
		{
			if (explodingPair == Left)
			{
				ExplodeChild();
				Left = new Number(0);
				Left.SetParent(this);
				Left.isExploding = true;
			}
			else if (explodingPair == Right)
			{
				ExplodeChild();
				Right = new Number(0);
				Right.SetParent(this);
				Right.isExploding = true;
			}
			else
			{
				Debug.LogError("Tried to explode a Pair which cannot be exploded: " + explodingPair);
			}

			// --- Local method ---
			void ExplodeChild()
			{
				Number leftAdjacentNumber = FindLeftAdjacentNumber(explodingPair);
				Number rightAdjacentNumber = FindRightAdjacentNumber(explodingPair);

				if (leftAdjacentNumber != null)
				{
					leftAdjacentNumber.AddValue(leftNumber.Value);
					leftAdjacentNumber.isExploding = true;
				}

				if (rightAdjacentNumber != null)
				{
					rightAdjacentNumber.AddValue(rightNumber.Value);
					rightAdjacentNumber.isExploding = true;
				}
			}
		}

		/// Steps upward through nested Pairs, until we reach a Pair in which this item is the right child,
		/// or we reach the outermost layer.
		private Number FindLeftAdjacentNumber(PairItemBase fromItem)
		{
			if (fromItem == Right)
			{
				// We've come from the right, we can start looking for the rightmost number starting on the left side
				return Left.FindRightmostNumber();
			}
			
			if (fromItem == Left)
			{
				// We've come from the left, we need to continue traversing upwards until we come from the right
				if (Parent != null)
				{
					return Parent.FindLeftAdjacentNumber(this);
				}
				
				// We've reached the outermost layer and we're still on the left - return null
				return null;
			}
			
			// This shouldn't happen 
			throw new ArgumentException("Failed to explode. " + fromItem + " was not a child of " + this);
		}

		/// Steps upward through nested Pairs, until we reach a Pair in which this item is the left child,
		/// or we reach the outermost layer.
		private Number FindRightAdjacentNumber(PairItemBase fromItem)
		{
			if (fromItem == Left)
			{
				// We've come from the left, we can start looking for the leftmost number starting on the right side
				return Right.FindLeftmostNumber();
			}
			
			if (fromItem == Right)
			{
				// We've come from the right, we need to continue traversing upwards until we come from the left
				if (Parent != null)
				{
					return Parent.FindRightAdjacentNumber(this);
				}
				
				// We've reached the outermost layer and we're still on the right - return null
				return null;
			}
			
			// This shouldn't happen
			throw new ArgumentException("Failed to explode. " + fromItem + " was not a child of " + this);
		}

		/// Drills down through right children of nested Pairs until we find a Number.
		public override Number FindRightmostNumber()
		{
			return Right.FindRightmostNumber();
		}

		/// Drills down through left children of nested Pairs until we find a Number.
		public override Number FindLeftmostNumber()
		{
			return Left.FindLeftmostNumber();
		}

		public override bool LookForNumberToSplit()
		{
			if (Left.LookForNumberToSplit())
			{
				if (Left is Number leftNumber)
				{
					Left = leftNumber.Split();
				}

				return true;
			}
			else if (Right.LookForNumberToSplit())
			{
				if (Right is Number rightNumber)
				{
					Right = rightNumber.Split();
				}
				
				return true;
			}
			
			return false;
		}

		public override int GetMagnitude()
		{
			return (3 * Left.GetMagnitude()) + (2 * Right.GetMagnitude());
		}

		public override string ToString()
		{
			if (isExploding)
			{
				return "<b><color=yellow>" + $"[{Left},{Right}]" + "</color></b>";
			}

			if (isSplitting)
			{
				return "<b><color=cyan>" + $"[{Left},{Right}]" + "</color></b>";
			}
			
			return $"[{Left},{Right}]";
		}
	}

	private class Number : PairItemBase
	{
		public int Value { get; private set; }

		public Number(int value) : base()
		{
			Value = value;
		}

		public void AddValue(int valueToAdd)
		{
			Value += valueToAdd;
		}

		public Pair Split()
		{
			int leftValue = Mathf.FloorToInt(Value / 2f);
			int rightValue = Value - leftValue;
			
			Pair newPair = new Pair();
			newPair.PushItem(new Number(leftValue));
			newPair.PushItem(new Number(rightValue));
			newPair.SetParent(Parent);
			newPair.isSplitting = true;
			return newPair;
		}

		public override bool LookForPairToExplode()
		{
			return false;
		}

		public override bool LookForNumberToSplit()
		{
			return Value >= 10;
		}

		public override Number FindLeftmostNumber()
		{
			return this;
		}

		public override Number FindRightmostNumber()
		{
			return this;
		}

		public override int GetMagnitude()
		{
			return Value;
		}

		public override string ToString()
		{
			if (isExploding)
			{
				return "<b><color=yellow>" + Value + "</color></b>";
			}

			if (isSplitting)
			{
				return "<b><color=cyan>" + Value + "</color></b>";
			}
			
			return Value.ToString();
		}
	}

	protected override void ExecutePuzzle2()
	{
		int largestMagnitude = int.MinValue;
		for (int a = 0; a < _inputDataLines.Length; a++)
		{
			for (int b = 0; b < _inputDataLines.Length; b++)
			{
				if (a == b)
				{
					continue;
				}
				
				Pair pairA = ParsePair(_inputDataLines[a]);
				Pair pairB = ParsePair(_inputDataLines[b]);
				Debug.Log("Adding Pairs " + pairA + " and " + pairB);
				
				// Sum the two Pairs
				Pair newSumPair = new Pair();
				newSumPair.PushItem(pairA);
				newSumPair.PushItem(pairB);

				// Reduce if necessary
				newSumPair.Reduce();

				// Store the magnitude
				largestMagnitude = Mathf.Max(largestMagnitude, newSumPair.GetMagnitude());
			}
		}
				
		// Calculate magnitude of final sum
		Debug.Log("Largest Magnitude: " + largestMagnitude);
	}
}
