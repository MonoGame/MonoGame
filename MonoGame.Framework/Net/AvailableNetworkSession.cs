namespace Microsoft.Xna.Framework.Net
{
    public sealed class AvailableNetworkSession
    {
        private string hostGamertag;

        internal AvailableNetworkSession(string hostGamertag)
        {
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