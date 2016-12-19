using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class NetworkSessionMasterServer
    {
        private IMasterServer server = NetworkSessionImplementation.MasterServer;

        public void Start(string appId)
        {
            server.Start(appId);
        }

        public void Update()
        {
            server.Update();
        }

        public void Shutdown()
        {
            server.Shutdown();
        }
    }
}
