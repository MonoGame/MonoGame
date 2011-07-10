﻿using System;
using System.Collections.Generic;
using System.Net;

namespace Lidgren.Network
{
	public partial class NetPeer
	{
		/// <summary>
		/// Send NetIntroduction to hostExternal and clientExternal; introducing client to host
		/// </summary>
		public void Introduce(
			IPEndPoint hostInternal,
			IPEndPoint hostExternal,
			IPEndPoint clientInternal,
			IPEndPoint clientExternal,
			string token)
		{
			// send message to client
			NetOutgoingMessage msg = CreateMessage(10 + token.Length + 1);
			msg.m_messageType = NetMessageType.NatIntroduction;
			msg.Write(false);
			msg.WritePadBits();
			msg.Write(hostInternal);
			msg.Write(hostExternal);
			msg.Write(token);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(clientExternal, msg));

			// send message to host
			msg = CreateMessage(10 + token.Length + 1);
			msg.m_messageType = NetMessageType.NatIntroduction;
			msg.Write(true);
			msg.WritePadBits();
			msg.Write(clientInternal);
			msg.Write(clientExternal);
			msg.Write(token);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(hostExternal, msg));
		}

		/// <summary>
		/// Called when host/client receives a NatIntroduction message from a master server
		/// </summary>
		private void HandleNatIntroduction(int ptr)
		{
			VerifyNetworkThread();

			// read intro
			NetIncomingMessage tmp = SetupReadHelperMessage(ptr, 1000); // never mind length

			byte hostByte = tmp.ReadByte();
			IPEndPoint remoteInternal = tmp.ReadIPEndpoint();
			IPEndPoint remoteExternal = tmp.ReadIPEndpoint();
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
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(remoteInternal, punch));

			// send external punch
			punch = CreateMessage(1);
			punch.m_messageType = NetMessageType.NatPunchMessage;
			punch.Write(hostByte);
			punch.Write(token);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(remoteExternal, punch));
		}

		/// <summary>
		/// Called when receiving a NatPunchMessage from a remote endpoint
		/// </summary>
		private void HandleNatPunch(int ptr, IPEndPoint senderEndpoint)
		{
			NetIncomingMessage tmp = SetupReadHelperMessage(ptr, 1000); // never mind length

			byte fromHostByte = tmp.ReadByte();
			if (fromHostByte == 0)
			{
				// it's from client
				LogDebug("NAT punch received from " + senderEndpoint + " we're host, so we ignore this");
				return; // don't alert hosts about nat punch successes; only clients
			}
			string token = tmp.ReadString();

			LogDebug("NAT punch received from " + senderEndpoint + " we're client, so we've succeeded - token is " + token);

			//
			// Release punch success to client; enabling him to Connect() to msg.SenderIPEndPoint if token is ok
			//
			NetIncomingMessage punchSuccess = CreateIncomingMessage(NetIncomingMessageType.NatIntroductionSuccess, 10);
			punchSuccess.m_senderEndpoint = senderEndpoint;
			punchSuccess.Write(token);
			ReleaseMessage(punchSuccess);
		}
	}
}
