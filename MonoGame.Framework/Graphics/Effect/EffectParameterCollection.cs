using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectParameterCollection : IEnumerable<EffectParameter>
    {
        internal static readonly EffectParameterCollection Empty = new EffectParameterCollection(new EffectParameter[0]);

        private readonly EffectParameter[] _parameters;
        private readonly Dictionary<string, int> _lookupDictionary = new Dictionary<string, int>();

        internal EffectParameterCollection(EffectParameter[] parameters)
        {
            _parameters = parameters;
            for (int i = 0; i < _parameters.Length; i++)
            {
                _lookupDictionary.Add(_parameters[i].Name, i);
            }
        }

        private EffectParameterCollection(EffectParameter[] parameters, Dictionary<string, int> lookupDictionary)
        {
            _parameters = parameters;
            _lookupDictionary = lookupDictionary;
        }

        internal EffectParameterCollection Clone()
        {
            if (_parameters.Length == 0)
                return Empty;

            var parameters = new EffectParameter[_parameters.Length];
            for (var i = 0; i < _parameters.Length; i++)
                parameters[i] = new EffectParameter(_parameters[i]);

            return new EffectParameterCollection(parameters, _lookupDictionary);
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
                if (_lookupDictionary.TryGetValue(name, out int i))
                    return _parameters[i];
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
