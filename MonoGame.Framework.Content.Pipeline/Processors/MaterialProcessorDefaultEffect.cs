// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Specifies the default effect type.
    /// </summary>
    public enum MaterialProcessorDefaultEffect
    {
        /// <summary>
        /// A BasicEffect Class effect.
        /// </summary>
        BasicEffect = 0,

        /// <summary>
        /// A SkinnedEffect Class effect.
        /// </summary>
        SkinnedEffect = 1,

        /// <summary>
        /// An EnvironmentMapEffect Class effect.
        /// </summary>
        EnvironmentMapEffect = 2,

        /// <summary>
        /// A DualTextureEffect Class effect.
        /// </summary>
        DualTextureEffect = 3,

        /// <summary>
        /// An AlphaTestEffect Class effect.
        /// </summary>
        AlphaTestEffect = 4,
    }
}
