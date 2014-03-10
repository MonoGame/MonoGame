// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
using TextureTarget = OpenTK.Graphics.ES20.All;
using TextureUnit = OpenTK.Graphics.ES20.All;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {
        internal int glTexture = -1;
        internal TextureTarget glTarget;
        internal TextureUnit glTextureUnit = TextureUnit.Texture0;
        internal SamplerState glLastSamplerState = null;
        private void PlatformGraphicsDeviceResetting()
        {
            this.glTexture = -1;
            this.glLastSamplerState = null;
        }

        private void PlatformDispose(bool disposing)
        {
            GraphicsDevice.AddDisposeAction(() =>
            {
                GL.DeleteTextures(1, ref glTexture);
                GraphicsExtensions.CheckGLError();
                glTexture = -1;
            });

            glLastSamplerState = null;
        }
    }
}

