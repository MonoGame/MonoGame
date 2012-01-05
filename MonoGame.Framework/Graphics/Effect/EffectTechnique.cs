using System;
namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectTechnique
	{
        internal Effect _effect;
		string name;
        public EffectPassCollection Passes { get; set; }
		public EffectAnnotationCollection Annotations { get; set; }

        public EffectTechnique(Effect effect, DXEffectObject.d3dx_technique technique)
        {
            Passes = new EffectPassCollection(this);
			Annotations = new EffectAnnotationCollection();
            _effect = effect;
			
			name = technique.name;
			
			for (int i=0; i<technique.pass_count; i++) {
				Passes._passes.Add (new EffectPass(this, technique.pass_handles[i]));
			}
        }
		
		public string Name {
			get { return name; }
		}
	}
}

