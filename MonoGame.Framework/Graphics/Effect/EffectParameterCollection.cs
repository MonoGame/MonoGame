using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
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

        public int Count
        {
            get { return _parameters.Length; }
        }
		
		public EffectParameter this[int index]
		{
			get { return _parameters[index]; }
		}
		
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
