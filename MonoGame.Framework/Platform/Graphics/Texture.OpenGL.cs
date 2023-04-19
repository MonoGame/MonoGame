// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using MonoGame.OpenGL;

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

        private void PlatformGraphicsDeviceResetting()
        {
            DeleteGLTexture();
        }

        internal override void PlatformApply(GraphicsDevice device, ShaderProgram program, ref ResourceBinding resourceBinding, bool writeAcess)
        {
            if (glTexture < 0)
                throw new InvalidOperationException("No valid texture");

            var bufferAccess = ShaderAccess == ShaderAccess.ReadWrite ? BufferAccess.ReadWrite : BufferAccess.ReadOnly;

            GL.BindImageTexture(resourceBinding.bindingSlot, glTexture, 0, true, 0, bufferAccess, glInternalFormat);
            GraphicsExtensions.CheckGLError();
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                DeleteGLTexture();
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

