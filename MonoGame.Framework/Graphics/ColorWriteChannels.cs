// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Defines the color channels for render target blending operations.
    /// </summary>
	[Flags]
	public enum ColorWriteChannels
	{
        /// <summary>
        /// No channels selected.
        /// </summary>
		None = 0,
        /// <summary>
        /// Red channel selected.
        /// </summary>
		Red = 1,
        /// <summary>
        /// Green channel selected.
        /// </summary>
		Green = 2,
        /// <summary>
        /// Blue channel selected.
        /// </summary>
		Blue = 4,
        /// <summary>
        /// Alpha channel selected.
        /// </summary>
		Alpha = 8,
        /// <summary>
        /// All channels selected.
        /// </summary>
		All = 15
	}
}