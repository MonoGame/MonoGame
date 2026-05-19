using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Represents a collection of <see cref="EffectParameter"/> objects.
    /// </summary>
    public class EffectParameterCollection : IEnumerable<EffectParameter>
    {
        internal static readonly EffectParameterCollection Empty = new EffectParameterCollection(new EffectParameter[0]);

        private readonly EffectParameter[] _parameters;
        private readonly Dictionary<string, int> _indexLookup;

        internal EffectParameterCollection(EffectParameter[] parameters)
        {
            _parameters = parameters;
            _indexLookup = new Dictionary<string, int>(_parameters.Length);
            for (int i = 0; i < _parameters.Length; i++)
            {
                string name = _parameters[i].Name;
                if(!string.IsNullOrWhiteSpace(name))
                    _indexLookup.Add(name, i);
            }
        }

        private EffectParameterCollection(EffectParameter[] parameters, Dictionary<string, int> indexLookup)
        {
            _parameters = parameters;
            _indexLookup = indexLookup;
        }

        internal EffectParameterCollection Clone()
        {
            if (_parameters.Length == 0)
                return Empty;

            var parameters = new EffectParameter[_parameters.Length];
            for (var i = 0; i < _parameters.Length; i++)
                parameters[i] = new EffectParameter(_parameters[i]);

            return new EffectParameterCollection(parameters, _indexLookup);
        }

        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        public int Count
        {
            get { return _parameters.Length; }
        }

        /// <summary>
        /// Retrieves the <see cref="EffectParameter"/> at the specified index in the collection.
        /// </summary>
        public EffectParameter this[int index]
		{
			get { return _parameters[index]; }
		}

        /// <summary>
        /// Retrieves a <see cref="EffectParameter"/> from the collection, given the name of the parameter.
        /// </summary>
        /// <param name="name">The name of the parameter to retrieve.</param>
        public EffectParameter this[string name]
        {
            get
            {
                int index;
                if (_indexLookup.TryGetValue(name, out index))
                    return _parameters[index];
                return null;
			}
        }

        /// <inheritdoc/>
        public IEnumerator<EffectParameter> GetEnumerator()
        {
            return ((IEnumerable<EffectParameter>)_parameters).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }
    }
}
