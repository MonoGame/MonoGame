using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public interface IEffectPassCollection
    {
        IEffectPass this[int index] { get; }

        IEffectPass this[string name] { get; }

        int Count { get; }

        IEnumerable<IEffectPass> Interfaces { get; }
    }
}