using System;
using System.IO;
using System.Security.Cryptography;

namespace Lidgren.Network
{
	public class NetAESEncryption : NetCryptoProviderBase
	{
		public NetAESEncryption(NetPeer peer)
			: base(peer, new AesCryptoServiceProvider())
		{
		}

		public NetAESEncryption(NetPeer peer, string key)
			: base(peer, new AesCryptoServiceProvider())
		{
			SetKey(key);
		}

		public NetAESEncryption(NetPeer peer, byte[] data, int offset, int count)
			: base(peer, new AesCryptoServiceProvider())
		{
			SetKey(data, offset, count);
		}
	}
}
