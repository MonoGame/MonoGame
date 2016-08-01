using System;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkSessionEndedEventArgs : EventArgs
    {
        public NetworkSessionEndedEventArgs(NetworkSessionEndReason endReason)
        {
            this.EndReason = endReason;
        }

        public NetworkSessionEndReason EndReason { get; }
    }
}