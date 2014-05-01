#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

using System;
using System.Collections.Generic;
#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
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
    internal static class GraphicsCapabilities
    {
        /// <summary>
        /// Whether the device fully supports non power-of-two textures, including
        /// mip maps and wrap modes other than CLAMP_TO_EDGE
        /// </summary>
        internal static bool SupportsNonPowerOfTwo { get; private set; }

        /// <summary>
        /// Whether the device supports anisotropic texture filtering
        /// </summary>
		internal static bool SupportsTextureFilterAnisotropic { get; private set; }

		internal static bool SupportsDepth24 { get; private set; }

		internal static bool SupportsPackedDepthStencil { get; private set; }

		internal static bool SupportsDepthNonLinear { get; private set; }

        /// <summary>
        /// Gets the support for DXT1
        /// </summary>
        internal static bool SupportsDxt1 { get; private set; }

        /// <summary>
        /// Gets the support for S3TC (DXT1, DXT3, DXT5)
        /// </summary>
        internal static bool SupportsS3tc { get; private set; }

        /// <summary>
        /// Gets the support for PVRTC
        /// </summary>
        internal static bool SupportsPvrtc { get; private set; }

        /// <summary>
        /// Gets the support for ETC1
        /// </summary>
        internal static bool SupportsEtc1 { get; private set; }

        /// <summary>
        /// Gets the support for ATITC
        /// </summary>
        internal static bool SupportsAtitc { get; private set; }

#if OPENGL
        /// <summary>
        /// True, if GL_ARB_framebuffer_object is supported; false otherwise.
        /// </summary>
        internal static bool SupportsFramebufferObjectARB { get; private set; }

        /// <summary>
        /// True, if GL_EXT_framebuffer_object is supported; false otherwise.
        /// </summary>
        internal static bool SupportsFramebufferObjectEXT { get; private set; }

        /// <summary>
        /// Gets the max texture anisotropy. This value typically lies
        /// between 0 and 16, where 0 means anisotropic filtering is not
        /// supported.
        /// </summary>
        internal static int MaxTextureAnisotropy { get; private set; }
#endif

        internal static void Initialize(GraphicsDevice device)
        {
			SupportsNonPowerOfTwo = GetNonPowerOfTwo(device);

#if GLES
			SupportsTextureFilterAnisotropic = device._extensions.Contains("GL_EXT_texture_filter_anisotropic");
			SupportsDepth24 = device._extensions.Contains("GL_OES_depth24");
			SupportsPackedDepthStencil = device._extensions.Contains("GL_OES_packed_depth_stencil");
			SupportsDepthNonLinear = device._extensions.Contains("GL_NV_depth_nonlinear");
#else
			SupportsTextureFilterAnisotropic = true;
			SupportsDepth24 = true;
			SupportsPackedDepthStencil = true;
			SupportsDepthNonLinear = false;
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
#if GLES && !ANGLE
            if (GraphicsCapabilities.SupportsTextureFilterAnisotropic)
            {
                GL.GetInteger(All.MaxTextureMaxAnisotropyExt, ref anisotropy);
            }
#else
            GL.GetInteger((GetPName)All.MaxTextureMaxAnisotropyExt, out anisotropy);
#endif
            GraphicsExtensions.CheckGLError();
            MaxTextureAnisotropy = anisotropy;
#endif
        }

        static bool GetNonPowerOfTwo(GraphicsDevice device)
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