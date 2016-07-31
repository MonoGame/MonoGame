using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class AvailableNetworkSessionCollection : ReadOnlyCollection<AvailableNetworkSession>, IDisposable
    {
        public AvailableNetworkSessionCollection(IList<AvailableNetworkSession> list) : base(list)
        {}

        public void Dispose()
        {}
    }
}