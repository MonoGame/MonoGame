﻿using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class AllowEndPointRequest : InternalMessage
    {
        public void Create(PeerEndPoint endPoint, NetworkMachine recipient)
        {
            if (!CurrentMachine.IsHost)
            {
                throw new NetworkException("Only host can send AllowEndPointRequest");
            }

            OutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.AllowEndPointRequest);
            msg.Write(endPoint);
            Queue.Place(msg);
        }

        public override void Receive(IncomingMessage msg, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }

            PeerEndPoint endPoint = msg.ReadPeerEndPoint();

            CurrentMachine.Session.allowlist.Add(endPoint);

            CurrentMachine.Session.InternalMessages.AllowEndPointResponse.Create(endPoint, senderMachine);
        }
    }
}
