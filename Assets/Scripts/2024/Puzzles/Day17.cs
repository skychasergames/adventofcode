using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using TMPro;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace AoC2024
{
	public class Day17 : PuzzleBase
	{
		[SerializeField] private bool _logEveryInstruction = true;
		[SerializeField] private TextMeshProUGUI _puzzle2Counter = null;
		
		private ulong _a = 0;
		private ulong _b = 0;
		private ulong _c = 0;
		private int[] _program;

		private EditorCoroutine _executePuzzleCoroutine = null;
		private bool _debugSkipProgram = false;

		private enum Opcode
		{
			ADV = 0,	// Division:	A = A / 2^combo
			BXL = 1,	// Bitwise XOR: B = B XOR literal
			BST = 2,	// Modulo:		B = combo % 8
			JNZ = 3,	// Jump:		Jump to literal if A != 0
			BXC = 4,	// Bitwise XOR: B = B XOR C
			OUT = 5,	// Output:		Output combo % 8
			BDV = 6,	// Division:	B = A / 2^combo
			CDV = 7 	// Division:	C = A / 2^combo
		}

		protected override void ExecutePuzzle1()
		{
			_puzzle2Counter.text = "N/A";
			
			for (int line = 0; line < _inputDataLines.Length; line += 4)
			{
				ParseProgram(line);
				
				LogResult("Executing Program", string.Join(",", _program));
				LogResult("Registers", "A=" + _a + ", B=" + _b + ", C=" + _c);
				
				ExecuteProgram();
			}
		}

		protected override void ExecutePuzzle2()
		{
			CancelPuzzle2Execution();
			_executePuzzleCoroutine = EditorCoroutineUtility.StartCoroutine(CheckForSelfReplicatingProgram(), this);
		}
		
		[Button("Cancel Puzzle 2 Execution")]
		private void CancelPuzzle2Execution()
		{
			if (_executePuzzleCoroutine != null)
			{
				EditorCoroutineUtility.StopCoroutine(_executePuzzleCoroutine);
				_executePuzzleCoroutine = null;
			}
			
			_debugSkipProgram = false;
			_puzzle2Counter.text = "-";
		}

		[Button("Debug Skip Program")]
		private void DebugSkipProgram()
		{
			_debugSkipProgram = true;
		}

		private void ParseProgram(int line)
		{
			_a = ulong.Parse(SplitString(_inputDataLines[line], ": ")[1]);
			_b = ulong.Parse(SplitString(_inputDataLines[line+1], ": ")[1]);
			_c = ulong.Parse(SplitString(_inputDataLines[line+2], ": ")[1]);
			_program = ParseIntArray(SplitString(_inputDataLines[line+3], ": ")[1], ",");
		}

		private void ExecuteProgram()
		{
			int i = 0;
			List<int> output = new List<int>();

			while (i < _program.Length)
			{
				ExecuteInstruction(ref i, ref output);
			}
			
			LogResult("Program Output", string.Join(",", output));
			LogResult("Registers", "A=" + _a + ", B=" + _b + ", C=" + _c);
		}

		private IEnumerator CheckForSelfReplicatingProgram()
		{
			// Special thanks to u/FantasyInSpace whose comment on some reddit post helped me greatly
			// when determining how best to approach this puzzle, after hitting several brick walls

			for (int line = 0; line < _inputDataLines.Length; line += 4)
			{
				ParseProgram(line);
				LogResult("Checking Program for self-replication", string.Join(",", _program));

				List<List<int>> selfReplicationANs = new List<List<int>>();
				List<List<int>> validANs = new List<List<int>>();
				validANs.Add(new List<int>());

				for (int p = _program.Length-1; p >= 0; p--)
				{
					List<List<int>> newValidANs = new List<List<int>>();
					for (int a = 0; a < 8; a++)
					{
						foreach (List<int> validAN in validANs)
						{
							List<int> newAN = new List<int>(validAN);
							newAN.Add(a);
							_a = GetAFromAN(newAN);
							LogResult("Testing Register A", _a + " (" + string.Join(",", newAN) + ")");
							_puzzle2Counter.text = _a.ToString();
							EditorApplication.QueuePlayerLoopUpdate();

							// Execute program
							int i = 0;
							List<int> output = new List<int>();
							while (i < _program.Length)
							{
								ExecuteInstruction(ref i, ref output);
							}
							
							// Check output against program
							bool outputMatchesInput = false;
							if (output.Count == _program.Length)
							{
								// Output length matches program length -- they must be fully identical
								outputMatchesInput = true;
								for (int o = 0; o < output.Count; o++)
								{
									if (output[o] != _program[o])
									{
										outputMatchesInput = false;
										break;
									}
								}

								if (outputMatchesInput)
								{
									// Program is self-replicating!
									Log("Output " + string.Join(",", output) + " is identical to program " + string.Join(",", _program.ToList().GetRange(_program.Length - output.Count, output.Count)));
									selfReplicationANs.Add(newAN);
								}
							}
							else if (output.Count > 0 && output.Count < _program.Length)
							{
								// Output is smaller than program length -- output must match last values of program
								outputMatchesInput = true;
								for (int offset = 0; offset < output.Count; offset++)
								{
									if (output[output.Count - 1 - offset] != _program[_program.Length - 1 - offset])
									{
										outputMatchesInput = false;
										break;
									}
								}

								if (outputMatchesInput)
								{
									// This aN is valid and should be iterated upon later
									Log("Output " + string.Join(",", output) + " partially matches program " + string.Join(",", _program.ToList().GetRange(_program.Length - output.Count, output.Count)));
									newValidANs.Add(newAN);
								}
							}
							
							if (!outputMatchesInput)
							{
								Log("Output " + string.Join(",", output) + " DOES NOT MATCH");
							}

							yield return null;
						}
					}

					validANs = newValidANs;
				}

				if (selfReplicationANs.Count > 0)
				{
					ulong lowestA = ulong.MaxValue;
					foreach (List<int> aN in selfReplicationANs)
					{
						ulong a = GetAFromAN(aN);
						LogResult("Program is self-replicating when using Register A", a + " (" + string.Join(",", aN) +")");

						if (a < lowestA)
						{
							lowestA = a;
						}
					}
					
					LogResult("Lowest Register A for self-replication", lowestA);
				}
				else
				{
					Log("Program is not self-replicating");
				}
			}
		}

		private ulong GetAFromAN(List<int> aN)
		{
			ulong a = 0;
			for (int n = 0; n < aN.Count; n++)
			{
				a += (ulong)aN[n] * (ulong)Mathf.Pow(8, aN.Count - 1 - n);
			}

			return a;
		}

		private Opcode ExecuteInstruction(ref int i, ref List<int> output)
		{
			Opcode opcode = (Opcode)_program[i];
			ulong operand = (ulong)_program[i+1];
			switch (opcode)
			{
			case Opcode.ADV:
				// A = A / 2^combo
				_a = (ulong)(_a / Math.Pow(2, GetComboOperand()));
				LogInstructionResult(i + ":adv", "Register A: " + _a);
				i += 2;
				break;

			case Opcode.BXL:
				// B = B XOR literal
				_b ^= operand;
				LogInstructionResult(i + ":bxl", "Register B: " + _b);
				i += 2;
				break;

			case Opcode.BST:
				// B = combo % 8
				_b = GetComboOperand() % 8;
				LogInstructionResult(i + ":bst", "Register B: " + _b);
				i += 2;
				break;

			case Opcode.JNZ:
				// Jump to literal if A != 0
				if (_a != 0)
				{
					LogInstructionResult(i + ":jnz", "Jump to " + operand);
					i = (int)operand;
				}
				else
				{
					LogInstructionResult(i + ":jnz", "Didn't jump, continued to " + (i + 2));
					i += 2;
				}
				break;

			case Opcode.BXC:
				// B = B XOR C
				_b ^= _c;
				LogInstructionResult(i + ":bxc", "Register B: " + _b);
				i += 2;
				break;

			case Opcode.OUT:
				// Output combo % 8
				output.Add((int)(GetComboOperand() % 8));
				LogInstructionResult(i + ":out", "Output: " + string.Join(",", output));
				i += 2;
				break;

			case Opcode.BDV:
				// B = A / 2^combo
				_b = (ulong)(_a / Math.Pow(2, GetComboOperand()));
				LogInstructionResult(i + ":bdv", "Register B: " + _b);
				i += 2;
				break;

			case Opcode.CDV:
				// C = A / 2^combo
				_c = (ulong)(_a / Math.Pow(2, GetComboOperand()));
				LogInstructionResult(i + ":cdv", "Register C: " + _c);
				i += 2;
				break;

			default:
				throw new ArgumentOutOfRangeException("Unhandled Opcode: " + _program[i]);
			}

			return opcode;
			
			// Local method
			ulong GetComboOperand()
			{
				switch (operand)
				{
				case 0:
				case 1:
				case 2:
				case 3:
					return operand;
				case 4:
					return _a;
				case 5:
					return _b;
				case 6:
					return _c;
				default:
					throw new NotImplementedException("Invalid Combo Operand: " + operand);
				}
			}

			// Local method
			void LogInstructionResult(string label, string result)
			{
				if (_logEveryInstruction)
				{
					Log("[" + label + "] " + result);
				}
			}
		}
	}
}
