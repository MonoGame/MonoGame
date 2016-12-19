using Microsoft.Xna.Framework.Net.Backend;
using Microsoft.Xna.Framework.Net.Backend.Lidgren;

namespace Microsoft.Xna.Framework.Net
{
    internal class NetworkSessionImplementation
    {
        // This is where the backend implementation is defined
        private static ISessionCreator SessionCreatorInstance;
        private static IMasterServer MasterServerInstance;

        internal static ISessionCreator SessionCreator
        {
            get
            {
                if (SessionCreatorInstance == null)
                {
                    SessionCreatorInstance = new LidgrenSessionCreator();
                }
                return SessionCreatorInstance;
            }
        }

        internal static IMasterServer MasterServer
        {
            get
            {
                if (MasterServerInstance == null)
                {
                    MasterServerInstance = new LidgrenMasterServer();
                }
                return MasterServerInstance;
            }
        }
    }
}
