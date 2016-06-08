// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if MONOMAC && PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
using GLPixelFormat = MonoMac.OpenGL.All;
using Bool = MonoMac.OpenGL.Boolean;
#endif
#if (MONOMAC && !PLATFORM_MACOS_LEGACY)
using OpenTK.Graphics.OpenGL;
using GLPixelFormat = OpenTK.Graphics.OpenGL.All;
using Bool = OpenTK.Graphics.OpenGL.Boolean;
#endif
#if DESKTOPGL
using OpenGL;
using GLPixelFormat = OpenGL.PixelFormat;
using PixelFormat = OpenGL.PixelFormat;
#endif
#if GLES
using OpenTK.Graphics.ES20;
using GLPixelFormat = OpenTK.Graphics.ES20.All;
using PixelFormat = OpenTK.Graphics.ES20.PixelFormat;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class TextureCube
	{
        private void PlatformConstruct(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            this.glTarget = TextureTarget.TextureCubeMap;

            Threading.BlockOnUIThread(() =>
            {
			GL.GenTextures(1, out this.glTexture);
            GraphicsExtensions.CheckGLError();
            GL.BindTexture(TextureTarget.TextureCubeMap, this.glTexture);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter,
                            mipMap ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter,
                            (int)TextureMagFilter.Linear);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS,
                            (int)TextureWrapMode.ClampToEdge);
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT,
                            (int)TextureWrapMode.ClampToEdge);
            GraphicsExtensions.CheckGLError();


            format.GetGLFormat(GraphicsDevice, out glInternalFormat, out glFormat, out glType);

            for (int i = 0; i < 6; i++)
            {
                TextureTarget target = GetGLCubeFace((CubeMapFace)i);

                if (glFormat == (PixelFormat)GLPixelFormat.CompressedTextureFormats)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    GL.TexImage2D(target, 0, glInternalFormat, size, size, 0, glFormat, glType, IntPtr.Zero);
                    GraphicsExtensions.CheckGLError();
                }
            }

            if (mipMap)
            {
#if IOS || ANDROID
				GL.GenerateMipmap(TextureTarget.TextureCubeMap);
#else
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.GenerateMipmap, (int)Bool.True);
#endif
                GraphicsExtensions.CheckGLError();
            }
            });
        }

        private void PlatformGetData<T>(CubeMapFace cubeMapFace, T[] data) where T : struct
        {
#if OPENGL && (MONOMAC || DESKTOPGL)
            TextureTarget target = GetGLCubeFace(cubeMapFace);
            GL.BindTexture(TextureTarget.TextureCubeMap, this.glTexture);
            GraphicsExtensions.CheckGLError();
            GL.GetTexImage<T>(target, 0, glFormat, glType, data);
            GraphicsExtensions.CheckGLError();
#else
            throw new NotImplementedException();
#endif
        }

        private void PlatformSetData<T>(CubeMapFace face, int level, IntPtr dataPtr, int xOffset, int yOffset, int width, int height)
        {
            Threading.BlockOnUIThread(() =>
            {
            GL.BindTexture(TextureTarget.TextureCubeMap, this.glTexture);
            GraphicsExtensions.CheckGLError();

            TextureTarget target = GetGLCubeFace(face);
            if (glFormat == (PixelFormat)GLPixelFormat.CompressedTextureFormats)
            {
                throw new NotImplementedException();
            }
            else
            {
                GL.TexSubImage2D(target, level, xOffset, yOffset, width, height, glFormat, glType, dataPtr);
                GraphicsExtensions.CheckGLError();
            }
            });
        }

		private TextureTarget GetGLCubeFace(CubeMapFace face) 
        {
			switch (face) 
            {
			case CubeMapFace.PositiveX: return TextureTarget.TextureCubeMapPositiveX;
			case CubeMapFace.NegativeX: return TextureTarget.TextureCubeMapNegativeX;
			case CubeMapFace.PositiveY: return TextureTarget.TextureCubeMapPositiveY;
			case CubeMapFace.NegativeY: return TextureTarget.TextureCubeMapNegativeY;
			case CubeMapFace.PositiveZ: return TextureTarget.TextureCubeMapPositiveZ;
			case CubeMapFace.NegativeZ: return TextureTarget.TextureCubeMapNegativeZ;
			}
			throw new ArgumentException();
		}
	}
}

