using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public interface IEffectParameterCollection
    {
        IEffectParameter this[int index] { get; }

        IEffectParameter this[string name] { get; }

        IEnumerable<IEffectParameter> Interfaces { get; }
    }
}