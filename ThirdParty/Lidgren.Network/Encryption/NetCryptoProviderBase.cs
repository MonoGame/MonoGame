using System;
using System.IO;
using System.Security.Cryptography;

namespace Lidgren.Network
{
	public abstract class NetCryptoProviderBase : NetEncryption
	{
		protected SymmetricAlgorithm m_algorithm;

		public NetCryptoProviderBase(NetPeer peer, SymmetricAlgorithm algo)
			: base(peer)
		{
			m_algorithm = algo;
			m_algorithm.GenerateKey();
			m_algorithm.GenerateIV();
		}

		public override void SetKey(byte[] data, int offset, int count)
		{
			int len = m_algorithm.Key.Length;
			var key = new byte[len];
			for (int i = 0; i < len; i++)
				key[i] = data[offset + (i % count)];
			m_algorithm.Key = key;

			len = m_algorithm.IV.Length;
			key = new byte[len];
			for (int i = 0; i < len; i++)
				key[len - 1 - i] = data[offset + (i % count)];
			m_algorithm.IV = key;
		}

		public override bool Encrypt(NetOutgoingMessage msg)
		{
			int unEncLenBits = msg.LengthBits;

			var ms = new MemoryStream();
			var cs = new CryptoStream(ms, m_algorithm.CreateEncryptor(), CryptoStreamMode.Write);
			cs.Write(msg.m_data, 0, msg.LengthBytes);
			cs.Close();

			// get results
			var arr = ms.ToArray();
			ms.Close();

			msg.EnsureBufferSize((arr.Length + 4) * 8);
			msg.LengthBits = 0; // reset write pointer
			msg.Write((uint)unEncLenBits);
			msg.Write(arr);
			msg.LengthBits = (arr.Length + 4) * 8;

			return true;
		}

		public override bool Decrypt(NetIncomingMessage msg)
		{
			int unEncLenBits = (int)msg.ReadUInt32();

			var ms = new MemoryStream(msg.m_data, 4, msg.LengthBytes - 4);
			var cs = new CryptoStream(ms, m_algorithm.CreateDecryptor(), CryptoStreamMode.Read);

			var result = m_peer.GetStorage(unEncLenBits);
			cs.Read(result, 0, NetUtility.BytesToHoldBits(unEncLenBits));
			cs.Close();

			// TODO: recycle existing msg

			msg.m_data = result;
			msg.m_bitLength = unEncLenBits;
			msg.m_readPosition = 0;

			return true;
		}
	}
}
