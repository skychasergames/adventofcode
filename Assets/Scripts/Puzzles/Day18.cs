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
					Pair newPair = new Pair(null, 0);
					nestedPairs.Push(newPair);
				}
				else
				{
					// Start a new nested Pair
					Pair parent = nestedPairs.Peek();
					Pair newPair = new Pair(parent, nestedPairs.Count);
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
				Number newNumber = new Number(value, parent, nestedPairs.Count);
				parent.PushItem(newNumber);
			}
		}

		throw new InvalidDataException("Failed to parse Pair string: " + pairString);
	}

	private abstract class PairItemBase
	{
		public Pair Parent { get; }
		public int Depth { get; }

		protected PairItemBase(Pair parent, int depth)
		{
			Parent = parent;
			Depth = depth;
		}

		public abstract bool LookForPairToExplode();
		public abstract Number FindRightmostNumber();
		public abstract Number FindLeftmostNumber();
	}

	private class Pair : PairItemBase
	{
		public PairItemBase Left { get; private set; }
		public PairItemBase Right { get; private set; }

		public Pair(Pair parent, int depth) : base(parent, depth)
		{
			
		}

		public void PushItem(PairItemBase item)
		{
			if (Left == null)
			{
				Left = item;
			}
			else if (Right == null)
			{
				Right = item;
			}
			else
			{
				Debug.LogError("Tried to push 3+ items onto Pair: " + this);
			}
		}

		public override bool LookForPairToExplode()
		{
			if (Left is Number leftNumber && Right is Number rightNumber && (Depth >= 4))
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
				Left = new Number(0, this, Depth + 1);
			}
			else if (explodingPair == Right)
			{
				ExplodeChild();
				Right = new Number(0, this, Depth + 1);
			}
			else
			{
				Debug.LogError("Tried to explode a Pair which cannot be exploded: " + explodingPair);
			}

			// --- Local method ---
			void ExplodeChild()
			{
				Debug.Log("Exploding Pair: " + explodingPair);

				Number leftAdjacentNumber = FindLeftAdjacentNumber(explodingPair);
				Number rightAdjacentNumber = FindRightAdjacentNumber(explodingPair);
				leftAdjacentNumber?.AddValue(leftNumber.Value);
				rightAdjacentNumber?.AddValue(rightNumber.Value);
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

		public void SplitLeft()
		{
			if (Left is Number leftNumber)
			{
				Left = leftNumber.Split();
			}
		}

		public void SplitRight()
		{
			if (Right is Number rightNumber)
			{
				Right = rightNumber.Split();
			}
		}

		public override string ToString()
		{
			return $"[{Left},{Right}]";
		}
	}

	private class Number : PairItemBase
	{
		public int Value { get; private set; }

		public Number(int value, Pair parent, int depth) : base(parent, depth)
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
			
			Pair newPair = new Pair(Parent, Depth);
			newPair.PushItem(new Number(leftValue, newPair, Depth + 1));
			newPair.PushItem(new Number(rightValue, newPair, Depth + 1));
			return newPair;
		}

		public override bool LookForPairToExplode()
		{
			return false;
		}

		public override Number FindLeftmostNumber()
		{
			return this;
		}

		public override Number FindRightmostNumber()
		{
			return this;
		}

		public override string ToString()
		{
			return Value.ToString();
		}
	}

	protected override void ExecutePuzzle2()
	{
		
	}
}
