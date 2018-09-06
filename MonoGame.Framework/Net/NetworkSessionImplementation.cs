using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net
{
    /// <summary>
    /// This is where the Net namespace backend implementation is defined
    /// </summary>
    internal class NetworkSessionImplementation
    {
        private static BaseSessionCreator SessionCreatorInstance;
        private static BaseMasterServer MasterServerInstance;

        public static BaseSessionCreator SessionCreator
        {
            get
            {
                if (SessionCreatorInstance == null)
                {
                    SessionCreatorInstance = new Backend.Lidgren.SessionCreator();
                }
                return SessionCreatorInstance;
            }
        }

        public static BaseMasterServer MasterServer
        {
            get
            {
                if (MasterServerInstance == null)
                {
                    MasterServerInstance = new Backend.Lidgren.MasterServer();
                }
                return MasterServerInstance;
            }
        }
    }
}
