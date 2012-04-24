using System;
namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectTechnique
	{
        private Effect _effect;

        public EffectPassCollection Passes { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }

        public string Name { get; private set; }

        internal EffectTechnique(Effect effect, string name, EffectPassCollection passes, EffectAnnotationCollection annotations)
        {
            _effect = effect;
            Name = name;
            Passes = passes;
            Annotations = annotations;
        }

        public EffectTechnique(Effect effect, DXEffectObject.d3dx_technique technique)
            : this( effect, technique.name, new EffectPassCollection(), new EffectAnnotationCollection() )
        {
            // TODO: Set the annotations from the effect object!

            for (int i = 0; i < technique.pass_count; i++)
                Passes.Add(new EffectPass(effect, technique.pass_handles[i]));
        }

    }


}

