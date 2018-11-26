using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    public class GamerCollection<T> : ReadOnlyCollection<T>, IEnumerable<Gamer>, IEnumerable where T : Gamer
    {
        private IList<T> contents;
        private IList<T> reference;

        public GamerCollection(IList<T> list) : base(list)
        {
            if (list == null) throw new ArgumentNullException("list");

            this.contents = list;
        }

        internal GamerCollection(IList<T> contents, IList<T> reference) : base(contents)
        {
            if (contents == null) throw new ArgumentNullException("contents");
            if (reference == null) throw new ArgumentNullException("reference");

            this.contents = contents;
            this.reference = reference;
        }

        internal void CopyFromReference()
        {
            if (reference == null)
            {
                return;
            }

            contents.Clear();
            foreach (var item in reference)
            {
                contents.Add(item);
            }
        }

        IEnumerator<Gamer> IEnumerable<Gamer>.GetEnumerator()
        {
            return base.GetEnumerator();
        }
    }
}
