using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class OutgoingMessage : IOutgoingMessage, IResetable
    {
        public OutgoingMessage()
        {
            this.Buffer = new NetBuffer();
        }

        internal NetBuffer Buffer { get; private set; }
        public IPeer Recipient { get; internal set; }
        public SendDataOptions Options { get; internal set; }
        public int Channel { get; internal set; }

        public void Reset()
        {
            Buffer.LengthBits = 0;
            Buffer.Position = 0;
            Recipient = null;
            Options = SendDataOptions.None;
            Channel = 0;
        }

        public void Write(string value)
        {
            Buffer.Write(value);
        }

        public void Write(byte[] value)
        {
            Buffer.Write(value);
        }

        public void Write(int value)
        {
            Buffer.Write(value);
        }

        public void Write(long value)
        {
            Buffer.Write(value);
        }

        public void Write(bool value)
        {
            Buffer.Write(value);
        }

        public void Write(byte value)
        {
            Buffer.Write(value);
        }

        public void Write(IPeerEndPoint value)
        {
            LidgrenEndPoint ep = value as LidgrenEndPoint;

            Buffer.Write(ep.endPoint);
        }

        public void Write(IPeer value)
        {
            Buffer.Write((value as ILidgrenPeer).Id);
        }
    }

    internal class IncomingMessage : IIncomingMessage, IResetable
    {
        internal LidgrenBackend Backend { get; set; }
        internal NetBuffer Buffer { get; set; }

        public IncomingMessage()
        { }

        public IncomingMessage(NetBuffer buffer)
        {
            this.Buffer = buffer;
        }

        public void Reset()
        {
            Backend = null;
            Buffer = null;
        }

        public bool ReadBoolean()
        {
            return Buffer.ReadBoolean();
        }

        public byte ReadByte()
        {
            return Buffer.ReadByte();
        }

        public void ReadBytes(byte[] into, int offset, int length)
        {
            Buffer.ReadBytes(into, offset, length);
        }

        public int ReadInt()
        {
            return Buffer.ReadInt32();
        }

        public long ReadLong()
        {
            return Buffer.ReadInt64();
        }

        public IPeerEndPoint ReadPeerEndPoint()
        {
            return new LidgrenEndPoint(Buffer.ReadIPEndPoint());
        }

        public IPeer ReadPeer()
        {
            return Backend.FindPeerById(Buffer.ReadInt64());
        }

        public string ReadString()
        {
            return Buffer.ReadString();
        }
    }
}
