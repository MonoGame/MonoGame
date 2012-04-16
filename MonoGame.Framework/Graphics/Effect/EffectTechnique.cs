using System;
namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectTechnique
	{
        internal static int id = 0;
        internal Effect _effect;

        public EffectPassCollection Passes { get; set; }
		public EffectAnnotationCollection Annotations { get; set; }

        public string Name { get; private set; }

#if NOMOJO

        public EffectTechnique(Effect effect)
        {
            _effect = effect;
            Passes = new EffectPassCollection(this);
            Annotations = new EffectAnnotationCollection();

            Name = string.Format("{0}.Technique{1}", effect.Name, ++id);
            
            Passes._passes.Add(new EffectPass(this));
        }

#else

        public EffectTechnique(Effect effect, DXEffectObject.d3dx_technique technique)
        {
            Passes = new EffectPassCollection(this);
			Annotations = new EffectAnnotationCollection();
            _effect = effect;
			
			Name = technique.name;
			
			for (int i=0; i<technique.pass_count; i++) {
				Passes._passes.Add (new EffectPass(this, technique.pass_handles[i]));
			}
        }

        public EffectTechnique(Effect effect, GLSLEffectObject.glslTechnique technique)
        {
            Passes = new EffectPassCollection(this);
			Annotations = new EffectAnnotationCollection();
            _effect = effect;
			
			Name = technique.name;
			
			for (int i=0; i<technique.pass_count; i++) {
				Passes._passes.Add (new EffectPass(this, technique.pass_handles[i]));
			}
        }
		
#endif

    }


}

