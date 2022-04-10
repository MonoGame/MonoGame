// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if OPENGL
using MonoGame.OpenGL;
using GetParamName = MonoGame.OpenGL.GetPName;
#endif

namespace Microsoft.Xna.Framework.Graphics
{

    internal partial class GraphicsCapabilities
    {
        /// <summary>
        /// True, if GL_ARB_framebuffer_object is supported; false otherwise.
        /// </summary>
        internal bool SupportsFramebufferObjectARB { get; private set; }

        /// <summary>
        /// True, if GL_EXT_framebuffer_object is supported; false otherwise.
        /// </summary>
        internal bool SupportsFramebufferObjectEXT { get; private set; }

        /// <summary>
        /// True, if GL_IMG_multisampled_render_to_texture is supported; false otherwise.
        /// </summary>
        internal bool SupportsFramebufferObjectIMG { get; private set; }


        private void PlatformInitialize(GraphicsDevice device)
        {
            var extensions = GL.GetExtensions();
#if GLES
            SupportsNonPowerOfTwo = extensions.Contains("GL_OES_texture_npot") ||
                extensions.Contains("GL_ARB_texture_non_power_of_two") ||
                extensions.Contains("GL_IMG_texture_npot") ||
                extensions.Contains("GL_NV_texture_npot_2D_mipmap");
#else
            // Unfortunately non PoT texture support is patchy even on desktop systems and we can't
            // rely on the fact that GL2.0+ supposedly supports npot in the core.
            // Reference: http://aras-p.info/blog/2012/10/17/non-power-of-two-textures/
            SupportsNonPowerOfTwo = device._maxTextureSize >= 8192;
#endif

            SupportsTextureFilterAnisotropic = extensions.Contains("GL_EXT_texture_filter_anisotropic");

#if GLES
			SupportsDepth24 = extensions.Contains("GL_OES_depth24");
			SupportsPackedDepthStencil = extensions.Contains("GL_OES_packed_depth_stencil");
			SupportsDepthNonLinear = extensions.Contains("GL_NV_depth_nonlinear");
            SupportsTextureMaxLevel = extensions.Contains("GL_APPLE_texture_max_level");
#else
            SupportsDepth24 = true;
            SupportsPackedDepthStencil = true;
            SupportsDepthNonLinear = false;
            SupportsTextureMaxLevel = true;
#endif
            // Texture compression
            SupportsS3tc = extensions.Contains("GL_EXT_texture_compression_s3tc") ||
                           extensions.Contains("GL_OES_texture_compression_S3TC") ||
                           extensions.Contains("GL_EXT_texture_compression_dxt3") ||
                           extensions.Contains("GL_EXT_texture_compression_dxt5");
            SupportsDxt1 = SupportsS3tc || extensions.Contains("GL_EXT_texture_compression_dxt1");
            SupportsPvrtc = extensions.Contains("GL_IMG_texture_compression_pvrtc");
            SupportsEtc1 = extensions.Contains("GL_OES_compressed_ETC1_RGB8_texture");
            SupportsAtitc = extensions.Contains("GL_ATI_texture_compression_atitc") ||
                            extensions.Contains("GL_AMD_compressed_ATC_texture");

            if (GL.BoundApi == GL.RenderApi.ES)
            {
                SupportsEtc2 = device.glMajorVersion >= 3;
            }


            // Framebuffer objects
#if GLES
            SupportsFramebufferObjectARB = GL.BoundApi == GL.RenderApi.ES && (device.glMajorVersion >= 2 || extensions.Contains("GL_ARB_framebuffer_object")); // always supported on GLES 2.0+
            SupportsFramebufferObjectEXT = extensions.Contains("GL_EXT_framebuffer_object");;
            SupportsFramebufferObjectIMG = extensions.Contains("GL_IMG_multisampled_render_to_texture") |
                                                 extensions.Contains("GL_APPLE_framebuffer_multisample") |
                                                 extensions.Contains("GL_EXT_multisampled_render_to_texture") |
                                                 extensions.Contains("GL_NV_framebuffer_multisample");
#else
            // if we're on GL 3.0+, frame buffer extensions are guaranteed to be present, but extensions may be missing
            // it is then safe to assume that GL_ARB_framebuffer_object is present so that the standard function are loaded
            SupportsFramebufferObjectARB = device.glMajorVersion >= 3 || extensions.Contains("GL_ARB_framebuffer_object");
            SupportsFramebufferObjectEXT = extensions.Contains("GL_EXT_framebuffer_object");
#endif
            // Anisotropic filtering
            int anisotropy = 0;
            if (SupportsTextureFilterAnisotropic)
            {
                GL.GetInteger((GetPName)GetParamName.MaxTextureMaxAnisotropyExt, out anisotropy);
                GraphicsExtensions.CheckGLError();
            }
            MaxTextureAnisotropy = anisotropy;

            // sRGB
#if GLES
            SupportsSRgb = extensions.Contains("GL_EXT_sRGB");
            SupportsFloatTextures = GL.BoundApi == GL.RenderApi.ES && (device.glMajorVersion >= 3 || extensions.Contains("GL_EXT_color_buffer_float"));
            SupportsHalfFloatTextures = GL.BoundApi == GL.RenderApi.ES && (device.glMajorVersion >= 3 || extensions.Contains("GL_EXT_color_buffer_half_float"));
            SupportsNormalized = GL.BoundApi == GL.RenderApi.ES && (device.glMajorVersion >= 3 && extensions.Contains("GL_EXT_texture_norm16"));
#else
            SupportsSRgb = extensions.Contains("GL_EXT_texture_sRGB") && extensions.Contains("GL_EXT_framebuffer_sRGB");
            SupportsFloatTextures = GL.BoundApi == GL.RenderApi.GL && (device.glMajorVersion >= 3 || extensions.Contains("GL_ARB_texture_float"));
            SupportsHalfFloatTextures = GL.BoundApi == GL.RenderApi.GL && (device.glMajorVersion >= 3 || extensions.Contains("GL_ARB_half_float_pixel"));;
            SupportsNormalized = GL.BoundApi == GL.RenderApi.GL && (device.glMajorVersion >= 3 || extensions.Contains("GL_EXT_texture_norm16"));;
#endif

            // TODO: Implement OpenGL support for texture arrays
            // once we can author shaders that use texture arrays.
            SupportsTextureArrays = false;

            SupportsDepthClamp = extensions.Contains("GL_ARB_depth_clamp");

            SupportsVertexTextures = false; // For now, until we implement vertex textures in OpenGL.


            GL.GetInteger((GetPName)GetParamName.MaxSamples, out _maxMultiSampleCount);

            SupportsInstancing = GL.VertexAttribDivisor != null;

            SupportsBaseIndexInstancing = GL.DrawElementsInstancedBaseInstance != null;

#if GLES
            SupportsSeparateBlendStates = false;
#else
            SupportsSeparateBlendStates = device.glMajorVersion >= 4 || extensions.Contains("GL_ARB_draw_buffers_blend");
#endif
        }

    }
}
