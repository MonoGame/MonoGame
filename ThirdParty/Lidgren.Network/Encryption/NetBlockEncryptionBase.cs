using System;
using System.Collections.Generic;

namespace Lidgren.Network
{
	/// <summary>
	/// Base for a non-threadsafe encryption class
	/// </summary>
	public abstract class NetBlockEncryptionBase : INetEncryption
	{
		// temporary space for one block to avoid reallocating every time
		private byte[] m_tmp;

		/// <summary>
		/// Block size in bytes for this cipher
		/// </summary>
		public abstract int BlockSize { get; }

		/// <summary>
		/// NetBlockEncryptionBase constructor
		/// </summary>
		public NetBlockEncryptionBase()
		{
			m_tmp = new byte[BlockSize];
		}

		/// <summary>
		/// Encrypt am outgoing message with this algorithm; no writing can be done to the message after encryption, or message will be corrupted
		/// </summary>
		public bool Encrypt(NetOutgoingMessage msg)
		{
			int payloadBitLength = msg.LengthBits;
			int numBytes = msg.LengthBytes;
			int blockSize = BlockSize;
			int numBlocks = (int)Math.Ceiling((double)numBytes / (double)blockSize);
			int dstSize = numBlocks * blockSize;

			msg.EnsureBufferSize(dstSize * 8 + 2); // add 2 bytes for payload length at end
			msg.LengthBits = dstSize * 8; // length will automatically adjust +4 bytes when payload length is written

			for(int i=0;i<numBlocks;i++)
			{
				EncryptBlock(msg.m_data, (i * blockSize), m_tmp);
				Buffer.BlockCopy(m_tmp, 0, msg.m_data, (i * blockSize), m_tmp.Length);
			}

			// add true payload length last
			msg.Write((ushort)payloadBitLength);

			return true;
		}

		/// <summary>
		/// Decrypt an incoming message encrypted with corresponding Encrypt
		/// </summary>
		/// <param name="msg">message to decrypt</param>
		/// <returns>true if successful; false if failed</returns>
		public bool Decrypt(NetIncomingMessage msg)
		{
			int numEncryptedBytes = msg.LengthBytes - 2; // last 2 bytes is true bit length
			int blockSize = BlockSize;
			int numBlocks = numEncryptedBytes / blockSize;
			if (numBlocks * blockSize != numEncryptedBytes)
				return false;

			for (int i = 0; i < numBlocks; i++)
			{
				DecryptBlock(msg.m_data, (i * blockSize), m_tmp);
				Buffer.BlockCopy(m_tmp, 0, msg.m_data, (i * blockSize), m_tmp.Length);
			}

			// read 16 bits of true payload length
			uint realSize = NetBitWriter.ReadUInt32(msg.m_data, 16, (numEncryptedBytes * 8));
			msg.m_bitLength = (int)realSize;
			return true;
		}

		/// <summary>
		/// Encrypt a block of bytes
		/// </summary>
		protected abstract void EncryptBlock(byte[] source, int sourceOffset, byte[] destination);

		/// <summary>
		/// Decrypt a block of bytes
		/// </summary>
		protected abstract void DecryptBlock(byte[] source, int sourceOffset, byte[] destination);
	}
}
