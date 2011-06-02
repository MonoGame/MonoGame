using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectTechniqueCollection : IEnumerable<EffectTechnique>
    {
		// Modified to be a list instead of dictionary object because a dictionary does not guarantee
		// the order is kept as it is a hash key.
		internal List <EffectTechnique> _techniques = new List<EffectTechnique> ();
        //Dictionary<string, EffectTechnique> _techniques = new Dictionary<string, EffectTechnique>();

        public EffectTechnique this[int index]
        {
            get { return _techniques [index]; }
            set { _techniques [index] = value; }
        }

        public EffectTechnique this[string name]
        {
            get {
				foreach (EffectTechnique technique in _techniques) {
					if (technique.Name == name)
						return technique;
				}
				return null;
		}
            set {

				var technique = this[name];
				if (technique != null)
					technique = value;
				else
					_techniques.Add(value);
			}
        }

        public IEnumerator<EffectTechnique> GetEnumerator()
        {
            return _techniques.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _techniques.GetEnumerator();
        }
    }
}
