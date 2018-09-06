using System;

namespace Microsoft.Xna.Framework.Net
{
    public class HostChangedEventArgs : EventArgs
    {
        public HostChangedEventArgs(NetworkGamer oldHost, NetworkGamer newHost)
        { }

        public NetworkGamer OldHost { get; }
        public NetworkGamer NewHost { get; }
    }
}