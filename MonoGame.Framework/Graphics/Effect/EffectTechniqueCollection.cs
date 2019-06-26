using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectTechniqueCollection : IEnumerable<EffectTechnique>
    {
		private readonly EffectTechnique[] _techniques;

        public int Count { get { return _techniques.Length; } }

        internal EffectTechniqueCollection(EffectTechnique[] techniques)
        {
            _techniques = techniques;
        }

        internal EffectTechniqueCollection Clone(Effect effect)
        {
            var techniques = new EffectTechnique[_techniques.Length];
            for (var i = 0; i < _techniques.Length; i++)
                techniques[i] = new EffectTechnique(effect, _techniques[i]);

            return new EffectTechniqueCollection(techniques);
        }
        
        public EffectTechnique this[int index]
        {
            get { return _techniques [index]; }
        }

        public EffectTechnique this[string name]
        {
            get 
            {
                // TODO: Add a name to technique lookup table.
				foreach (var technique in _techniques) 
                {
					if (technique.Name == name)
						return technique;
			    }

			    return null;
		    }
        }

        public IEnumerator<EffectTechnique> GetEnumerator()
        {
            return ((IEnumerable<EffectTechnique>)_techniques).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _techniques.GetEnumerator();
        }
    }
}
