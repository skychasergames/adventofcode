using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AoC2021
{
	public class Day16 : PuzzleBase
	{
		[SerializeField] protected TextAsset _puzzle2ExampleData = null;

		protected override void ExecutePuzzle1()
		{
			foreach (string line in _inputDataLines)
			{
				LogResult("Transmission Hex", line);
			
				string transmissionBinary = ConvertHexToBinary(line);
				//LogResult("Transmission Binary", transmissionBinary);

				Packet packet = CreatePacket(transmissionBinary, out string remainingData);
				//LogPacket(packet, 0);
				//LogResult("Extraneous data", remainingData);

				int sumOfVersionNumbers = 0;
				sumOfVersionNumbers = GetSumOfVersionNumbers(packet, sumOfVersionNumbers);

				LogResult("Sum of Version Numbers", sumOfVersionNumbers);
			}
		}

		private void LogPacket(Packet packet, int depth)
		{
			if (packet is LiteralValuePacket literalValuePacket)
			{
				Log("Literal Value Packet >> " + depth);
				LogResult(" -> Version", packet.Version);
				LogResult(" -> Payload", literalValuePacket.GetValue());
			}
			else if (packet is OperatorPacket operatorPacket)
			{
				Log("Operator Packet >> " + depth);
				LogResult(" -> Version", packet.Version);
				LogResult(" -> Sub-Packets", operatorPacket.SubPackets.Count);

				foreach (Packet subPacket in operatorPacket.SubPackets)
				{
					LogPacket(subPacket, depth + 1);
				}
			}
		}

		private int GetSumOfVersionNumbers(Packet packet, int sum)
		{
			sum += packet.Version;
			if (packet is OperatorPacket operatorPacket)
			{
				foreach (Packet subPacket in operatorPacket.SubPackets)
				{
					sum = GetSumOfVersionNumbers(subPacket, sum);
				}
			}

			return sum;
		}

		private static Packet CreatePacket(string packetBinary, out string remainingData)
		{
			ReadPacketBinary(packetBinary, out int version, out int typeId, out string encodedData);
		
			switch (typeId)
			{
			case 0:
				return new SumOperatorPacket(version, encodedData, out remainingData);
			
			case 1:
				return new ProductOperatorPacket(version, encodedData, out remainingData);
			
			case 2:
				return new MinimumOperatorPacket(version, encodedData, out remainingData);
			
			case 3:
				return new MaximumOperatorPacket(version, encodedData, out remainingData);
			
			case 4:
				return new LiteralValuePacket(version, encodedData, out remainingData);
			
			case 5:
				return new GreaterThanOperatorPacket(version, encodedData, out remainingData);
			
			case 6:
				return new LessThanOperatorPacket(version, encodedData, out remainingData);
				
			case 7:
				return new EqualToOperatorPacket(version, encodedData, out remainingData);
			
			default:
				throw new InvalidDataException("TypeId " + typeId + " is not valid");
			}
		}

		private static void ReadPacketBinary(string packetBinary, out int version, out int typeId, out string encodedData)
		{
			const int versionLength = 3;
			const int typeIdLength = 3;

			version = (int)ConvertBinaryToDec(packetBinary.Substring(0, versionLength));
			typeId = (int)ConvertBinaryToDec(packetBinary.Substring(versionLength, typeIdLength));
			encodedData = packetBinary.Substring(versionLength + typeIdLength);
		}

		private static string ConvertHexToBinary(string hex)
		{
			StringBuilder binaryBuilder = new StringBuilder();
		
			foreach (char byteInHex in hex)
			{
				int byteInBase10 = Convert.ToInt32(byteInHex.ToString(), 16);
				string byteInBinary = Convert.ToString(byteInBase10, 2).PadLeft(4, '0');
				binaryBuilder.Append(byteInBinary);
			}
		
			return binaryBuilder.ToString();
		}

		private static long ConvertBinaryToDec(string binary)
		{
			return Convert.ToInt64(binary, 2);
		}

		private abstract class Packet
		{
			public int Version { get; }
			public abstract long GetValue();

			public Packet(int version)
			{
				Version = version;
			}
		}
	
		private class LiteralValuePacket : Packet
		{
			private long _decodedData;
		
			public LiteralValuePacket(int version, string encodedData, out string remainingData) : base(version)
			{
				_decodedData = DecodeData(encodedData, out remainingData);
			}

			public override long GetValue()
			{
				return _decodedData;
			}

			private long DecodeData(string encodedData, out string remainingData)
			{
				StringBuilder binaryBuilder = new StringBuilder();
			
				int dataGroup = 0;
				char continuationBit;
			
				do
				{
					const int byteLength = 4;
					int prefixBitIndex = dataGroup * (1 + byteLength);
					continuationBit = encodedData[prefixBitIndex];
					string groupBinary = encodedData.Substring(prefixBitIndex + 1, byteLength);
					binaryBuilder.Append(groupBinary);
					dataGroup++;
					remainingData = encodedData.Substring(prefixBitIndex + 1 + byteLength);
				}
				while (continuationBit == '1');

				return ConvertBinaryToDec(binaryBuilder.ToString());
			}
		}

		private abstract class OperatorPacket : Packet
		{
			public List<Packet> SubPackets { get; }

			protected OperatorPacket(int version, string encodedData, out string remainingData) : base(version)
			{
				SubPackets = DecodeData(encodedData, out remainingData);
			}

			private List<Packet> DecodeData(string encodedData, out string remainingData)
			{
				List<Packet> subPackets = new List<Packet>();
				char lengthTypeIdBit = encodedData[0];
				if (lengthTypeIdBit == '0')
				{
					// If the length type ID is 0, then the next 15 bits are a number that represents
					// 	the total length in bits of the sub-packets contained by this packet
				
					// Determine how long the data string is
					string subPacketDataLengthBinary = encodedData.Substring(1, 15);
					int subPacketDataLength = (int)ConvertBinaryToDec(subPacketDataLengthBinary);

					// Create the sub-packets
					string subPacketData = encodedData.Substring(1 + 15, subPacketDataLength);
					while (subPacketData.Length > 0)
					{
						Packet subPacket = CreatePacket(subPacketData, out subPacketData);
						subPackets.Add(subPacket);
					}

					// Set the remaining data
					remainingData = encodedData.Substring(1 + 15 + subPacketDataLength);
				}
				else
				{
					// If the length type ID is 1, then the next 11 bits are a number that represents
					//  the number of sub-packets immediately contained by this packet
				
					// Determine how many sub-packets there are
					string subPacketCountBinary = encodedData.Substring(1, 11);
					int subPacketCount = (int)ConvertBinaryToDec(subPacketCountBinary);

					// Create the sub-packets
					string subPacketData = encodedData.Substring(1 + 11);
					for (int i = 0; i < subPacketCount; i++)
					{
						Packet subPacket = CreatePacket(subPacketData, out subPacketData);
						subPackets.Add(subPacket);
					}

					// Use the remaining sub-packet data as the remaining data
					remainingData = subPacketData;
				}

				return subPackets;
			}
		}

		private class SumOperatorPacket : OperatorPacket
		{
			public SumOperatorPacket(int version, string encodedData, out string remainingData)
				: base(version, encodedData, out remainingData) { }

			public override long GetValue()
			{
				return SubPackets.Sum(subPacket => subPacket.GetValue());
			}
		}

		private class ProductOperatorPacket : OperatorPacket
		{
			public ProductOperatorPacket(int version, string encodedData, out string remainingData)
				: base(version, encodedData, out remainingData) { }

			public override long GetValue()
			{
				return SubPackets.Aggregate(1L, (current, subPacket) => current * subPacket.GetValue());
			}
		}

		private class MinimumOperatorPacket : OperatorPacket
		{
			public MinimumOperatorPacket(int version, string encodedData, out string remainingData)
				: base(version, encodedData, out remainingData) { }

			public override long GetValue()
			{
				return SubPackets.Min(subPacket => subPacket.GetValue());
			}
		}

		private class MaximumOperatorPacket : OperatorPacket
		{
			public MaximumOperatorPacket(int version, string encodedData, out string remainingData)
				: base(version, encodedData, out remainingData) { }

			public override long GetValue()
			{
				return SubPackets.Max(subPacket => subPacket.GetValue());
			}
		}

		private class GreaterThanOperatorPacket : OperatorPacket
		{
			public GreaterThanOperatorPacket(int version, string encodedData, out string remainingData)
				: base(version, encodedData, out remainingData) { }

			public override long GetValue()
			{
				if (SubPackets.Count != 2)
				{
					throw new InvalidDataException("GreaterThanOperatorPacket requires exactly 2 sub-packets (found " + SubPackets.Count + ")");
				}

				return SubPackets[0].GetValue() > SubPackets[1].GetValue() ? 1 : 0;
			}
		}

		private class LessThanOperatorPacket : OperatorPacket
		{
			public LessThanOperatorPacket(int version, string encodedData, out string remainingData)
				: base(version, encodedData, out remainingData) { }

			public override long GetValue()
			{
				if (SubPackets.Count != 2)
				{
					throw new InvalidDataException("LessThanOperatorPacket requires exactly 2 sub-packets (found " + SubPackets.Count + ")");
				}

				return SubPackets[0].GetValue() < SubPackets[1].GetValue() ? 1 : 0;
			}
		}

		private class EqualToOperatorPacket : OperatorPacket
		{
			public EqualToOperatorPacket(int version, string encodedData, out string remainingData)
				: base(version, encodedData, out remainingData) { }

			public override long GetValue()
			{
				if (SubPackets.Count != 2)
				{
					throw new InvalidDataException("EqualToOperatorPacket requires exactly 2 sub-packets (found " + SubPackets.Count + ")");
				}

				return SubPackets[0].GetValue() == SubPackets[1].GetValue() ? 1 : 0;
			}
		}

		protected override void OnTestPuzzle2Button()
		{
			_isExample = true;
			ParseInputData(_puzzle2ExampleData);
			ExecutePuzzle2();
		}
	
		protected override void ExecutePuzzle2()
		{
			foreach (string line in _inputDataLines)
			{
				LogResult("Transmission Hex", line);
			
				string transmissionBinary = ConvertHexToBinary(line);
				//LogResult("Transmission Binary", transmissionBinary);

				Packet packet = CreatePacket(transmissionBinary, out string remainingData);
				//LogPacket(packet, 0);
				//LogResult("Extraneous data", remainingData);

				long value = packet.GetValue();

				LogResult("Calculated Packet Value", value);
			}
		}
	}
}