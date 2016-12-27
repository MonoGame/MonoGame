using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class LidgrenOutgoingMessage : Backend.OutgoingMessage, IResetable
    {
        internal Peer recipient;
        internal SendDataOptions options;
        internal int channel;

        public LidgrenOutgoingMessage()
        {
            this.Buffer = new NetBuffer();
        }

        internal NetBuffer Buffer { get; private set; }
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

        public override void Write(PeerEndPoint value)
        {
            LidgrenEndPoint ep = value as LidgrenEndPoint;

            Buffer.Write(ep.endPoint);
        }

        public override void Write(Peer value)
        {
            Buffer.Write((value as ILidgrenPeer).Id);
        }
    }

    internal class LidgrenIncomingMessage : Backend.IncomingMessage, IResetable
    {
        internal LidgrenBackend Backend { get; set; }
        internal NetBuffer Buffer { get; set; }

        public LidgrenIncomingMessage()
        { }

        public LidgrenIncomingMessage(NetBuffer buffer)
        {
            this.Buffer = buffer;
        }

        public void Reset()
        {
            Backend = null;
            Buffer = null;
        }

        public override bool ReadBoolean()
        {
            return Buffer.ReadBoolean();
        }

        public override byte ReadByte()
        {
            return Buffer.ReadByte();
        }

        public override void ReadBytes(byte[] into, int offset, int length)
        {
            Buffer.ReadBytes(into, offset, length);
        }

        public override int ReadInt()
        {
            return Buffer.ReadInt32();
        }

        public override long ReadLong()
        {
            return Buffer.ReadInt64();
        }

        public override PeerEndPoint ReadPeerEndPoint()
        {
            return new LidgrenEndPoint(Buffer.ReadIPEndPoint());
        }

        public override Peer ReadPeer()
        {
            return Backend.FindPeerById(Buffer.ReadInt64());
        }

        public override string ReadString()
        {
            return Buffer.ReadString();
        }
    }
}
