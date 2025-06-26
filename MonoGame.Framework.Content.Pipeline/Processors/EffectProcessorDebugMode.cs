// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Specifies how debugging of effects is to be supported in PIX.
    /// </summary>
    public enum EffectProcessorDebugMode
    {
        /// <summary>
        /// Enables effect debugging when built with Debug profile.
        /// </summary>
        Auto = 0,

        /// <summary>
        /// Enables effect debugging for all profiles. Will produce unoptimized shaders.
        /// </summary>
        Debug = 1,

        /// <summary>
        /// Disables debugging for all profiles, produce optimized shaders.
        /// </summary>
        Optimize = 2,
    }
}
