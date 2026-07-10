using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a collection of <see cref="EffectTechnique"/> objects.
    /// </summary>
    public class EffectTechniqueCollection : IEnumerable<EffectTechnique>
    {
		private readonly EffectTechnique[] _techniques;

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
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

        /// <summary>
        /// Retrieves the <see cref="EffectTechnique"/> at the specified index in the collection.
        /// </summary>
        public EffectTechnique this[int index]
        {
            get { return _techniques [index]; }
        }

        /// <summary>
        /// Retrieves a <see cref="EffectTechnique"/> from the collection, given the name of the technique.
        /// </summary>
        /// <param name="name">The name of the technique to retrieve.</param>
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

        /// <inheritdoc/>
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
