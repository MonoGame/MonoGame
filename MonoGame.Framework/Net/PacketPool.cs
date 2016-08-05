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
        }
    }

    internal class PacketPool
    {
        protected IList<Packet> freePackets = new List<Packet>();

        public Packet GetPacket(int length)
        {
            Packet packet = null;

            foreach (Packet freePacket in freePackets)
            {
                if (freePacket.length == length)
                {
                    packet = freePacket;
                    break;
                }
            }

            if (packet != null)
            {
                return packet;
            }
            
            return new Packet(length);
        }

        public void RecyclePacket(Packet packet)
        {
            freePackets.Add(packet);
        }
    }
}
