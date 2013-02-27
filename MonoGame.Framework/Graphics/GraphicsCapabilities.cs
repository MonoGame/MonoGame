#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright � 2009 The MonoGame Team

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
        internal static bool NonPowerOfTwo { get; private set; }

        /// <summary>
        /// Whether the device supports anisotropic texture filtering
        /// </summary>
        internal static bool TextureFilterAnisotric { get; private set; }

        internal static void Initialize(GraphicsDevice device)
        {
            NonPowerOfTwo = GetNonPowerOfTwo(device);

#if GLES
            TextureFilterAnisotric = device._extensions.Contains("GL_EXT_texture_filter_anisotropic");
#else
            TextureFilterAnisotric = true;
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