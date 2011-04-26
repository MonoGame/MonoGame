using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoMac.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectPass
    {
        EffectTechnique _technique = null;

        public void Apply()
        {
            _technique._effect.Apply();
        }

        public EffectPass(EffectTechnique technique)
        {
            _technique = technique;
        }
		
		public string Name { get; set; }
    }
}
