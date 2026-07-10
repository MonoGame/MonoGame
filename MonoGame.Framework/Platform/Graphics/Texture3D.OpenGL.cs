// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture3D : Texture
    {

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, int depth, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
#if GLES
            throw new NotSupportedException("OpenGL ES 2.0 doesn't support 3D textures.");
#else
            this.glTarget = TextureTarget.Texture3D;

            Threading.BlockOnUIThread(() =>
            {
                GL.GenTextures(1, out this.glTexture);
                GraphicsExtensions.CheckGLError();

                GL.BindTexture(glTarget, glTexture);
                GraphicsExtensions.CheckGLError();

                format.GetGLFormat(GraphicsDevice, out glInternalFormat, out glFormat, out glType);

                GL.TexImage3D(glTarget, 0, glInternalFormat, width, height, depth, 0, glFormat, glType, IntPtr.Zero);
                GraphicsExtensions.CheckGLError();
            });

            if (mipMap)
                throw new NotImplementedException("Texture3D does not yet support mipmaps.");
#endif
        }

        private void PlatformSetData<T>(
            int level,
            int left, int top, int right, int bottom, int front, int back,
            T[] data, int startIndex, int elementCount)
        {
#if GLES
            throw new NotSupportedException("OpenGL ES 2.0 doesn't support 3D textures.");
#else
            var width = right - left;
            var height = bottom - top;
            var depth = back - front;

            Threading.BlockOnUIThread(() =>
            {
                var elementSizeInByte = Marshal.SizeOf<T>();
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                try
                {
                    var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);

                    GL.BindTexture(glTarget, glTexture);
                    GraphicsExtensions.CheckGLError();

                    GL.TexSubImage3D(glTarget, level, left, top, front, width, height, depth, glFormat, glType, dataPtr);
                    GraphicsExtensions.CheckGLError();
                }
                finally
                {
                    dataHandle.Free();
                }
            });
#endif
        }

        private void PlatformGetData<T>(int level, int left, int top, int right, int bottom, int front, int back, T[] data, int startIndex, int elementCount)
             where T : struct
        {
#if GLES
            throw new NotSupportedException("OpenGL ES 2.0 doesn't support 3D textures.");
#else
            var width = right - left;
            var height = bottom - top;
            var depth = back - front;

            Threading.BlockOnUIThread(() =>
            {
                GL.BindTexture(glTarget, glTexture);
                GraphicsExtensions.CheckGLError();

                // If we're getting a subset of the texture, we need to read the entire level and copy the relevant portion
                if (left != 0 || top != 0 || front != 0 || width != Width || height != Height || depth != Depth)
                {
                    var elementSizeInByte = Marshal.SizeOf<T>();
                    var levelWidth = Math.Max(Width >> level, 1);
                    var levelHeight = Math.Max(Height >> level, 1);
                    var levelDepth = Math.Max(Depth >> level, 1);
                    var levelSize = levelWidth * levelHeight * levelDepth * Format.GetSize();
                    var tempData = new byte[levelSize];

                    var tempHandle = GCHandle.Alloc(tempData, GCHandleType.Pinned);
                    try
                    {
                        GL.GetTexImage(glTarget, level, glFormat, glType, tempData);
                        GraphicsExtensions.CheckGLError();

                        // Copy the subset to the output array
                        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                        try
                        {
                            var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
                            var formatSize = Format.GetSize();

                            for (int z = 0; z < depth; z++)
                            {
                                for (int y = 0; y < height; y++)
                                {
                                    var sourceIndex = ((front + z) * levelHeight * levelWidth + (top + y) * levelWidth + left) * formatSize;
                                    var destIndex = (z * height * width + y * width) * formatSize;
                                    Marshal.Copy(tempData, sourceIndex, dataPtr + destIndex, width * formatSize);
                                }
                            }
                        }
                        finally
                        {
                            dataHandle.Free();
                        }
                    }
                    finally
                    {
                        tempHandle.Free();
                    }
                }
                else
                {
                    // Get the entire texture level - we can read directly into the data array
                    GL.GetTexImage(glTarget, level, glFormat, glType, data);
                    GraphicsExtensions.CheckGLError();
                }
            });
#endif
        }
    }
}

