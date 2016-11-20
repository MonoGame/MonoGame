using System;

namespace Microsoft.Xna.Framework.Net
{
    public static class NetworkSessionSettings
    {
        public static string AppId = "MonoGameApp";
        public static int Port = 14242;
        public static string MasterServerAddress = "127.0.0.1";
        public static int MasterServerPort = 14243;
        public static TimeSpan MasterServerRegistrationInterval = TimeSpan.FromSeconds(60.0);
    }
}
