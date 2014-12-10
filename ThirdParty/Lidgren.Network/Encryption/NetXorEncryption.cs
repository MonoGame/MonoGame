using System;
using System.Collections.Generic;
using System.Text;

namespace Lidgren.Network
{
	/// <summary>
	/// Example class; not very good encryption
	/// </summary>
	public class NetXorEncryption : NetEncryption
	{
		private byte[] m_key;

		/// <summary>
		/// NetXorEncryption constructor
		/// </summary>
		public NetXorEncryption(NetPeer peer, byte[] key)
			: base(peer)
		{
			m_key = key;
		}

		public override void SetKey(byte[] data, int offset, int count)
		{
			m_key = new byte[count];
			Array.Copy(data, offset, m_key, 0, count);
		}

		/// <summary>
		/// NetXorEncryption constructor
		/// </summary>
		public NetXorEncryption(NetPeer peer, string key)
			: base(peer)
		{
			m_key = Encoding.UTF8.GetBytes(key);
		}

		/// <summary>
		/// Encrypt an outgoing message
		/// </summary>
		public override bool Encrypt(NetOutgoingMessage msg)
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
		public override bool Decrypt(NetIncomingMessage msg)
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
