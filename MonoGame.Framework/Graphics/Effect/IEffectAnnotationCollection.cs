using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public interface IEffectAnnotationCollection
    {
        int Count { get; }

        IEffectAnnotation this[int index] { get; }

        IEffectAnnotation this[string name] { get; }

        IEnumerable<IEffectAnnotation> Interfaces { get; }
    }
}