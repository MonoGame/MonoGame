using System;
using System.IO;
using System.Security.Cryptography;

namespace Lidgren.Network
{
	public abstract class NetCryptoProviderEncryption : NetEncryption
	{
		public NetCryptoProviderEncryption(NetPeer peer)
			: base(peer)
		{
		}

		protected abstract CryptoStream GetEncryptStream(MemoryStream ms);

		protected abstract CryptoStream GetDecryptStream(MemoryStream ms);

		public override bool Encrypt(NetOutgoingMessage msg)
		{
			int unEncLenBits = msg.LengthBits;

			var ms = new MemoryStream();
			var cs = GetEncryptStream(ms);
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
			var cs = GetDecryptStream(ms);

			var result = m_peer.GetStorage(unEncLenBits);
			cs.Read(result, 0, NetUtility.BytesToHoldBits(unEncLenBits));
			cs.Close();

			// TODO: recycle existing msg

			msg.m_data = result;
			msg.m_bitLength = unEncLenBits;

			return true;
		}
	}
}
