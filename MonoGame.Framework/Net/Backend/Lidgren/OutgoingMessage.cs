using System;
using System.Net;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class OutgoingMessage : BaseOutgoingMessage, IResetable
    {
        public Peer recipient;
        public SendDataOptions options;
        public int channel;

        public OutgoingMessage()
        {
            Buffer = new NetBuffer();
        }

        public NetBuffer Buffer { get; private set; }
        public override Peer Recipient { get { return recipient; } }
        public override SendDataOptions Options { get { return options; } }
        public override int Channel { get { return channel; } }

        public void Reset()
        {
            Buffer.LengthBits = 0;
            Buffer.Position = 0;
            recipient = null;
            options = SendDataOptions.None;
            channel = 0;
        }

        public override void Write(string value)
        {
            Buffer.Write(value);
        }

        public override void Write(byte[] value)
        {
            Buffer.Write(value);
        }

        public override void Write(int value)
        {
            Buffer.Write(value);
        }

        public override void Write(long value)
        {
            Buffer.Write(value);
        }

        public override void Write(bool value)
        {
            Buffer.Write(value);
        }

        public override void Write(byte value)
        {
            Buffer.Write(value);
        }

        public override void Write(BasePeerEndPoint value)
        {
            Buffer.Write((value as GuidEndPoint).ToString());
        }

        public override void Write(Peer value)
        {
            Buffer.Write((value as LidgrenPeer).SessionId);
        }

        public void Write(IPEndPoint value)
        {
            Buffer.Write(value);
        }
    }
}
