// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
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

                for (var i = 0; i < 6; i++)
                {
                    var target = GetGLCubeFace((CubeMapFace)i);

                    if (glFormat == (PixelFormat)GLPixelFormat.CompressedTextureFormats)
                    {
                        var imageSize = 0;
                        switch (format)
                        {
                            case SurfaceFormat.RgbPvrtc2Bpp:
                            case SurfaceFormat.RgbaPvrtc2Bpp:
                                imageSize = (Math.Max(size, 16) * Math.Max(size, 8) * 2 + 7) / 8;
                                break;
                            case SurfaceFormat.RgbPvrtc4Bpp:
                            case SurfaceFormat.RgbaPvrtc4Bpp:
                                imageSize = (Math.Max(size, 8) * Math.Max(size, 8) * 4 + 7) / 8;
                                break;
                            case SurfaceFormat.Dxt1:
                            case SurfaceFormat.Dxt1a:
                            case SurfaceFormat.Dxt1SRgb:
                            case SurfaceFormat.Dxt3:
                            case SurfaceFormat.Dxt3SRgb:
                            case SurfaceFormat.Dxt5:
                            case SurfaceFormat.Dxt5SRgb:
                            case SurfaceFormat.RgbEtc1:
                            case SurfaceFormat.RgbaAtcExplicitAlpha:
                            case SurfaceFormat.RgbaAtcInterpolatedAlpha:
                                imageSize = (size + 3) / 4 * ((size + 3) / 4) * format.GetSize();
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                        GL.CompressedTexImage2D(target, 0, glInternalFormat, size, size, 0, imageSize, IntPtr.Zero);
                        GraphicsExtensions.CheckGLError();
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

        private void PlatformGetData<T>(CubeMapFace cubeMapFace, int level, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
#if OPENGL && (MONOMAC || DESKTOPGL)
            if (glFormat == (PixelFormat) GLPixelFormat.CompressedTextureFormats)
            {
                throw new NotImplementedException();
            }
            else
            {
                var target = GetGLCubeFace(cubeMapFace);
                GL.BindTexture(TextureTarget.TextureCubeMap, this.glTexture);
                GraphicsExtensions.CheckGLError();

                var temp = new T[this.size * this.size];
                GL.GetTexImage(target, level, this.glFormat, this.glType, temp);
                GraphicsExtensions.CheckGLError();
                int z = 0, w = 0;

                for (var y = rect.Y; y < rect.Y + rect.Height; ++y)
                {
                    for (var x = rect.X; x < rect.X + rect.Width; ++x)
                    {
                        data[startIndex + z*rect.Width + w] = temp[y*size + x];
                        ++w;
                    }
                    ++z;
                    w = 0;
                }
            }
#else
            throw new NotImplementedException();
#endif
        }

        private void PlatformSetData<T>(CubeMapFace face, int level, Rectangle rect, T[] data, int startIndex, int elementCount)
        {
            Threading.BlockOnUIThread(() =>
            {
                var elementSizeInByte = Utilities.ReflectionHelpers.SizeOf<T>.Get();
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                // Use try..finally to make sure dataHandle is freed in case of an error
                try
                {
                    var startBytes = startIndex * elementSizeInByte;
                    var dataPtr = (IntPtr) (dataHandle.AddrOfPinnedObject().ToInt64() + startIndex*elementSizeInByte);
                    GL.BindTexture(TextureTarget.TextureCubeMap, this.glTexture);
                    GraphicsExtensions.CheckGLError();

                    var target = GetGLCubeFace(face);
                    if (glFormat == (PixelFormat) GLPixelFormat.CompressedTextureFormats)
                    {
                        GL.CompressedTexSubImage2D(target, level, rect.X, rect.Y, rect.Width, rect.Height, (PixelFormat)glInternalFormat, elementCount * startBytes, dataPtr);
                        GraphicsExtensions.CheckGLError();
                    }
                    else
                    {
                        GL.TexSubImage2D(target, level, rect.X, rect.Y, rect.Width, rect.Height, glFormat, glType,
                            dataPtr);
                        GraphicsExtensions.CheckGLError();
                    }
                }
                finally
                {
                    dataHandle.Free();
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

