using System;
using System.Collections.Generic;
using System.Linq;

namespace AoC2024
{
	public class Day9 : PuzzleBase
	{
		protected override void ExecutePuzzle1()
		{
			foreach (string diskMapString in _inputDataLines)
			{
				ushort?[] blocks = GetBlocks(diskMapString);
				LogBlocks("Expanded disk map", blocks);

				ReorderBlocks(ref blocks);
				LogBlocks("Reordered disk map", blocks);

				CalculateChecksum(blocks);
			}
		}

		protected override void ExecutePuzzle2()
		{
			foreach (string diskMapString in _inputDataLines)
			{
				ushort?[] blocks = GetBlocks(diskMapString);
				LogBlocks("Expanded disk map", blocks);

				ReorderBlocksWithoutFragmentation(ref blocks);
				LogBlocks("Reordered disk map", blocks);

				CalculateChecksum(blocks);
			}
		}

		private ushort?[] GetBlocks(string diskMapString)
		{
			LogResult("Decompressing disk map", diskMapString);
			
			// Parse disk map string and initialize blocks array
			int[] diskMap = ParseIntArray(diskMapString);
			int blockCount = diskMap.Sum();
			LogResult("Total blocks", blockCount);
			ushort?[] blocks = new ushort?[blockCount];

			// Decompress files and free spaces into blocks array
			int writeBlock = 0;
			for (int i = 0; i < diskMap.Length; i++)
			{
				if (i % 2 == 0)
				{
					// File
					ushort fileID = (ushort)(i / 2);
					for (int j = 0; j < diskMap[i]; j++)
					{
						blocks[writeBlock] = fileID;
						writeBlock++;
					}
				}
				else
				{
					// Free space
					for (int j = 0; j < diskMap[i]; j++)
					{
						blocks[writeBlock] = null;
						writeBlock++;
					}
				}
			}

			return blocks;
		}

		private void ReorderBlocks(ref ushort?[] blocks)
		{
			// Backfill file blocks into free spaces
			int fromBlock = blocks.Length-1;
			int toBlock = 0;
				
			// Locate next available block to move
			while (fromBlock >= 0)
			{
				if (blocks[fromBlock] != null)
				{
					// Locate next available space for block
					while (toBlock < fromBlock)
					{
						if (blocks[toBlock] == null)
						{
							//Log("Move from " + fromBlock + " to " + toBlock);
								
							// Move block
							blocks[toBlock] = blocks[fromBlock];
							blocks[fromBlock] = null;
							break;
						}
						toBlock++;
					}
				}
				fromBlock--;
			}
		}

		private void ReorderBlocksWithoutFragmentation(ref ushort?[] blocks)
		{
			// Backfill whole files into free spaces
			int fromBlock = blocks.Length-1;
			
			// Locate next available block to move
			while (fromBlock >= 0)
			{
				if (blocks[fromBlock] != null)
				{
					// Determine block size of file
					ushort? fileID = blocks[fromBlock];
					int fileEndBlock = fromBlock;
					while (fromBlock > 0 && blocks[fromBlock-1] == fileID)
					{
						fromBlock--;
					}

					int fileSize = (fileEndBlock - fromBlock) + 1;
					
					// Find block of space large enough for file
					int toBlock = 0;
					while (toBlock < fromBlock)
					{
						if (blocks[toBlock] == null)
						{
							// Determine size of empty space
							int spaceStartBlock = toBlock;
							while (toBlock < fromBlock && blocks[toBlock+1] == null)
							{
								toBlock++;
							}

							int spaceSize = (toBlock - spaceStartBlock) + 1;
							if (spaceSize >= fileSize)
							{
								//Log("Move from [" + fromBlock + "-" + fileEndBlock + "] to [" + spaceStartBlock + "-" + (spaceStartBlock + fileSize) + "]");
								
								// Move file
								for (int i = 0; i < fileSize; i++)
								{
									blocks[spaceStartBlock + i] = fileID;
									blocks[fromBlock + i] = null;
								}

								//LogBlocks("After move", blocks);
								break;
							}
						}
						toBlock++;
					}

				}
				fromBlock--;
			}
		}

		private void CalculateChecksum(ushort?[] blocks)
		{
			// Calculate filesystem checksum
			ulong filesystemChecksum = 0;
			for (int i = 0; i < blocks.Length; i++)
			{
				if (blocks[i] != null)
				{
					filesystemChecksum += (ulong)(i * blocks[i]);
				}
			}

			LogResult("Filesystem checksum", filesystemChecksum);
		}

		private void LogBlocks(string label, ushort?[] blocks)
		{
			// Debug log expanded disk map
			string expandedDiskMap = "";
			foreach (ushort? block in blocks)
			{
				if (block != null)
				{
					expandedDiskMap += block;
				}
				else
				{
					expandedDiskMap += ".";
				}
			}
			LogResult(label, expandedDiskMap);
		}
	}
}
