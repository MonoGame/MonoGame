using System;

namespace Lidgren.Network
{
	internal abstract class NetSenderChannelBase
	{
		// access this directly to queue things in this channel
		internal NetQueue<NetOutgoingMessage> m_queuedSends;

		internal abstract int WindowSize { get; }

		internal abstract int GetAllowedSends();

		internal abstract NetSendResult Enqueue(NetOutgoingMessage message);
		internal abstract void SendQueuedMessages(float now);
		internal abstract void Reset();
		internal abstract void ReceiveAcknowledge(float now, int sequenceNumber);
	}
}
