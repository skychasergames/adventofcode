using System;
using System.Collections.Generic;
using UnityEngine;

namespace AoC2024
{
	public class Day17 : PuzzleBase
	{
		[SerializeField] private bool _logEveryInstruction = true;
		
		private long _a = 0;
		private long _b = 0;
		private long _c = 0;
		private int[] _program;

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
			for (int line = 0; line < _inputDataLines.Length; line += 4)
			{
				ParseProgram(line);
				ExecuteProgram();
			}
		}

		protected override void ExecutePuzzle2()
		{
			
		}

		private void ParseProgram(int line)
		{
			_a = int.Parse(SplitString(_inputDataLines[line], ": ")[1]);
			_b = int.Parse(SplitString(_inputDataLines[line+1], ": ")[1]);
			_c = int.Parse(SplitString(_inputDataLines[line+2], ": ")[1]);
			_program = ParseIntArray(SplitString(_inputDataLines[line+3], ": ")[1], ",");
		}

		private void ExecuteProgram()
		{
			LogResult("Executing Program", string.Join(",", _program));
			LogResult("Registers", "A=" + _a + ", B=" + _b + ", C=" + _c);
			
			int i = 0;
			List<int> output = new List<int>();

			while (i < _program.Length)
			{
				long operand = _program[i+1];
				
				switch ((Opcode)_program[i])
				{
				case Opcode.ADV:
					// A = A / 2^combo
					_a = (long)(_a / Math.Pow(2, GetComboOperand()));
					LogInstructionResult("adv", "Register A: " + _a);
					i += 2;
					break;

				case Opcode.BXL:
					// B = B XOR literal
					_b ^= operand;
					LogInstructionResult("bxl", "Register B: " + _b);
					i += 2;
					break;

				case Opcode.BST:
					// B = combo % 8
					_b = GetComboOperand() % 8;
					LogInstructionResult("bst", "Register B: " + _b);
					i += 2;
					break;

				case Opcode.JNZ:
					// Jump to literal if A != 0
					if (_a != 0)
					{
						LogInstructionResult("jnz", "Jump to " + operand);
						i = (int)operand;
					}
					else
					{
						LogInstructionResult("jnz", "Didn't jump, continued to " + (i + 2));
						i += 2;
					}
					break;

				case Opcode.BXC:
					// B = B XOR C
					_b ^= _c;
					LogInstructionResult("bxc", "Register B: " + _b);
					i += 2;
					break;

				case Opcode.OUT:
					// Output combo % 8
					output.Add((int)(GetComboOperand() % 8));
					LogInstructionResult("out", "Output: " + string.Join(",", output));
					i += 2;
					break;

				case Opcode.BDV:
					// B = A / 2^combo
					_b = (long)(_a / Math.Pow(2, GetComboOperand()));
					LogInstructionResult("bdv", "Register B: " + _b);
					i += 2;
					break;

				case Opcode.CDV:
					// C = A / 2^combo
					_c = (long)(_a / Math.Pow(2, GetComboOperand()));
					LogInstructionResult("cdv", "Register C: " + _c);
					i += 2;
					break;

				default:
					throw new ArgumentOutOfRangeException("Unhandled Opcode: " + _program[i]);
				}
				
				// Local method
				long GetComboOperand()
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
				void LogInstructionResult(string opcode, string result)
				{
					if (_logEveryInstruction)
					{
						Log("[" + _program[i] + ":" + opcode + "] " + result);
					}
				}
			}
			
			LogResult("Program Output", string.Join(",", output));
			LogResult("Registers", "A=" + _a + ", B=" + _b + ", C=" + _c);
		}
	}
}
