using System;

namespace Microsoft.Xna.Framework.Net
{
    public static class NetworkSessionSettings
    {
        private static string Id = string.Empty;

        public static string GameAppId
        {
            get
            {
                if (Game.Instance == null)
                {
                    throw new InvalidOperationException("Should only be called from within a MonoGame Game instance. For the master server, manually specify the GUID of the game to relay.");
                }

                if (Id == string.Empty)
                {
                    var assembly = System.Reflection.Assembly.GetAssembly(Game.Instance.GetType());

                    if (assembly != null)
                    {
                        object[] guidObjs = assembly.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);

                        if (guidObjs != null && guidObjs.Length > 0)
                        {
                            Id = ((System.Runtime.InteropServices.GuidAttribute)guidObjs[0]).Value;
                        }
                    }
                }

                return Id;
            }

            set
            {
                Id = value;
            }
        }

        public static int Port = 14242;
        public static string MasterServerAddress = "127.0.0.1";
        public static int MasterServerPort = 14243;
        public static TimeSpan MasterServerRegistrationInterval = TimeSpan.FromSeconds(60.0);
    }
}
