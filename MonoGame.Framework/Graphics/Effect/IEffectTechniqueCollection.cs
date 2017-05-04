using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public interface IEffectTechniqueCollection
    {
        IEffectTechnique this[int index] { get; }

        IEffectTechnique this[string name] { get; }

        IEnumerable<IEffectTechnique> Interfaces { get; }
    }
}