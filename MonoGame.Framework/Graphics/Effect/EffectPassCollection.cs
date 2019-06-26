using System;
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

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_passes);
        }
            
        IEnumerator<EffectPass> IEnumerable<EffectPass>.GetEnumerator()
        {
            return ((IEnumerable<EffectPass>)_passes).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _passes.GetEnumerator();
        }

        public struct Enumerator : IEnumerator<EffectPass>
        {
            private readonly EffectPass[] _array;
            private int _index;
            private EffectPass _current;

            internal Enumerator(EffectPass[] array)
            {
                _array = array;
                _index = 0;
                _current = null;
            }

            public bool MoveNext()
            {
                if (_index < _array.Length)
                {
                    _current = _array[_index];
                    _index++;
                    return true;
                }
                _index = _array.Length + 1;
                _current = null;
                return false;
            }

            public EffectPass Current
            {
                get { return _current; }
            }

            public void Dispose()
            {

            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (_index == _array.Length + 1)
                        throw new InvalidOperationException();
                    return Current;
                }
            }

            void System.Collections.IEnumerator.Reset()
            {
                _index = 0;
                _current = null;
            }
        }
    }
}
