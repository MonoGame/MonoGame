using System;
using System.IO;
using System.Security.Cryptography;

namespace Lidgren.Network
{
	public class NetTripleDESEncryption : NetCryptoProviderBase
	{
		public NetTripleDESEncryption(NetPeer peer)
			: base(peer, new TripleDESCryptoServiceProvider())
		{
		}

		public NetTripleDESEncryption(NetPeer peer, string key)
			: base(peer, new TripleDESCryptoServiceProvider())
		{
			SetKey(key);
		}

		public NetTripleDESEncryption(NetPeer peer, byte[] data, int offset, int count)
			: base(peer, new TripleDESCryptoServiceProvider())
		{
			SetKey(data, offset, count);
		}
	}
}
