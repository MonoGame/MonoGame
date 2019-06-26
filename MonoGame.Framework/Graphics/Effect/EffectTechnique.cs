using System;
namespace Microsoft.Xna.Framework.Graphics
{
	public class EffectTechnique
	{
        public EffectPassCollection Passes { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }

        public string Name { get; private set; }

        internal EffectTechnique(Effect effect, EffectTechnique cloneSource)
        {
            // Share all the immutable types.
            Name = cloneSource.Name;
            Annotations = cloneSource.Annotations;

            // Clone the mutable types.
            Passes = cloneSource.Passes.Clone(effect);
        }

        internal EffectTechnique(Effect effect, string name, EffectPassCollection passes, EffectAnnotationCollection annotations)
        {
            Name = name;
            Passes = passes;
            Annotations = annotations;
        }
    }


}

