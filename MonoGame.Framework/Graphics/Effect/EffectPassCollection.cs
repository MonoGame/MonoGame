using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectPassCollection : IEnumerable<EffectPass>
    {
		// Modified to be a list instead of dictionary object because a dictionary does not guarantee
		// the order is kept as it is a hash key.
		internal List<EffectPass> _passes = new List<EffectPass>();
        //Dictionary<string, EffectPass> _passes = new Dictionary<string, EffectPass>();
        private EffectTechnique _effectTechnique;

        public EffectPassCollection(EffectTechnique effectTechnique)
        {
            _effectTechnique = effectTechnique;
			
        }

        public EffectPass this[int index]
        {
            get { return _passes[index]; }
            set { 
				_passes[index] = value; 
			}
        }

        public EffectPass this[string name]
        {
            get {
				foreach (EffectPass pass in _passes) {
					if (pass.Name == name)
						return pass;
				}
				return null;
		}
            set {

				var pass = this[name];
				if (pass != null)
					pass = value;
				else
					_passes.Add(value);
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
    }
}
