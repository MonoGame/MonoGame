using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectPassCollection : IEnumerable<EffectPass>
    {
		private readonly EffectPass[] _passes;

        internal EffectPassCollection(EffectPass [] passes)
        {
            _passes = passes;
        }

        internal EffectPassCollection Clone(Effect effect)
        {
            var passes = new EffectPass[_passes.Length];
            for (var i = 0; i < _passes.Length; i++)
                passes[i] = new EffectPass(effect, _passes[i]);

            return new EffectPassCollection(passes);
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
            get { return _passes.Length; }
        }

        IEnumerator<EffectPass> IEnumerable<EffectPass>.GetEnumerator()
        {
            return ((IEnumerable<EffectPass>)_passes).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _passes.GetEnumerator();
        }
    }
}
