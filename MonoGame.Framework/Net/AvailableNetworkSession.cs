using System.Net;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class AvailableNetworkSession
    {
        internal IPEndPoint remoteEndPoint;
        private string hostGamertag;

        internal AvailableNetworkSession(IPEndPoint remoteEndPoint, string hostGamertag)
        {
            this.remoteEndPoint = remoteEndPoint;
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