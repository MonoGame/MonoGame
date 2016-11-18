using System;
using System.Collections;
using System.Collections.Generic;
using Lidgren.Network;
using Microsoft.Xna.Framework.Net.Backend;
using System.IO;

namespace Microsoft.Xna.Framework.Net
{
    internal class NetworkSessionPublicInfo
    {
        internal NetworkSessionType sessionType;
        internal NetworkSessionProperties sessionProperties;
        internal string hostGamertag;
        internal int maxGamers;
        internal int privateGamerSlots;
        internal int currentGamerCount;
        internal int openPrivateGamerSlots;
        internal int openPublicGamerSlots;

        internal static NetworkSessionPublicInfo FromMessage(IIncomingMessage msg)
        {
            NetworkSessionPublicInfo publicInfo = new NetworkSessionPublicInfo();

            publicInfo.Unpack(msg);

            return publicInfo;
        }

        internal void Set(NetworkSessionType sessionType, NetworkSessionProperties sessionProperties, string hostGamertag, int maxGamers, int privateGamerSlots, int currentGamerCount, int openPrivateGamerSlots, int openPublicGamerSlots)
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

        internal void Pack(IOutgoingMessage msg)
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

        internal void Unpack(IIncomingMessage msg)
        {
            if (sessionProperties == null)
            {
                sessionProperties = new NetworkSessionProperties();
            }

            sessionType = (NetworkSessionType)msg.ReadByte();
            sessionProperties.Unpack(msg);
            hostGamertag = msg.ReadString();
            maxGamers = msg.ReadInt();
            privateGamerSlots = msg.ReadInt();
            currentGamerCount = msg.ReadInt();
            openPrivateGamerSlots = msg.ReadInt();
            openPublicGamerSlots = msg.ReadInt();
        }
    }
}