using System.Collections.Generic;
using System.Net;

using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class AvailableNetworkSession
    {
        internal IPEndPoint remoteEndPoint;
        internal IEnumerable<SignedInGamer> gamers;
        private string hostGamertag;

        internal AvailableNetworkSession(IPEndPoint remoteEndPoint, IEnumerable<SignedInGamer> gamers, string hostGamertag)
        {
            this.remoteEndPoint = remoteEndPoint;
            this.gamers = gamers;
            this.hostGamertag = hostGamertag;
        }

        //public int CurrentGamerCount { get; }
        public string HostGamertag { get { return hostGamertag; } }
        //public int OpenPrivateGamerSlots { get; }
        //public int OpenPublicGamerSlots { get; }
        //public QualityOfService QualityOfService { get; }
        //public NetworkSessionProperties SessionProperties { get; }
    }
}