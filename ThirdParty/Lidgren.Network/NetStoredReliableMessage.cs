using System;

namespace Lidgren.Network
{
	internal struct NetStoredReliableMessage
	{
		public int NumSent;
		public double LastSent;
		public NetOutgoingMessage Message;
		public int SequenceNumber;

		public void Reset()
		{
			NumSent = 0;
			LastSent = 0.0;
			Message = null;
		}
	}
}
