using Microsoft.Xna.Framework.Net.Backend;
using Microsoft.Xna.Framework.Net.Backend.Lidgren;

namespace Microsoft.Xna.Framework.Net
{
    internal class NetworkSessionImplementation
    {
        // This is where the backend implementation is defined
        public static ISessionCreator SessionCreator = new LidgrenSessionCreator();
    }
}
