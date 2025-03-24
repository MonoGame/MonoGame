// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Provides information about the capabilities of the
    /// current graphics device. A very useful thread for investigating GL extenion names
    /// http://stackoverflow.com/questions/3881197/opengl-es-2-0-extensions-on-android-devices
    /// </summary>
    internal partial class GraphicsCapabilities
    {
        internal void Initialize(GraphicsDevice device)
        {
            PlatformInitialize(device);
        }

        /// <summary>
        /// Whether the device fully supports non power-of-two textures, including
        /// mip maps and wrap modes other than CLAMP_TO_EDGE
        /// </summary>
        internal bool SupportsNonPowerOfTwo { get; private set; }

        /// <summary>
        /// Whether the device supports anisotropic texture filtering
        /// </summary>
		internal bool SupportsTextureFilterAnisotropic { get; private set; }

        internal bool SupportsDepth24 { get; private set; }

        internal bool SupportsPackedDepthStencil { get; private set; }

        internal bool SupportsDepthNonLinear { get; private set; }

        /// <summary>
        /// Gets the support for DXT1
        /// </summary>
        internal bool SupportsDxt1 { get; private set; }

        /// <summary>
        /// Gets the support for S3TC (DXT1, DXT3, DXT5)
        /// </summary>
        internal bool SupportsS3tc { get; private set; }

        /// <summary>
        /// Gets the support for PVRTC
        /// </summary>
        internal bool SupportsPvrtc { get; private set; }

        /// <summary>
        /// Gets the support for ETC1
        /// </summary>
        internal bool SupportsEtc1 { get; private set; }

        /// <summary>
        /// Gets the support for ETC2
        /// </summary>
        internal bool SupportsEtc2 { get; private set; }

        /// <summary>
        /// Gets the support for ATITC
        /// </summary>
        internal bool SupportsAtitc { get; private set; }

        internal bool SupportsTextureMaxLevel { get; private set; }

        /// <summary>
        /// True, if sRGB is supported. On Direct3D platforms, this is always <code>true</code>.
        /// On OpenGL platforms, it is <code>true</code> if both framebuffer sRGB
        /// and texture sRGB are supported.
        /// </summary>
        internal bool SupportsSRgb { get; private set; }

        internal bool SupportsTextureArrays { get; private set; }

        internal bool SupportsDepthClamp { get; private set; }

        internal bool SupportsVertexTextures { get; private set; }

        /// <summary>
        /// True, if the underlying platform supports floating point textures. 
        /// For Direct3D platforms this is always <code>true</code>.
        /// For OpenGL Desktop platforms it is always <code>true</code>.
        /// For OpenGL Mobile platforms it requires `GL_EXT_color_buffer_float`.
        /// If the requested format is not supported an <code>NotSupportedException</code>
        /// will be thrown.
        /// </summary>
        internal bool SupportsFloatTextures { get; private set; }

        /// <summary>
        /// True, if the underlying platform supports half floating point textures. 
        /// For Direct3D platforms this is always <code>true</code>.
        /// For OpenGL Desktop platforms it is always <code>true</code>.
        /// For OpenGL Mobile platforms it requires `GL_EXT_color_buffer_half_float`.
        /// If the requested format is not supported an <code>NotSupportedException</code>
        /// will be thrown.
        /// </summary>
        internal bool SupportsHalfFloatTextures { get; private set; }

        internal bool SupportsNormalized { get; private set; }

        /// <summary>
        /// Gets the max texture anisotropy. This value typically lies
        /// between 0 and 16, where 0 means anisotropic filtering is not
        /// supported.
        /// </summary>
        internal int MaxTextureAnisotropy { get; private set; }

        // The highest possible MSCount
        private const int MultiSampleCountLimit = 32;

        private int _maxMultiSampleCount;

        internal int MaxMultiSampleCount
        {
            get { return _maxMultiSampleCount; }
        }

        internal bool SupportsInstancing { get; private set; }

        internal bool SupportsBaseIndexInstancing { get; private set; }

        internal bool SupportsSeparateBlendStates { get; private set; }
    }
}
