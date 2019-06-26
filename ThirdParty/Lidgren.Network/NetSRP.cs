#define USE_SHA256

using System;
using System.Security.Cryptography;
using System.Text;

namespace Lidgren.Network
{
	/// <summary>
	/// Helper methods for implementing SRP authentication
	/// </summary>
	public static class NetSRP
	{
		private static readonly NetBigInteger N = new NetBigInteger("0115b8b692e0e045692cf280b436735c77a5a9e8a9e7ed56c965f87db5b2a2ece3", 16);
		private static readonly NetBigInteger g = NetBigInteger.Two;
		private static readonly NetBigInteger k = ComputeMultiplier();

		private static HashAlgorithm GetHashAlgorithm()
		{
#if USE_SHA256
			// this does not seem to work as of yet
			return SHA256.Create();
#else
			return SHA1.Create();
#endif
		}

		/// <summary>
		/// Compute multiplier (k)
		/// </summary>
		private static NetBigInteger ComputeMultiplier()
		{
			string one = NetUtility.ToHexString(N.ToByteArrayUnsigned());
			string two = NetUtility.ToHexString(g.ToByteArrayUnsigned());

			string ccstr = one + two.PadLeft(one.Length, '0');
			byte[] cc = NetUtility.ToByteArray(ccstr);

			var sha = GetHashAlgorithm();
			var ccHashed = sha.ComputeHash(cc);

			return new NetBigInteger(NetUtility.ToHexString(ccHashed), 16);
		}

		/// <summary>
		/// Create 16 bytes of random salt
		/// </summary>
		public static byte[] CreateRandomSalt()
		{
			byte[] retval = new byte[16];
			CryptoRandom.Instance.NextBytes(retval);
			return retval;
		}

		/// <summary>
		/// Create 32 bytes of random ephemeral value
		/// </summary>
		public static byte[] CreateRandomEphemeral()
		{
			byte[] retval = new byte[32];
			CryptoRandom.Instance.NextBytes(retval);
			return retval;
		}

		/// <summary>
		/// Computer private key (x)
		/// </summary>
		public static byte[] ComputePrivateKey(string username, string password, byte[] salt)
		{
			var sha = GetHashAlgorithm();

			byte[] tmp = Encoding.UTF8.GetBytes(username + ":" + password);
			byte[] innerHash = sha.ComputeHash(tmp);

			byte[] total = new byte[innerHash.Length + salt.Length];
			Buffer.BlockCopy(salt, 0, total, 0, salt.Length);
			Buffer.BlockCopy(innerHash, 0, total, salt.Length, innerHash.Length);

			// x   ie. H(salt || H(username || ":" || password))
			return new NetBigInteger(NetUtility.ToHexString(sha.ComputeHash(total)), 16).ToByteArrayUnsigned();
		}

		/// <summary>
		/// Creates a verifier that the server can later use to authenticate users later on (v)
		/// </summary>
		public static byte[] ComputeServerVerifier(byte[] privateKey)
		{
			NetBigInteger x = new NetBigInteger(NetUtility.ToHexString(privateKey), 16);

			// Verifier (v) = g^x (mod N)
			var serverVerifier = g.ModPow(x, N);

			return serverVerifier.ToByteArrayUnsigned();
		}

		/// <summary>
		/// SHA hash data
		/// </summary>
		public static byte[] Hash(byte[] data)
		{
			var sha = GetHashAlgorithm();
			return sha.ComputeHash(data);
		}

		/// <summary>
		/// Compute client public ephemeral value (A)
		/// </summary>
		public static byte[] ComputeClientEphemeral(byte[] clientPrivateEphemeral) // a
		{
			// A= g^a (mod N) 
			NetBigInteger a = new NetBigInteger(NetUtility.ToHexString(clientPrivateEphemeral), 16);
			NetBigInteger retval = g.ModPow(a, N);

			return retval.ToByteArrayUnsigned();
		}

		/// <summary>
		/// Compute server ephemeral value (B)
		/// </summary>
		public static byte[] ComputeServerEphemeral(byte[] serverPrivateEphemeral, byte[] verifier) // b
		{
			var b = new NetBigInteger(NetUtility.ToHexString(serverPrivateEphemeral), 16);
			var v = new NetBigInteger(NetUtility.ToHexString(verifier), 16);

			// B = kv + g^b (mod N) 
			var bb = g.ModPow(b, N);
			var kv = v.Multiply(k);
			var B = (kv.Add(bb)).Mod(N);

			return B.ToByteArrayUnsigned();
		}

		/// <summary>
		/// Compute intermediate value (u)
		/// </summary>
		public static byte[] ComputeU(byte[] clientPublicEphemeral, byte[] serverPublicEphemeral)
		{
			// u = SHA-1(A || B)
			string one = NetUtility.ToHexString(clientPublicEphemeral);
			string two = NetUtility.ToHexString(serverPublicEphemeral);

			int len = 66; //  Math.Max(one.Length, two.Length);
			string ccstr = one.PadLeft(len, '0') + two.PadLeft(len, '0');

			byte[] cc = NetUtility.ToByteArray(ccstr);

			var sha = GetHashAlgorithm();
			var ccHashed = sha.ComputeHash(cc);

			return new NetBigInteger(NetUtility.ToHexString(ccHashed), 16).ToByteArrayUnsigned();
		}

		/// <summary>
		/// Computes the server session value
		/// </summary>
		public static byte[] ComputeServerSessionValue(byte[] clientPublicEphemeral, byte[] verifier, byte[] udata, byte[] serverPrivateEphemeral)
		{
			// S = (Av^u) ^ b (mod N)
			var A = new NetBigInteger(NetUtility.ToHexString(clientPublicEphemeral), 16);
			var v = new NetBigInteger(NetUtility.ToHexString(verifier), 16);
			var u = new NetBigInteger(NetUtility.ToHexString(udata), 16);
			var b = new NetBigInteger(NetUtility.ToHexString(serverPrivateEphemeral), 16);

			NetBigInteger retval = v.ModPow(u, N).Multiply(A).Mod(N).ModPow(b, N).Mod(N);

			return retval.ToByteArrayUnsigned();
		}

		/// <summary>
		/// Computes the client session value
		/// </summary>
		public static byte[] ComputeClientSessionValue(byte[] serverPublicEphemeral, byte[] xdata,  byte[] udata, byte[] clientPrivateEphemeral)
		{
			// (B - kg^x) ^ (a + ux)   (mod N)
			var B = new NetBigInteger(NetUtility.ToHexString(serverPublicEphemeral), 16);
			var x = new NetBigInteger(NetUtility.ToHexString(xdata), 16);
			var u = new NetBigInteger(NetUtility.ToHexString(udata), 16);
			var a = new NetBigInteger(NetUtility.ToHexString(clientPrivateEphemeral), 16);

			var bx = g.ModPow(x, N);
			var btmp = B.Add(N.Multiply(k)).Subtract(bx.Multiply(k)).Mod(N);
			return btmp.ModPow(x.Multiply(u).Add(a), N).ToByteArrayUnsigned();
		}

		/// <summary>
		/// Create XTEA symmetrical encryption object from sessionValue
		/// </summary>
		public static NetXtea CreateEncryption(NetPeer peer, byte[] sessionValue)
		{
			var sha = GetHashAlgorithm();
			var hash = sha.ComputeHash(sessionValue);
			
			var key = new byte[16];
			for(int i=0;i<16;i++)
			{
				key[i] = hash[i];
				for (int j = 1; j < hash.Length / 16; j++)
					key[i] ^= hash[i + (j * 16)];
			}

			return new NetXtea(peer, key);
		}
	}
}
