using System;
using System.Net;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class LidgrenOutgoingMessage : OutgoingMessage, IResetable
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
            Buffer.Write((value as LidgrenGuidEndPoint).ToString());
        }

        public override void Write(Peer value)
        {
            Buffer.Write((value as LidgrenPeer).SessionId);
        }

        internal void Write(IPEndPoint value)
        {
            Buffer.Write(value);
        }
    }

    internal class LidgrenIncomingMessage : IncomingMessage, IResetable
    {
        public LidgrenBackend Backend { get; private set; }
        public NetBuffer Buffer { get; private set; }

        public LidgrenIncomingMessage()
        { }

        public LidgrenIncomingMessage(NetBuffer buffer)
        {
            this.Buffer = buffer;
        }

        public void Set(LidgrenBackend backend, NetBuffer buffer)
        {
            this.Backend = backend;
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
            return LidgrenGuidEndPoint.Parse(Buffer.ReadString());
        }

        public override Peer ReadPeer()
        {
            if (Backend == null)
            {
                throw new InvalidOperationException("Backend is null, cannot read peer without context");
            }
            return Backend.FindPeerById(Buffer.ReadInt64());
        }

        public override string ReadString()
        {
            return Buffer.ReadString();
        }

        internal IPEndPoint ReadIPEndPoint()
        {
            return Buffer.ReadIPEndPoint();
        }
    }
}
