using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class AvailableNetworkSession
    {
        internal AvailableNetworkSession(Guid hostGuid, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionType sessionType, int currentGamerCount, string hostGamertag, int openPrivateGamerSlots, int openPublicGamerSlots, NetworkSessionProperties sessionProperties)
        {
            this.HostGuid = hostGuid;
            this.SessionType = sessionType;
            this.LocalGamers = localGamers;
            this.MaxGamers = maxGamers;
            this.PrivateGamerSlots = privateGamerSlots;

            this.CurrentGamerCount = currentGamerCount;
            this.HostGamertag = hostGamertag;
            this.OpenPrivateGamerSlots = openPrivateGamerSlots;
            this.OpenPublicGamerSlots = openPublicGamerSlots;
            this.QualityOfService = new QualityOfService();
            this.SessionProperties = sessionProperties;
        }

        internal Guid HostGuid { get; }
        internal NetworkSessionType SessionType { get; }
        internal IEnumerable<SignedInGamer> LocalGamers { get; }
        internal int MaxGamers { get; }
        internal int PrivateGamerSlots { get; }
        internal object Tag { get; set; }

        public int CurrentGamerCount { get; }
        public string HostGamertag { get; }
        public int OpenPrivateGamerSlots { get; }
        public int OpenPublicGamerSlots { get; }
        public QualityOfService QualityOfService { get; }
        public NetworkSessionProperties SessionProperties { get; }
    }
}
