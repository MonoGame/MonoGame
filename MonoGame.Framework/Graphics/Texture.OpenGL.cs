// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if MONOMAC && PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
#endif
#if (MONOMAC && !PLATFORM_MACOS_LEGACY)
using OpenTK.Graphics.OpenGL;
#endif
#if DESKTOPGL
using OpenGL;
#endif
#if GLES
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {
        internal int glTexture = -1;
        internal TextureTarget glTarget;
        internal TextureUnit glTextureUnit = TextureUnit.Texture0;
        internal PixelInternalFormat glInternalFormat;
        internal PixelFormat glFormat;
        internal PixelType glType;
        internal SamplerState glLastSamplerState;

        private void PlatformGraphicsDeviceResetting()
        {
            DeleteGLTexture();
            glLastSamplerState = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                DeleteGLTexture();
                glLastSamplerState = null;
            }

            base.Dispose(disposing);
        }

        private void DeleteGLTexture()
        {
            if (glTexture > 0)
            {
                GraphicsDevice.DisposeTexture(glTexture);
            }
            glTexture = -1;
        }
    }
}

