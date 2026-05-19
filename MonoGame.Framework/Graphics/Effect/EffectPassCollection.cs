using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a collection of <see cref="EffectPass"/> objects.
    /// </summary>
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

        /// <summary>
        /// Retrieves the <see cref="EffectPass"/> at the specified index in the collection.
        /// </summary>
        public EffectPass this[int index]
        {
            get { return _passes[index]; }
        }

        /// <summary>
        /// Retrieves a <see cref="EffectPass"/> from the collection, given the name of the pass.
        /// </summary>
        /// <param name="name">The name of the pass to retrieve.</param>
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

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count
        {
            get { return _passes.Length; }
        }

        /// <summary>
        /// Returns a <see cref="EffectPassCollection.Enumerator">EffectPassCollection.Enumerator</see>
        /// that can iterate through a collection.
        /// </summary>
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

        /// <summary>
        /// Enumerator to iterate through the <see cref="EffectPassCollection"/>
        /// </summary>
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

            /// <inheritdoc/>
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

            /// <inheritdoc/>
            public EffectPass Current
            {
                get { return _current; }
            }

            /// <inheritdoc cref="IDisposable.Dispose()"/>
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
