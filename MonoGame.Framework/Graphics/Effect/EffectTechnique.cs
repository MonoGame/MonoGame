using System;
namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectTechnique
	{
        internal Effect _effect;

        public EffectPassCollection Passes { get; set; }

        public EffectTechnique(Effect effect)
        {
            Passes = new EffectPassCollection(this);
            _effect = effect;
        }
		
		public string Name 
		{ 
			get; 
			set;
		}
	}
}

