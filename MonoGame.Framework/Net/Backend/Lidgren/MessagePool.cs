using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal static class MessagePool
    {
        public static GenericPool<OutgoingMessage> Outgoing = new GenericPool<OutgoingMessage>();
        public static GenericPool<IncomingMessage> Incoming = new GenericPool<IncomingMessage>();
    }
}
