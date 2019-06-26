using System;

namespace Lidgren.Network
{
	internal static class NetFragmentationHelper
	{
		internal static int WriteHeader(
			byte[] destination,
			int ptr,
			int group,
			int totalBits,
			int chunkByteSize,
			int chunkNumber)
		{
			uint num1 = (uint)group;
			while (num1 >= 0x80)
			{
				destination[ptr++] = (byte)(num1 | 0x80);
				num1 = num1 >> 7;
			}
			destination[ptr++] = (byte)num1;

			// write variable length fragment total bits
			uint num2 = (uint)totalBits;
			while (num2 >= 0x80)
			{
				destination[ptr++] = (byte)(num2 | 0x80);
				num2 = num2 >> 7;
			}
			destination[ptr++] = (byte)num2;

			// write variable length fragment chunk size
			uint num3 = (uint)chunkByteSize;
			while (num3 >= 0x80)
			{
				destination[ptr++] = (byte)(num3 | 0x80);
				num3 = num3 >> 7;
			}
			destination[ptr++] = (byte)num3;

			// write variable length fragment chunk number
			uint num4 = (uint)chunkNumber;
			while (num4 >= 0x80)
			{
				destination[ptr++] = (byte)(num4 | 0x80);
				num4 = num4 >> 7;
			}
			destination[ptr++] = (byte)num4;

			return ptr;
		}

		internal static int ReadHeader(byte[] buffer, int ptr, out int group, out int totalBits, out int chunkByteSize, out int chunkNumber)
		{
			int num1 = 0;
			int num2 = 0;
			while (true)
			{
				byte num3 = buffer[ptr++];
				num1 |= (num3 & 0x7f) << (num2 & 0x1f);
				num2 += 7;
				if ((num3 & 0x80) == 0)
				{
					group = num1;
					break;
				}
			}

			num1 = 0;
			num2 = 0;
			while (true)
			{
				byte num3 = buffer[ptr++];
				num1 |= (num3 & 0x7f) << (num2 & 0x1f);
				num2 += 7;
				if ((num3 & 0x80) == 0)
				{
					totalBits = num1;
					break;
				}
			}

			num1 = 0;
			num2 = 0;
			while (true)
			{
				byte num3 = buffer[ptr++];
				num1 |= (num3 & 0x7f) << (num2 & 0x1f);
				num2 += 7;
				if ((num3 & 0x80) == 0)
				{
					chunkByteSize = num1;
					break;
				}
			}

			num1 = 0;
			num2 = 0;
			while (true)
			{
				byte num3 = buffer[ptr++];
				num1 |= (num3 & 0x7f) << (num2 & 0x1f);
				num2 += 7;
				if ((num3 & 0x80) == 0)
				{
					chunkNumber = num1;
					break;
				}
			}

			return ptr;
		}

		internal static int GetFragmentationHeaderSize(int groupId, int totalBytes, int chunkByteSize, int numChunks)
		{
			int len = 4;

			// write variable length fragment group id
			uint num1 = (uint)groupId;
			while (num1 >= 0x80)
			{
				len++;
				num1 = num1 >> 7;
			}

			// write variable length fragment total bits
			uint num2 = (uint)(totalBytes * 8);
			while (num2 >= 0x80)
			{
				len++;
				num2 = num2 >> 7;
			}

			// write variable length fragment chunk byte size
			uint num3 = (uint)chunkByteSize;
			while (num3 >= 0x80)
			{
				len++;
				num3 = num3 >> 7;
			}

			// write variable length fragment chunk number
			uint num4 = (uint)numChunks;
			while (num4 >= 0x80)
			{
				len++;
				num4 = num4 >> 7;
			}

			return len;
		}

		internal static int GetBestChunkSize(int group, int totalBytes, int mtu)
		{
			int tryChunkSize = mtu - NetConstants.HeaderByteSize - 4; // naive approximation
			int est = GetFragmentationHeaderSize(group, totalBytes, tryChunkSize, totalBytes / tryChunkSize);
			tryChunkSize = mtu - NetConstants.HeaderByteSize - est; // slightly less naive approximation

			int headerSize = 0;
			do
			{
				tryChunkSize--; // keep reducing chunk size until it fits within MTU including header

				int numChunks = totalBytes / tryChunkSize;
				if (numChunks * tryChunkSize < totalBytes)
					numChunks++;

				headerSize = GetFragmentationHeaderSize(group, totalBytes, tryChunkSize, numChunks); // 4+ bytes

			} while (tryChunkSize + headerSize + NetConstants.HeaderByteSize + 1 >= mtu);

			return tryChunkSize;
		}
	}
}
