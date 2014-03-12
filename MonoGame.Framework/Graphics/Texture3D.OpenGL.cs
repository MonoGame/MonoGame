// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Texture3D : Texture
	{
#if !GLES
		PixelInternalFormat glInternalFormat;
		PixelFormat glFormat;
		PixelType glType;
#endif

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
#if GLES
            throw new NotSupportedException("OpenGL ES 2.0 doesn't support 3D textures.");
#else
            this.glTarget = TextureTarget.Texture3D;

            GL.GenTextures(1, out this.glTexture);
            GraphicsExtensions.CheckGLError();

            GL.BindTexture(glTarget, glTexture);
            GraphicsExtensions.CheckGLError();

            format.GetGLFormat(out glInternalFormat, out glFormat, out glType);

            GL.TexImage3D(glTarget, 0, glInternalFormat, width, height, depth, 0, glFormat, glType, IntPtr.Zero);
            GraphicsExtensions.CheckGLError();

            if (mipMap)
                throw new NotImplementedException("Texture3D does not yet support mipmaps.");
#endif
        }

        private void PlatformSetData(int level,
                                     int left, int top, int right, int bottom, int front, int back,
                                     IntPtr dataPtr, int width, int height, int depth)
        {
#if GLES
            throw new NotSupportedException("OpenGL ES 2.0 doesn't support 3D textures.");
#else
            GL.BindTexture(glTarget, glTexture);
            GraphicsExtensions.CheckGLError();
            GL.TexSubImage3D(glTarget, level, left, top, front, width, height, depth, glFormat, glType, dataPtr);
            GraphicsExtensions.CheckGLError();
#endif
        }

        private void PlatformGetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount)
             where T : struct
        {

            throw new NotImplementedException();
        }
	}
}

