using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectPassCollection : IEnumerable<EffectPass>
    {
        Dictionary<string, EffectPass> _passes = new Dictionary<string, EffectPass>();
        private EffectTechnique _effectTechnique;

        public EffectPassCollection(EffectTechnique effectTechnique)
        {
            _effectTechnique = effectTechnique;
        }

        public EffectPass this[int index]
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public EffectPass this[string name]
        {
            get { return _passes[name]; }
            set { _passes[name] = value; }
        }

        public int Count
        {
            get { return _passes.Count; }
        }

        public IEnumerator<EffectPass> GetEnumerator()
        {
            return _passes.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _passes.Values.GetEnumerator();
        }
    }
}
