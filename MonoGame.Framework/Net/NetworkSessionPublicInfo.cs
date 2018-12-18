using System;
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

        private bool initialized;

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

            initialized = true;
        }

        public void Pack(NetOutgoingMessage msg)
        {
            if (!initialized) throw new InvalidOperationException("Cannot pack uninitialized public info");

            msg.Write((byte)sessionType);
            sessionProperties.Pack(msg);
            msg.Write(hostGamertag);
            msg.Write(maxGamers);
            msg.Write(privateGamerSlots);
            msg.Write(currentGamerCount);
            msg.Write(openPrivateGamerSlots);
            msg.Write(openPublicGamerSlots);
        }

        public bool Unpack(NetIncomingMessage msg)
        {
            NetworkSessionType sessionType;
            NetworkSessionProperties sessionProperties = new NetworkSessionProperties();
            string hostGamertag;
            int maxGamers, privateGamerSlots, currentGamerCount, openPrivateGamerSlots, openPublicGamerSlots;
            try
            {
                sessionType = (NetworkSessionType)msg.ReadByte();
                if (!sessionProperties.Unpack(msg))
                {
                    return false;
                }
                hostGamertag = msg.ReadString();
                maxGamers = msg.ReadInt32();
                privateGamerSlots = msg.ReadInt32();
                currentGamerCount = msg.ReadInt32();
                openPrivateGamerSlots = msg.ReadInt32();
                openPublicGamerSlots = msg.ReadInt32();
            }
            catch
            {
                return false;
            }

            if (this.sessionProperties == null)
            {
                // NetworkSessionProperties sent over the network are read-only
                this.sessionProperties = new NetworkSessionProperties(true);
            }
            this.sessionType = sessionType;
            this.sessionProperties.CopyValuesFrom(sessionProperties);
            this.hostGamertag = hostGamertag;
            this.maxGamers = maxGamers;
            this.privateGamerSlots = privateGamerSlots;
            this.currentGamerCount = currentGamerCount;
            this.openPrivateGamerSlots = openPrivateGamerSlots;
            this.openPublicGamerSlots = openPublicGamerSlots;

            initialized = true;
            return true;
        }
    }
}
