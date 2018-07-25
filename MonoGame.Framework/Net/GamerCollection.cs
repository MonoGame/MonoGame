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
            contents = list ?? throw new ArgumentNullException(nameof(list));
        }

        internal GamerCollection(IList<T> contents, IList<T> reference) : base(contents)
        {
            this.contents = contents ?? throw new ArgumentNullException(nameof(contents));
            this.reference = reference ?? throw new ArgumentNullException(nameof(reference));
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
