using Microsoft.Xna.Framework.Net.Backend;
using Microsoft.Xna.Framework.Net.Backend.Lidgren;

namespace Microsoft.Xna.Framework.Net
{
    internal class NetworkSessionImplementation
    {
        // This is where the backend implementation is defined
        private static SessionCreator SessionCreatorInstance;
        private static MasterServer MasterServerInstance;

        internal static SessionCreator SessionCreator
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

        internal static MasterServer MasterServer
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
