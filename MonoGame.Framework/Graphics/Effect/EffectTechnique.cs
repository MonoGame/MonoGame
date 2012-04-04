using System;
namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectTechnique
	{
        internal static int id = 0;
        internal Effect _effect;
		string name;
        public EffectPassCollection Passes { get; set; }
		public EffectAnnotationCollection Annotations { get; set; }

#if WINRT

#else

#if NOMOJO

        public EffectTechnique(Effect effect)
        {
            _effect = effect;
            Passes = new EffectPassCollection(this);
            Annotations = new EffectAnnotationCollection();

            name = string.Format("Technique{0}", ++id);

            //Only supports a single pass.
            Passes._passes.Add(new EffectPass(this));

            this.name = effect.Name;
        }
#endif // NOMOJO

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

        public EffectTechnique(Effect effect, GLSLEffectObject.glslTechnique technique)
        {
            Passes = new EffectPassCollection(this);
			Annotations = new EffectAnnotationCollection();
            _effect = effect;
			
			name = technique.name;
			
			for (int i=0; i<technique.pass_count; i++) {
				Passes._passes.Add (new EffectPass(this, technique.pass_handles[i]));
			}
        }
#endif
		
		public string Name {
			get { return name; }
		}
	}
}

