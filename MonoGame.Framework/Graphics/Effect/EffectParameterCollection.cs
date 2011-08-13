using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectParameterCollection : IEnumerable<EffectParameter>
    {
        internal Dictionary<string, EffectParameter> _parameters = new Dictionary<string, EffectParameter>();

        public EffectParameter this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public int Count
        {
            get { return _parameters.Count; }
        }

        public EffectParameter this[string name]
        {
            get { return _parameters[name]; }
            internal set { _parameters[name] = value; }
        }

        public IEnumerator<EffectParameter> GetEnumerator()
        {
            return _parameters.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _parameters.Values.GetEnumerator();
        }
    }
}
