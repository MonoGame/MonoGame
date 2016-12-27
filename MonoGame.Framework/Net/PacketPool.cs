using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Net
{
    internal class Packet
    {
        public int length;
        public byte[] data;

        public Packet(int length)
        {
            this.length = length;
            this.data = new byte[length];

            this.Reset();
        }

        public void Reset()
        {
            for (int i = 0; i < length; i++)
            {
                data[i] = 255;
            }
        }
    }

    internal class PacketPool
    {
        protected IList<Packet> freePackets = new List<Packet>();

        public Packet Get(int requestedLength)
        {
            for (int i = freePackets.Count - 1; i >= 0; i--)
            {
                if (freePackets[i].length == requestedLength)
                {
                    Packet packet = freePackets[i];
                    freePackets.RemoveAt(i);
                    return packet;
                }
            }

            return new Packet(requestedLength);
        }

        public Packet GetAndFillWith(byte[] source)
        {
            return GetAndFillWith(source, 0, source.Length);
        }

        public Packet GetAndFillWith(byte[] source, int offset, int count)
        {
            Packet packet = Get(count);
            
            Array.Copy(source, offset, packet.data, 0, count);

            return packet;
        }

        public void Recycle(Packet packet)
        {
            packet.Reset();

            freePackets.Add(packet);
        }
    }
}
