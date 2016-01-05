// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
#if OPENGL
#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
#else
using OpenTK.Graphics.OpenGL;
#endif
#elif GLES
using OpenTK.Graphics.ES20;
#else
using OpenTK.Graphics.OpenGL;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Provides information about the capabilities of the
    /// current graphics device. A very useful thread for investigating GL extenion names
    /// http://stackoverflow.com/questions/3881197/opengl-es-2-0-extensions-on-android-devices
    /// </summary>
    internal class GraphicsCapabilities
    {
        public GraphicsCapabilities(GraphicsDevice graphicsDevice)
        {
            Initialize(graphicsDevice);
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
        /// Gets the support for ATITC
        /// </summary>
        internal bool SupportsAtitc { get; private set; }

#if OPENGL
        /// <summary>
        /// True, if GL_ARB_framebuffer_object is supported; false otherwise.
        /// </summary>
        internal bool SupportsFramebufferObjectARB { get; private set; }

        /// <summary>
        /// True, if GL_EXT_framebuffer_object is supported; false otherwise.
        /// </summary>
        internal bool SupportsFramebufferObjectEXT { get; private set; }

        /// <summary>
        /// Gets the max texture anisotropy. This value typically lies
        /// between 0 and 16, where 0 means anisotropic filtering is not
        /// supported.
        /// </summary>
        internal int MaxTextureAnisotropy { get; private set; }
#endif

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

        internal void Initialize(GraphicsDevice device)
        {
			SupportsNonPowerOfTwo = GetNonPowerOfTwo(device);

#if OPENGL
            SupportsTextureFilterAnisotropic = device._extensions.Contains("GL_EXT_texture_filter_anisotropic");
#else
            SupportsTextureFilterAnisotropic = true;
#endif
#if GLES
			SupportsDepth24 = device._extensions.Contains("GL_OES_depth24");
			SupportsPackedDepthStencil = device._extensions.Contains("GL_OES_packed_depth_stencil");
			SupportsDepthNonLinear = device._extensions.Contains("GL_NV_depth_nonlinear");
            SupportsTextureMaxLevel = device._extensions.Contains("GL_APPLE_texture_max_level");
#else
            SupportsDepth24 = true;
			SupportsPackedDepthStencil = true;
			SupportsDepthNonLinear = false;
            SupportsTextureMaxLevel = true;
#endif

            // Texture compression
#if DIRECTX
            SupportsDxt1 = true;
            SupportsS3tc = true;
#elif OPENGL
            SupportsS3tc = device._extensions.Contains("GL_EXT_texture_compression_s3tc") ||
                device._extensions.Contains("GL_OES_texture_compression_S3TC") ||
                device._extensions.Contains("GL_EXT_texture_compression_dxt3") ||
                device._extensions.Contains("GL_EXT_texture_compression_dxt5");
            SupportsDxt1 = SupportsS3tc || device._extensions.Contains("GL_EXT_texture_compression_dxt1");
            SupportsPvrtc = device._extensions.Contains("GL_IMG_texture_compression_pvrtc");
            SupportsEtc1 = device._extensions.Contains("GL_OES_compressed_ETC1_RGB8_texture");
            SupportsAtitc = device._extensions.Contains("GL_ATI_texture_compression_atitc") ||
                device._extensions.Contains("GL_AMD_compressed_ATC_texture");
#endif

            // OpenGL framebuffer objects
#if OPENGL
#if GLES
            SupportsFramebufferObjectARB = true; // always supported on GLES 2.0+
            SupportsFramebufferObjectEXT = false;
#else
            SupportsFramebufferObjectARB = device._extensions.Contains("GL_ARB_framebuffer_object");
            SupportsFramebufferObjectEXT = device._extensions.Contains("GL_EXT_framebuffer_object");
#endif
#endif

            // Anisotropic filtering
#if OPENGL
            int anisotropy = 0;
            if (SupportsTextureFilterAnisotropic)
            {
                GL.GetInteger((GetPName)All.MaxTextureMaxAnisotropyExt, out anisotropy);
                GraphicsExtensions.CheckGLError();
            }
            MaxTextureAnisotropy = anisotropy;
#endif

            // sRGB
#if DIRECTX
            SupportsSRgb = true;
#elif OPENGL
#if GLES
            SupportsSRgb = device._extensions.Contains("GL_EXT_sRGB");
#else
            SupportsSRgb = device._extensions.Contains("GL_EXT_texture_sRGB") && device._extensions.Contains("GL_EXT_framebuffer_sRGB");
#endif
#endif

#if DIRECTX
            SupportsTextureArrays = device.GraphicsProfile == GraphicsProfile.HiDef;
#elif OPENGL
            // TODO: Implement OpenGL support for texture arrays
            // once we can author shaders that use texture arrays.
            SupportsTextureArrays = false;
#endif

#if DIRECTX
            SupportsDepthClamp = device.GraphicsProfile == GraphicsProfile.HiDef;
#elif OPENGL
            SupportsDepthClamp = device._extensions.Contains("GL_ARB_depth_clamp");
#endif

#if DIRECTX
            SupportsVertexTextures = device.GraphicsProfile == GraphicsProfile.HiDef;
#elif OPENGL
            SupportsVertexTextures = false; // For now, until we implement vertex textures in OpenGL.
#endif
        }

        bool GetNonPowerOfTwo(GraphicsDevice device)
        {
#if OPENGL
#if GLES
            return device._extensions.Contains("GL_OES_texture_npot") ||
                   device._extensions.Contains("GL_ARB_texture_non_power_of_two") ||
                   device._extensions.Contains("GL_IMG_texture_npot") ||
                   device._extensions.Contains("GL_NV_texture_npot_2D_mipmap");
#else
            // Unfortunately non PoT texture support is patchy even on desktop systems and we can't
            // rely on the fact that GL2.0+ supposedly supports npot in the core.
            // Reference: http://aras-p.info/blog/2012/10/17/non-power-of-two-textures/
            return device._maxTextureSize >= 8192;
#endif

#else
            return device.GraphicsProfile == GraphicsProfile.HiDef;
#endif
        }
    }
}