using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NaughtyAttributes;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

public class Day16 : PuzzleBase
{
	protected override void ExecutePuzzle1()
	{
		foreach (string line in _inputDataLines)
		{
			LogResult("Transmission Hex", line);
			
			string transmissionBinary = ConvertHexToBinary(line);
			LogResult("Transmission Binary", transmissionBinary);

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
			LogResult(" -> Payload", literalValuePacket.DecodedData);
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
		
		if (typeId == 4)
		{
			return new LiteralValuePacket(version, typeId, encodedData, out remainingData);
		}

		return new OperatorPacket(version, typeId, encodedData, out remainingData);
	}

	private static void ReadPacketBinary(string packetBinary, out int version, out int typeId, out string encodedData)
	{
		const int versionLength = 3;
		const int typeIdLength = 3;

		version = ConvertBinaryToDec(packetBinary.Substring(0, versionLength));
		typeId = ConvertBinaryToDec(packetBinary.Substring(versionLength, typeIdLength));
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

	private static int ConvertBinaryToDec(string binary)
	{
		return (int)Convert.ToInt64(binary, 2);
	}

	private abstract class Packet
	{
		public int Version { get; }
		public int TypeId { get; }

		public Packet(int version, int typeId)
		{
			Version = version;
			TypeId = typeId;
		}
	}
	
	private class LiteralValuePacket : Packet
	{
		public int DecodedData { get; }

		public LiteralValuePacket(int version, int typeId, string encodedData, out string remainingData) : base(version, typeId)
		{
			DecodedData = DecodeData(encodedData, out remainingData);
		}

		private int DecodeData(string encodedData, out string remainingData)
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

	private class OperatorPacket : Packet
	{
		public List<Packet> SubPackets { get; }

		public OperatorPacket(int version, int typeId, string encodedData, out string remainingData) : base(version, typeId)
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
				int subPacketDataLength = ConvertBinaryToDec(subPacketDataLengthBinary);

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
				int subPacketCount = ConvertBinaryToDec(subPacketCountBinary);

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

	protected override void ExecutePuzzle2()
	{
		
	}
}