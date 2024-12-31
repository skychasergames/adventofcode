using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Cache = System.Collections.Concurrent.ConcurrentDictionary<string, long>;

namespace AoC2024
{
	public class Day19 : PuzzleBase
	{
		private static readonly Color BlockBackgroundDefault = Color.grey;
		private static readonly Color BlockBackgroundPossible = Color.yellow;
		private static readonly Color BlockBackgroundImpossible = Color.grey;
		
		private static readonly Color BlockStripeWhite = Color.white;
		private static readonly Color BlockStripeBlue = new Color(0.00f, 0.50f, 1.00f);
		private static readonly Color BlockStripeBlack = Color.black;
		private static readonly Color BlockStripeRed = new Color(1.00f, 0.10f, 0.00f);
		private static readonly Color BlockStripeGreen = new Color(0.00f, 0.75f, 0.15f);
		
		[SerializeField] private ColorBlock _colorBlockPrefab = null;
		[SerializeField] private RectTransform _designEntryPrefab = null;
		[SerializeField] private Image _arrowPrefab = null;
		[SerializeField] private RectTransform _towelsParent = null;
		[SerializeField] private RectTransform _designsParent = null;
		[Tooltip("Turns out there's literally hundreds of combinations for some designs... Could be here a while...")]
		[SerializeField] private int _maxTowelCombinationsPerDesign = 1;
		[Tooltip("In case the scene gets too laggy when displaying hundreds of designs...")]
		[SerializeField] private int _maxDesignsToDisplay = 50;

		private string[] _availableTowelPatterns = null;
		private List<List<string>> _possibleTowelCombinations = null;
		private EditorCoroutine _executePuzzleCoroutine = null;
		
		protected override void ExecutePuzzle1()
		{
			ResetScene();
			
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(ExecutePuzzle(), this);
		}

		protected override void ExecutePuzzle2()
		{
			// Initialize towels
			_availableTowelPatterns = SplitString(_inputDataLines[0], ", ");
			
			Cache cache = new Cache();
			List<long> towelCombinationCount = _inputDataLines
				.Skip(1)
				.Select(CountTowelCombinationsForRemainingDesign)
				.ToList();

			LogResult("Total possible towel configurations", towelCombinationCount.Sum());

			// Local method, recursive
			long CountTowelCombinationsForRemainingDesign(string remainingDesign)
			{
				//LogResult("Remaining pattern", remainingDesign);
				return cache.GetOrAdd(
					remainingDesign,
					design => design == "" ? 1 : _availableTowelPatterns.Where(design.StartsWith).Sum(towel => CountTowelCombinationsForRemainingDesign(design.Substring(towel.Length)))
				);
			}
		}

		[Button("Reset Scene")]
		private void ResetScene()
		{
			if (_executePuzzleCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
				_executePuzzleCoroutine = null;
			}

			while (_towelsParent.childCount > 0)
			{
				DestroyImmediate(_towelsParent.GetChild(0).gameObject);
			}

			while (_designsParent.childCount > 0)
			{
				DestroyImmediate(_designsParent.GetChild(0).gameObject);
			}

			EditorApplication.QueuePlayerLoopUpdate();
		}

		private IEnumerator ExecutePuzzle()
		{
			// Initialize towels
			_availableTowelPatterns = SplitString(_inputDataLines[0], ", ");
			
			foreach (string towelPattern in _availableTowelPatterns)
			{
				List<Color> towelColors = GetColorsForStripePattern(towelPattern);
				ColorBlock towelBlock = Instantiate(_colorBlockPrefab, _towelsParent);
				towelBlock.Initialize(BlockBackgroundDefault, towelColors);
			}

			// Build designs
			int totalPossibleDesigns = 0;
			for (int line = 1; line < _inputDataLines.Length; line++)
			{
				string designPattern = _inputDataLines[line];
				LogResult("Checking design", designPattern);

				RectTransform designEntry = null;
				ColorBlock designBlock = null;
				if (line - 1 < _maxDesignsToDisplay)
				{
					// Instantiate design container & color block
					List<Color> designColors = GetColorsForStripePattern(designPattern);
					designEntry = Instantiate(_designEntryPrefab, _designsParent);
					designBlock = Instantiate(_colorBlockPrefab, designEntry);
					designBlock.Initialize(BlockBackgroundDefault, designColors);

					Canvas.ForceUpdateCanvases();
					EditorApplication.QueuePlayerLoopUpdate();
				}

				// Determine if design is possible
				yield return FindPossibleTowelCombinationsForDesign(designPattern);
				if (_possibleTowelCombinations.Count > 0)
				{
					Log("Design " + designPattern + " is possible through " + _possibleTowelCombinations.Count + " combination(s) of towels");
					totalPossibleDesigns++;
					
					if (designEntry != null)
					{
						designBlock.SetBackgroundColor(BlockBackgroundPossible);

						// Display combinations in canvas (if limit hasn't been reached)
						foreach (List<string> towelCombination in _possibleTowelCombinations)
						{
							// Instantiate arrow
							Instantiate(_arrowPrefab, designEntry);

							// Instantiate color blocks
							foreach (string towelUsed in towelCombination)
							{
								List<Color> towelColors = GetColorsForStripePattern(towelUsed);
								ColorBlock towel = Instantiate(_colorBlockPrefab, designEntry);
								towel.Initialize(BlockBackgroundDefault, towelColors);
							}
						}
					}
				}
				else
				{
					LogResult("Design is impossible", designPattern);
					if (designBlock != null)
					{
						designBlock.SetBackgroundColor(BlockBackgroundImpossible);
					}
				}
			}

			Canvas.ForceUpdateCanvases();
			EditorApplication.QueuePlayerLoopUpdate();

			LogResult("Total possible designs", totalPossibleDesigns);
		}

		private List<Color> GetColorsForStripePattern(string stripePattern)
		{
			return stripePattern.Select(c => c switch
			{
				'w' => BlockStripeWhite,
				'u' => BlockStripeBlue,
				'b' => BlockStripeBlack,
				'r' => BlockStripeRed,
				'g' => BlockStripeGreen,
				_ => throw new ArgumentOutOfRangeException("Unhandled char: " + c)
			}).ToList();
		}

		private IEnumerator FindPossibleTowelCombinationsForDesign(string designPattern)
		{
			_possibleTowelCombinations = new List<List<string>>();

			// Narrow list of towel patterns down to those that could fit into the design
			List<string> matchingTowelPatterns = _availableTowelPatterns.Where(designPattern.Contains).ToList();
			//LogResult("Matching towel patterns", string.Join(",", matchingTowelPatterns));
			
			// Exit early if any required color does not appear in the narrowed-down list of towel patterns
			List<char> allPossibleColors = matchingTowelPatterns.SelectMany(s => s).Distinct().ToList();
			foreach (char stripe in designPattern.Where(stripe => !allPossibleColors.Contains(stripe)))
			{
				LogResult("Color did not appear in any towels", stripe);
				yield break;
			}

			// Attempt to combine matching towel patterns into completed design
			Stack<List<string>> buildingDesigns = new Stack<List<string>>();
			foreach (string startingPattern in matchingTowelPatterns.Where(designPattern.StartsWith))
			{
				buildingDesigns.Push(new List<string> { startingPattern });
				//LogResult("Start building design", startingPattern);
			}
			
			while (buildingDesigns.Count > 0)
			{
				List<string> buildingDesign = buildingDesigns.Pop();
				//LogResult("Iterating on building design", string.Join(",", buildingDesign) + " (flattened: " + string.Join("", buildingDesign) + ")");
				if (string.Join("", buildingDesign) == designPattern)
				{
					// Design is complete!
					_possibleTowelCombinations.Add(buildingDesign);
					LogResult("Found combination for " + designPattern, string.Join(",", buildingDesign));

					if (_possibleTowelCombinations.Count >= _maxTowelCombinationsPerDesign)
					{
						Log("Too many possible combinations for this design! Moving on to next design.");
						yield break;
					}
				}
				else
				{
					int i = buildingDesign.Sum(pattern => pattern.Length);
					foreach (string pattern in matchingTowelPatterns.Where(p => (i + p.Length <= designPattern.Length) && (designPattern.Substring(i, p.Length) == p)))
					{
						List<string> newDesign = new List<string>(buildingDesign) { pattern }; 
						buildingDesigns.Push(newDesign);
						//LogResult("New design", string.Join(",", newDesign));
					}
				}

				yield return null;
			}
		}
	}
}
