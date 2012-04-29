using System;
namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectTechnique
	{
        internal Effect _effect;

        public EffectPassCollection Passes { get; set; }
		public EffectAnnotationCollection Annotations { get; set; }

        public string Name { get; private set; }

        public EffectTechnique(Effect effect, DXEffectObject.d3dx_technique technique)
        {
            _effect = effect;

            Name = technique.name;

            // TODO: Set the annotations from the effect object!
            Annotations = new EffectAnnotationCollection();

            Passes = new EffectPassCollection(this);
            for (int i = 0; i < technique.pass_count; i++)
				Passes._passes.Add (new EffectPass(this, technique.pass_handles[i]));
        }

    }


}

