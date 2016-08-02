using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class LocalNetworkGamer : NetworkGamer
    {
        public LocalNetworkGamer(SignedInGamer signedInGamer, byte id) : base(true, id)
        {
            this.SignedInGamer = signedInGamer;
        }

        public SignedInGamer SignedInGamer { get; }
    }
}