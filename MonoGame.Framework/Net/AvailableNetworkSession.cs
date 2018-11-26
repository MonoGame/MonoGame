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

        internal Guid HostGuid { get; private set; }
        internal NetworkSessionType SessionType { get; private set; }
        internal IEnumerable<SignedInGamer> LocalGamers { get; private set; }
        internal int MaxGamers { get; private set; }
        internal int PrivateGamerSlots { get; private set; }
        internal object Tag { get; set; }

        public int CurrentGamerCount { get; private set; }
        public string HostGamertag { get; private set; }
        public int OpenPrivateGamerSlots { get; private set; }
        public int OpenPublicGamerSlots { get; private set; }
        public QualityOfService QualityOfService { get; private set; }
        public NetworkSessionProperties SessionProperties { get; private set; }
    }
}
