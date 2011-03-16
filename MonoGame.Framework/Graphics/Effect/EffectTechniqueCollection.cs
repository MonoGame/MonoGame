using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectTechniqueCollection : IEnumerable<EffectTechnique>
    {
        Dictionary<string, EffectTechnique> _techniques = new Dictionary<string, EffectTechnique>();

        public EffectTechnique this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public EffectTechnique this[string name]
        {
            get { return _techniques[name]; }
            set { _techniques[name] = value; }
        }

        public IEnumerator<EffectTechnique> GetEnumerator()
        {
            return _techniques.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _techniques.Values.GetEnumerator();
        }
    }
}
