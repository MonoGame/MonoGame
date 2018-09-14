using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    internal class NetworkSessionPublicInfo
    {
        public NetworkSessionType sessionType;
        public NetworkSessionProperties sessionProperties;
        public string hostGamertag;
        public int maxGamers;
        public int privateGamerSlots;
        public int currentGamerCount;
        public int openPrivateGamerSlots;
        public int openPublicGamerSlots;

        public static NetworkSessionPublicInfo FromMessage(NetIncomingMessage msg)
        {
            var publicInfo = new NetworkSessionPublicInfo();
            publicInfo.Unpack(msg);
            return publicInfo;
        }

        public void Set(NetworkSessionType sessionType, NetworkSessionProperties sessionProperties, string hostGamertag, int maxGamers, int privateGamerSlots, int currentGamerCount, int openPrivateGamerSlots, int openPublicGamerSlots)
        {
            this.sessionType = sessionType;
            this.sessionProperties = sessionProperties;
            this.hostGamertag = hostGamertag;
            this.maxGamers = maxGamers;
            this.privateGamerSlots = privateGamerSlots;
            this.currentGamerCount = currentGamerCount;
            this.openPrivateGamerSlots = openPrivateGamerSlots;
            this.openPublicGamerSlots = openPublicGamerSlots;
        }

        public void Pack(NetOutgoingMessage msg)
        {
            msg.Write((byte)sessionType);
            sessionProperties.Pack(msg);
            msg.Write(hostGamertag);
            msg.Write(maxGamers);
            msg.Write(privateGamerSlots);
            msg.Write(currentGamerCount);
            msg.Write(openPrivateGamerSlots);
            msg.Write(openPublicGamerSlots);
        }

        public void Unpack(NetIncomingMessage msg)
        {
            if (sessionProperties == null)
            {
                // If this NetworkSessionPublicInfo is sent over the network, NetworkSessionProperties should be read-only
                sessionProperties = new NetworkSessionProperties(true);
            }

            sessionType = (NetworkSessionType)msg.ReadByte();
            sessionProperties.Unpack(msg);
            hostGamertag = msg.ReadString();
            maxGamers = msg.ReadInt32();
            privateGamerSlots = msg.ReadInt32();
            currentGamerCount = msg.ReadInt32();
            openPrivateGamerSlots = msg.ReadInt32();
            openPublicGamerSlots = msg.ReadInt32();
        }
    }
}
