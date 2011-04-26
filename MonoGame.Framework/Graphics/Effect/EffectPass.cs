using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.ES20;
using OpenTK.Graphics.ES11;
using GL11 = OpenTK.Graphics.ES11.GL;
using GL20 = OpenTK.Graphics.ES20.GL;
using All11 = OpenTK.Graphics.ES11.All;
using All20 = OpenTK.Graphics.ES20.All;

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
