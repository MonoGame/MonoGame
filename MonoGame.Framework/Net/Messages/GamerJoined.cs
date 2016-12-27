﻿using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class GamerJoined : InternalMessage
    {
        public void Create(LocalNetworkGamer localGamer, NetworkMachine recipient)
        {
            OutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.GamerJoined);

            msg.Write(localGamer.DisplayName);
            msg.Write(localGamer.Gamertag);
            msg.Write(localGamer.Id);
            msg.Write(localGamer.IsPrivateSlot);
            msg.Write(localGamer.IsReady);

            Queue.Place(msg);
        }

        public override void Receive(IncomingMessage msg, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                return;
            }
            if (!senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            string displayName = msg.ReadString();
            string gamertag = msg.ReadString();
            byte id = msg.ReadByte();
            bool isPrivateSlot = msg.ReadBoolean();
            bool isReady = msg.ReadBoolean();

            if (CurrentMachine.Session.FindGamerById(id) != null)
            {
                // TODO: SuspiciousGamerIdCollision
                Debug.Assert(false);
                return;
            }

            NetworkGamer remoteGamer = new NetworkGamer(senderMachine, displayName, gamertag, id, isPrivateSlot, isReady);
            CurrentMachine.Session.AddGamer(remoteGamer);
        }
    }
}
