using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectPassCollection : IEnumerable<EffectPass>
    {
		private List<EffectPass> _passes = new List<EffectPass>();

        internal EffectPassCollection()
        {
        }

        internal EffectPassCollection(Effect effect, EffectPassCollection cloneSource)
        {
            foreach (var pass in cloneSource)
                Add(new EffectPass(effect, pass));
        }

        public EffectPass this[int index]
        {
            get { return _passes[index]; }
        }

        public EffectPass this[string name]
        {
            get 
            {
                // TODO: Add a name to pass lookup table.
				foreach (var pass in _passes) 
                {
					if (pass.Name == name)
						return pass;
				}
				return null;
		    }
        }

        public int Count
        {
            get { return _passes.Count; }
        }

        public IEnumerator<EffectPass> GetEnumerator()
        {
            return _passes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _passes.GetEnumerator();
        }

        internal void Add(EffectPass pass)
        {
            _passes.Add(pass);
        }
    }
}
