using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public interface IEffect : IDisposable
    {
        IEffectParameterCollection Parameters { get; }

        IEffectTechniqueCollection Techniques { get; }

        IEffectTechnique CurrentTechnique { get; }
    }
}
