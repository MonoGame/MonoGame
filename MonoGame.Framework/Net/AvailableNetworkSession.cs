using System.Collections.Generic;
using System.Net;

using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class AvailableNetworkSession
    {
        internal IPEndPoint remoteEndPoint;
        internal IEnumerable<SignedInGamer> localGamers;
        internal int maxGamers;
        internal int privateGamerSlots;
        internal NetworkSessionType sessionType;

        internal AvailableNetworkSession(IPEndPoint remoteEndPoint, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionType sessionType, int currentGamerCount, string hostGamertag, int openPrivateGamerSlots, int openPublicGamerSlots, NetworkSessionProperties sessionProperties)
        {
            this.remoteEndPoint = remoteEndPoint;
            this.localGamers = localGamers;
            this.maxGamers = maxGamers;
            this.privateGamerSlots = privateGamerSlots;
            this.sessionType = sessionType;

            this.CurrentGamerCount = currentGamerCount;
            this.HostGamertag = hostGamertag;
            this.OpenPrivateGamerSlots = openPrivateGamerSlots;
            this.OpenPublicGamerSlots = openPublicGamerSlots;
            this.QualityOfService = new QualityOfService();
            this.SessionProperties = sessionProperties;
        }

        public int CurrentGamerCount { get; }
        public string HostGamertag { get; }
        public int OpenPrivateGamerSlots { get; }
        public int OpenPublicGamerSlots { get; }
        public QualityOfService QualityOfService { get; }
        public NetworkSessionProperties SessionProperties { get; }
    }
}