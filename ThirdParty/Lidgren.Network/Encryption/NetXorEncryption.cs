using System;
using System.Collections.Generic;
using System.Text;

namespace Lidgren.Network
{
	/// <summary>
	/// Example class; not very good encryption
	/// </summary>
	public class NetXorEncryption : INetEncryption
	{
		private byte[] m_key;

		/// <summary>
		/// NetXorEncryption constructor
		/// </summary>
		public NetXorEncryption(byte[] key)
		{
			m_key = key;
		}

		/// <summary>
		/// NetXorEncryption constructor
		/// </summary>
		public NetXorEncryption(string key)
		{
			m_key = Encoding.ASCII.GetBytes(key);
		}

		/// <summary>
		/// Encrypt an outgoing message
		/// </summary>
		public bool Encrypt(NetOutgoingMessage msg)
		{
			int numBytes = msg.LengthBytes;
			for (int i = 0; i < numBytes; i++)
			{
				int offset = i % m_key.Length;
				msg.m_data[i] = (byte)(msg.m_data[i] ^ m_key[offset]);
			}
			return true;
		}

		/// <summary>
		/// Decrypt an incoming message
		/// </summary>
		public bool Decrypt(NetIncomingMessage msg)
		{
			int numBytes = msg.LengthBytes;
			for (int i = 0; i < numBytes; i++)
			{
				int offset = i % m_key.Length;
				msg.m_data[i] = (byte)(msg.m_data[i] ^ m_key[offset]);
			}
			return true;
		}
	}
}
