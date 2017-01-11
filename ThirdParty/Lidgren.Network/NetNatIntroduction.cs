using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

#if !__NOIPENDPOINT__
using NetEndPoint = System.Net.IPEndPoint;
#endif

namespace Lidgren.Network
{
	public partial class NetPeer {
		private const byte HostByte = 1;
		private const byte ClientByte = 0;

		/// <summary>
		/// Send NetIntroduction to hostExternal and clientExternal; introducing client to host
		/// </summary>
		public void Introduce(
			NetEndPoint hostInternal,
			NetEndPoint hostExternal,
			NetEndPoint clientInternal,
			NetEndPoint clientExternal,
			string token)
		{
			// send message to client
			NetOutgoingMessage um = CreateMessage(10 + token.Length + 1);
			um.m_messageType = NetMessageType.NatIntroduction;
			um.Write((byte)0);
			um.Write(hostInternal);
			um.Write(hostExternal);
			um.Write(token);
			Interlocked.Increment(ref um.m_recyclingCount);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(clientExternal, um));

			// send message to host
			um = CreateMessage(10 + token.Length + 1);
			um.m_messageType = NetMessageType.NatIntroduction;
			um.Write((byte)1);
			um.Write(clientInternal);
			um.Write(clientExternal);
			um.Write(token);
			Interlocked.Increment(ref um.m_recyclingCount);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(hostExternal, um));
		}

		/// <summary>
		/// Called when host/client receives a NatIntroduction message from a master server
		/// </summary>
		internal void HandleNatIntroduction(int ptr)
		{
			VerifyNetworkThread();

			// read intro
			NetIncomingMessage tmp = SetupReadHelperMessage(ptr, 1000); // never mind length

			byte hostByte = tmp.ReadByte();
			NetEndPoint remoteInternal = tmp.ReadIPEndPoint();
			NetEndPoint remoteExternal = tmp.ReadIPEndPoint();
			string token = tmp.ReadString();
			bool isHost = (hostByte != 0);

			LogDebug("NAT introduction received; we are designated " + (isHost ? "host" : "client"));

			NetOutgoingMessage punch;

			if (!isHost && m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.NatIntroductionSuccess) == false)
				return; // no need to punch - we're not listening for nat intros!

			// send internal punch
			punch = CreateMessage(1);
			punch.m_messageType = NetMessageType.NatPunchMessage;
			punch.Write(hostByte);
			punch.Write(token);
			Interlocked.Increment(ref punch.m_recyclingCount);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(remoteInternal, punch));
			LogDebug("NAT punch sent to " + remoteInternal);

			// send external punch
			punch = CreateMessage(1);
			punch.m_messageType = NetMessageType.NatPunchMessage;
			punch.Write(hostByte);
			punch.Write(token);
			Interlocked.Increment(ref punch.m_recyclingCount);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(remoteExternal, punch));
			LogDebug("NAT punch sent to " + remoteExternal);

		}

		/// <summary>
		/// Called when receiving a NatPunchMessage from a remote endpoint
		/// </summary>
		private void HandleNatPunch(int ptr, NetEndPoint senderEndPoint)
		{
			NetIncomingMessage tmp = SetupReadHelperMessage(ptr, 1000); // never mind length

			var isFromClient = tmp.ReadByte() == ClientByte;
			string token = tmp.ReadString();
			if (isFromClient)
			{
				LogDebug("NAT punch received from " + senderEndPoint + " we're host, so we send a NatIntroductionConfirmed message - token is " + token);

				var confirmResponse = CreateMessage(1);
				confirmResponse.m_messageType = NetMessageType.NatIntroductionConfirmed;
				confirmResponse.Write(HostByte);
				confirmResponse.Write(token);
				Interlocked.Increment(ref confirmResponse.m_recyclingCount);
				m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(senderEndPoint, confirmResponse));
			}
			else
			{
				LogDebug("NAT punch received from " + senderEndPoint + " we're client, so we send a NatIntroductionConfirmRequest - token is " + token);

				var confirmRequest = CreateMessage(1);
				confirmRequest.m_messageType = NetMessageType.NatIntroductionConfirmRequest;
				confirmRequest.Write(ClientByte);
				confirmRequest.Write(token);
				Interlocked.Increment(ref confirmRequest.m_recyclingCount);
				m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(senderEndPoint, confirmRequest));
			}
		}

		private void HandleNatPunchConfirmRequest(int ptr, NetEndPoint senderEndPoint)
		{
			NetIncomingMessage tmp = SetupReadHelperMessage(ptr, 1000); // never mind length
			var isFromClient = tmp.ReadByte() == ClientByte;
			string token = tmp.ReadString();

			LogDebug("Received NAT punch confirmation from " + senderEndPoint + " sending NatIntroductionConfirmed - token is " + token);

			var confirmResponse = CreateMessage(1);
			confirmResponse.m_messageType = NetMessageType.NatIntroductionConfirmed;
			confirmResponse.Write(isFromClient ? HostByte : ClientByte);
			confirmResponse.Write(token);
			Interlocked.Increment(ref confirmResponse.m_recyclingCount);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(senderEndPoint, confirmResponse));
		}

		private void HandleNatPunchConfirmed(int ptr, NetEndPoint senderEndPoint)
		{
			NetIncomingMessage tmp = SetupReadHelperMessage(ptr, 1000); // never mind length
			var isFromClient = tmp.ReadByte() == ClientByte;
			if (isFromClient)
			{
				LogDebug("NAT punch confirmation received from " + senderEndPoint + " we're host, so we ignore this");
				return;
			}

			string token = tmp.ReadString();

			LogDebug("NAT punch confirmation received from " + senderEndPoint + " we're client so we go ahead and succeed the introduction");

			//
			// Release punch success to client; enabling him to Connect() to msg.Sender if token is ok
			//
			NetIncomingMessage punchSuccess = CreateIncomingMessage(NetIncomingMessageType.NatIntroductionSuccess, 10);
			punchSuccess.m_senderEndPoint = senderEndPoint;
			punchSuccess.Write(token);
			ReleaseMessage(punchSuccess);
	    }
	}
}
