using System;
using System.Collections.Generic;

namespace Lidgren.Network
{
	/// <summary>
	/// Interface for an encryption algorithm
	/// </summary>
	public interface INetEncryption
	{
		/// <summary>
		/// Encrypt an outgoing message in place
		/// </summary>
		bool Encrypt(NetOutgoingMessage msg);

		/// <summary>
		/// Decrypt an incoming message in place
		/// </summary>
		bool Decrypt(NetIncomingMessage msg);
	}
}
