using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public class NetworkGamer : Gamer
    {
        internal NetworkGamer(bool isLocal, byte id) : base()
        {
            this.IsLocal = isLocal;
            this.Id = id;
        }

        public bool IsLocal { get; }
        public byte Id { get; }
    }
}