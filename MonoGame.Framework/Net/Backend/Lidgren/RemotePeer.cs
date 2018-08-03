using System;
using System.Net;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal abstract class LidgrenPeer : Peer
    {
        public abstract long SessionId { get; }
    }
    internal class RemotePeer : LidgrenPeer
    {
        private NetConnection connection;
        private GuidEndPoint endPoint;
        private IPEndPoint internalIp;
        private IPEndPoint externalIp;

        public RemotePeer(NetConnection connection, GuidEndPoint endPoint, IPEndPoint internalIp, IPEndPoint externalIp)
        {
            this.connection = connection;
            this.endPoint = endPoint;
            this.internalIp = internalIp;
            this.externalIp = externalIp;
        }

        public NetConnection Connection { get { return connection; } }
        public IPEndPoint InternalIp { get { return internalIp; } } // Might differ from Connection!
        public IPEndPoint ExternalIp { get { return externalIp; } } // Might differ from Connection!

        public override BasePeerEndPoint EndPoint { get { return endPoint; } }
        public override long SessionId { get { return connection.RemoteUniqueIdentifier; } }
        public override TimeSpan RoundtripTime { get { return TimeSpan.FromSeconds(connection.AverageRoundtripTime); } }
        public override object Tag { get; set; }

        public override void Disconnect(string byeMessage)
        {
            connection.Disconnect(byeMessage);
        }
    }
}
