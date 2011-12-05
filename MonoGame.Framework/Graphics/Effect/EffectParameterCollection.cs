using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectParameterCollection : IEnumerable<EffectParameter>
    {
        internal List<EffectParameter> _parameters = new List<EffectParameter>();

        public int Count
        {
            get { return _parameters.Count; }
        }
		
		public EffectParameter this[int index]
		{
			get { return _parameters[index]; }
		}
		
		public EffectParameter this[string name]
        {
            get {
				foreach (EffectParameter parameter in _parameters) {
					if (parameter.Name == name) {
						return parameter;
					}
				}
				return null;
			}
        }

        public IEnumerator<EffectParameter> GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }
    }
}
