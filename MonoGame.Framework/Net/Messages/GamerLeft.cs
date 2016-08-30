using Lidgren.Network;
using System;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GamerLeftMessageSender : IInternalMessageSender
    {
        private LocalNetworkGamer localGamer;

        public GamerLeftMessageSender(LocalNetworkGamer localGamer)
        {
            this.localGamer = localGamer;
        }

        public InternalMessageType MessageType { get { return InternalMessageType.GamerLeft; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            output.Write(localGamer.Id);
        }
    }

    internal struct GamerLeftMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            byte id = input.ReadByte();

            if (senderMachine.IsLocal)
            {
                NetworkGamer localGamer = NetworkSession.Session.FindGamerById(id);

                if (!localGamer.IsLocal)
                {
                    throw new NetworkException("Remote gamer left from local machine");
                }

                NetworkSession.Session.InvokeGamerLeftEvent(new GamerLeftEventArgs(localGamer));
            }
            else
            {
                NetworkGamer remoteGamer = NetworkSession.Session.FindGamerById(id);

                NetworkSession.Session.InvokeGamerLeftEvent(new GamerLeftEventArgs(remoteGamer));
                NetworkSession.Session.RemoveGamer(remoteGamer);
            }
        }
    }
}
