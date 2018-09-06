using System;
using System.Net;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class IncomingMessage : BaseIncomingMessage, IResetable
    {
        public SessionBackend Backend { get; private set; }
        public NetBuffer Buffer { get; private set; }

        public IncomingMessage()
        { }

        public IncomingMessage(NetBuffer buffer)
        {
            Buffer = buffer;
        }

        public void Set(SessionBackend backend, NetBuffer buffer)
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

        public GuidEndPoint ReadGuidEndPoint()
        {
            return GuidEndPoint.Parse(Buffer.ReadString());
        }

        public override BasePeerEndPoint ReadPeerEndPoint()
        {
            return GuidEndPoint.Parse(Buffer.ReadString());
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

        public IPEndPoint ReadIPEndPoint()
        {
            return Buffer.ReadIPEndPoint();
        }
    }
}
