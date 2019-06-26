// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
	/// <summary>
	/// Defines formats for depth-stencil buffer.
	/// </summary>
    public enum DepthFormat
    {
        /// <summary>
        /// Depth-stencil buffer will not be created.
        /// </summary>
		None,
        /// <summary>
        /// 16-bit depth buffer.
        /// </summary>
		Depth16,
        /// <summary>
        /// 24-bit depth buffer. Equivalent of <see cref="DepthFormat.Depth24Stencil8"/> for DirectX platforms.
        /// </summary>
		Depth24,
        /// <summary>
        /// 32-bit depth-stencil buffer. Where 24-bit depth and 8-bit for stencil used.
        /// </summary>
		Depth24Stencil8
    }
}