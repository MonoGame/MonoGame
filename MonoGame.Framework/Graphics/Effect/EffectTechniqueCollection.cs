using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectTechniqueCollection : IEnumerable<EffectTechnique>
    {

#if NOMOJO
        public EffectTechniqueCollection() : base() { }


        public EffectTechniqueCollection(Effect effect)
        {
            var tech = new EffectTechnique(effect);
            tech.Passes = new EffectPassCollection(tech);
            Add(tech);
        }
#endif


        // Modified to be a list instead of dictionary object because a dictionary does not guarantee
		// the order is kept as it is a hash key.
		internal List <EffectTechnique> _techniques = new List<EffectTechnique> ();
        //Dictionary<string, EffectTechnique> _techniques = new Dictionary<string, EffectTechnique>();

        public EffectTechnique this[int index]
        {
            get { return _techniques [index]; }
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
        }

        public IEnumerator<EffectTechnique> GetEnumerator()
        {
            return _techniques.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _techniques.GetEnumerator();
        }

        internal void Add(EffectTechnique technique)
        {
            _techniques.Add(technique);

            if (_techniques.Count == 1)
                technique._effect.CurrentTechnique = technique;
        }
    }
}
