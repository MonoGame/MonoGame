using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public class GamerCollection<T> : ReadOnlyCollection<T>, IEnumerable<Gamer>, IEnumerable where T : Gamer
    {
        public GamerCollection(IList<T> list) : base(list)
        { }

        IEnumerator<Gamer> IEnumerable<Gamer>.GetEnumerator()
        {
            return base.GetEnumerator();
        }
    }
}