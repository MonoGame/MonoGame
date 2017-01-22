// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using Microsoft.Xna.Framework.Utilities;
using MonoGame.Utilities.Png;

#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
#else
using AppKit;
using CoreGraphics;
using Foundation;
#endif
#endif

#if IOS
using UIKit;
using CoreGraphics;
using Foundation;
#endif

#if OPENGL
#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
using GLPixelFormat = MonoMac.OpenGL.All;
using PixelFormat = MonoMac.OpenGL.PixelFormat;
#else
using OpenTK.Graphics.OpenGL;
using GLPixelFormat = OpenTK.Graphics.OpenGL.All;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
using PixelInternalFormat = OpenTK.Graphics.OpenGL.PixelFormat;
#endif
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
using PixelInternalFormat = OpenTK.Graphics.ES20.PixelFormat;
#endif

#if ANDROID
using Android.Graphics;
#endif
#endif // OPENGL

#if DESKTOPGL || MONOMAC || ANGLE
using System.Drawing.Imaging;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
        private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
        {
            this.glTarget = TextureTarget.Texture2D;

            Threading.BlockOnUIThread(() =>
            {
                // Store the current bound texture.
                var prevTexture = GraphicsExtensions.GetBoundTexture2D();

                GenerateGLTextureIfRequired();

                format.GetGLFormat(GraphicsDevice, out glInternalFormat, out glFormat, out glType);

                if (glFormat == (PixelFormat)GLPixelFormat.CompressedTextureFormats)
                {
                    var imageSize = 0;
                    switch (format)
                    {
                        case SurfaceFormat.RgbPvrtc2Bpp:
                        case SurfaceFormat.RgbaPvrtc2Bpp:
                            imageSize = (Math.Max(this.width, 16) * Math.Max(this.height, 8) * 2 + 7) / 8;
                            break;
                        case SurfaceFormat.RgbPvrtc4Bpp:
                        case SurfaceFormat.RgbaPvrtc4Bpp:
                            imageSize = (Math.Max(this.width, 8) * Math.Max(this.height, 8) * 4 + 7) / 8;
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
                            imageSize = ((this.width + 3) / 4) * ((this.height + 3) / 4) * GraphicsExtensions.GetSize(format);
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, glInternalFormat,
                                            this.width, this.height, 0,
                                            imageSize, IntPtr.Zero);
                    GraphicsExtensions.CheckGLError();
                }
                else
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0, glInternalFormat,
                        this.width, this.height, 0,
                        glFormat, glType, IntPtr.Zero);
                    GraphicsExtensions.CheckGLError();
                }

                if (mipmap)
                {
#if IOS || ANDROID
				    GL.GenerateMipmap(TextureTarget.TextureCubeMap);
#else
                    GraphicsDevice.FramebufferHelper.Get().GenerateMipmap((int) glTarget);
                    // This updates the mipmaps after a change in the base texture
                    GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.GenerateMipmap, 1);
#endif
                }

                // Restore the bound texture.
                GL.BindTexture(TextureTarget.Texture2D, prevTexture);
                GraphicsExtensions.CheckGLError();

            });
        }

        private void PlatformSetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            Threading.BlockOnUIThread(() =>
            {
                var elementSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                // Use try..finally to make sure dataHandle is freed in case of an error
                try
                {
                    var startBytes = startIndex * elementSizeInByte;
                    var dataPtr = new IntPtr(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                    // Store the current bound texture.
                    var prevTexture = GraphicsExtensions.GetBoundTexture2D();

                    GenerateGLTextureIfRequired();

                    GL.BindTexture(TextureTarget.Texture2D, this.glTexture);
                    GraphicsExtensions.CheckGLError();

                    if (glFormat == (PixelFormat)GLPixelFormat.CompressedTextureFormats)
                    {
                        GL.CompressedTexSubImage2D(TextureTarget.Texture2D, level, rect.X, rect.Y, rect.Width, rect.Height,
                            (PixelInternalFormat) glInternalFormat, elementCount * elementSizeInByte, dataPtr);
                        GraphicsExtensions.CheckGLError();
                    }
                    else
                    {
                        GL.TexSubImage2D(TextureTarget.Texture2D, level, rect.X, rect.Y,
                            rect.Width, rect.Height, glFormat, glType, dataPtr);
                        GraphicsExtensions.CheckGLError();
                    }

#if !ANDROID
                    GL.Finish();
                    GraphicsExtensions.CheckGLError();
#endif
                    // Restore the bound texture.
                    GL.BindTexture(TextureTarget.Texture2D, prevTexture);
                    GraphicsExtensions.CheckGLError();
                }
                finally
                {
                    dataHandle.Free();
                }

#if !ANDROID
                // Required to make sure that any texture uploads on a thread are completed
                // before the main thread tries to use the texture.
                GL.Finish();
#endif
            });
        }

        private void PlatformGetData<T>(int level, int arraySlice, Rectangle rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            Threading.EnsureUIThread();

#if GLES
            // TODO: check for for non renderable formats (formats that can't be attached to FBO)

            var framebufferId = 0;
			#if (IOS || ANDROID)
			GL.GenFramebuffers(1, out framebufferId);
			#else
            GL.GenFramebuffers(1, ref framebufferId);
			#endif
            GraphicsExtensions.CheckGLError();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
            GraphicsExtensions.CheckGLError();
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferSlot.ColorAttachment0, TextureTarget.Texture2D, this.glTexture, 0);
            GraphicsExtensions.CheckGLError();

            GL.ReadPixels(rect.X, rect.Y, rect.Width, rect.Height, this.glFormat, this.glType, data);
            GraphicsExtensions.CheckGLError();
            GL.DeleteFramebuffers(1, ref framebufferId);
            GraphicsExtensions.CheckGLError();
#else
            var tSizeInByte = ReflectionHelpers.SizeOf<T>.Get();
            GL.BindTexture(TextureTarget.Texture2D, this.glTexture);

            if (glFormat == (PixelFormat) GLPixelFormat.CompressedTextureFormats)
            {
                // Note: for compressed format Format.GetSize() returns the size of a 4x4 block
                var pixelToT = Format.GetSize() / tSizeInByte;
                var tFullWidth = Math.Max(this.width >> level, 1) / 4 * pixelToT;
                var temp = new T[Math.Max(this.height >> level, 1) / 4 * tFullWidth];
#if MONOMAC
                var tempHandle = GCHandle.Alloc(temp, GCHandleType.Pinned);
                var ptr = tempHandle.AddrOfPinnedObject();
                GL.GetCompressedTexImage(TextureTarget.Texture2D, level, ptr);
                tempHandle.Free();
#else
                GL.GetCompressedTexImage(TextureTarget.Texture2D, level, temp);
#endif
                GraphicsExtensions.CheckGLError();

                var rowCount = rect.Height / 4;
                var tRectWidth = rect.Width / 4 * Format.GetSize() / tSizeInByte;
                for (var r = 0; r < rowCount; r++)
                {
                    var tempStart = rect.X / 4 * pixelToT + (rect.Top / 4 + r) * tFullWidth;
                    var dataStart = startIndex + r * tRectWidth;
                    Array.Copy(temp, tempStart, data, dataStart, tRectWidth);
                }
            }
            else
            {
                // we need to convert from our format size to the size of T here
                var tFullWidth = Math.Max(this.width >> level, 1) * Format.GetSize() / tSizeInByte;
                var temp = new T[Math.Max(this.height >> level, 1) * tFullWidth];
                GL.GetTexImage(TextureTarget.Texture2D, level, glFormat, glType, temp);
                GraphicsExtensions.CheckGLError();

                var pixelToT = Format.GetSize() / tSizeInByte;
                var rowCount = rect.Height;
                var tRectWidth = rect.Width * pixelToT;
                for (var r = 0; r < rowCount; r++)
                {
                    var tempStart = rect.X * pixelToT + (r + rect.Top) * tFullWidth;
                    var dataStart = startIndex + r * tRectWidth;
                    Array.Copy(temp, tempStart, data, dataStart, tRectWidth);
                }
            }
#endif
            }

        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Stream stream)
        {
#if IOS || MONOMAC

#if IOS
			using (var uiImage = UIImage.LoadFromData(NSData.FromStream(stream)))
#elif MONOMAC
			using (var nsImage = NSImage.FromStream (stream))
#endif
			{
#if IOS
				var cgImage = uiImage.CGImage;
#elif MONOMAC
#if PLATFORM_MACOS_LEGACY
				var rectangle = RectangleF.Empty;
#else
                var rectangle = CGRect.Empty;
#endif
                var cgImage = nsImage.AsCGImage (ref rectangle, null, null);
#endif

			    return PlatformFromStream(graphicsDevice, cgImage);
			}
#endif
#if ANDROID
            using (Bitmap image = BitmapFactory.DecodeStream(stream, null, new BitmapFactory.Options
            {
                InScaled = false,
                InDither = false,
                InJustDecodeBounds = false,
                InPurgeable = true,
                InInputShareable = true,
            }))
            {
                return PlatformFromStream(graphicsDevice, image);
            }
#endif
#if DESKTOPGL || ANGLE
            Bitmap image = (Bitmap)Bitmap.FromStream(stream);
            try
            {
                // Fix up the Image to match the expected format
                image = (Bitmap)image.RGBToBGR();

                var data = new byte[image.Width * image.Height * 4];

                BitmapData bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                if (bitmapData.Stride != image.Width * 4)
                    throw new NotImplementedException();
                Marshal.Copy(bitmapData.Scan0, data, 0, data.Length);
                image.UnlockBits(bitmapData);

                Texture2D texture = null;
                texture = new Texture2D(graphicsDevice, image.Width, image.Height);
                texture.SetData(data);

                return texture;
            }
            finally
            {
                image.Dispose();
            }
#endif
        }

#if IOS
        [CLSCompliant(false)]
        public static Texture2D FromStream(GraphicsDevice graphicsDevice, UIImage uiImage)
        {
            return PlatformFromStream(graphicsDevice, uiImage.CGImage);
        }
#elif ANDROID
        [CLSCompliant(false)]
        public static Texture2D FromStream(GraphicsDevice graphicsDevice, Bitmap bitmap)
        {
            return PlatformFromStream(graphicsDevice, bitmap);
        }

        [CLSCompliant(false)]
        public void Reload(Bitmap image)
        {
            var width = image.Width;
            var height = image.Height;

            int[] pixels = new int[width * height];
            if ((width != image.Width) || (height != image.Height))
            {
                using (Bitmap imagePadded = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888))
                {
                    Canvas canvas = new Canvas(imagePadded);
                    canvas.DrawARGB(0, 0, 0, 0);
                    canvas.DrawBitmap(image, 0, 0, null);
                    imagePadded.GetPixels(pixels, 0, width, 0, 0, width, height);
                    imagePadded.Recycle();
                }
            }
            else
            {
                image.GetPixels(pixels, 0, width, 0, 0, width, height);
            }

            image.Recycle();

            this.SetData<int>(pixels);
        }
#endif

#if MONOMAC
        public static Texture2D FromStream(GraphicsDevice graphicsDevice, NSImage nsImage)
        {
#if PLATFORM_MACOS_LEGACY
            var rectangle = RectangleF.Empty;
#else
            var rectangle = CGRect.Empty;
#endif
		    var cgImage = nsImage.AsCGImage (ref rectangle, null, null);
            return PlatformFromStream(graphicsDevice, cgImage);
        }
#endif

#if IOS || MONOMAC
        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, CGImage cgImage)
        {
			var width = cgImage.Width;
			var height = cgImage.Height;

            var data = new byte[width * height * 4];

            var colorSpace = CGColorSpace.CreateDeviceRGB();
            var bitmapContext = new CGBitmapContext(data, width, height, 8, width * 4, colorSpace, CGBitmapFlags.PremultipliedLast);
#if PLATFORM_MACOS_LEGACY || IOS
            bitmapContext.DrawImage(new RectangleF(0, 0, width, height), cgImage);
#else
            bitmapContext.DrawImage(new CGRect(0, 0, width, height), cgImage);
#endif
            bitmapContext.Dispose();
            colorSpace.Dispose();

            Texture2D texture = null;
            Threading.BlockOnUIThread(() =>
            {
                texture = new Texture2D(graphicsDevice, (int)width, (int)height, false, SurfaceFormat.Color);
                texture.SetData(data);
            });

            return texture;
        }
#elif ANDROID
        private static Texture2D PlatformFromStream(GraphicsDevice graphicsDevice, Bitmap image)
        {
            var width = image.Width;
            var height = image.Height;

            int[] pixels = new int[width * height];
            if ((width != image.Width) || (height != image.Height))
            {
                using (Bitmap imagePadded = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888))
                {
                    Canvas canvas = new Canvas(imagePadded);
                    canvas.DrawARGB(0, 0, 0, 0);
                    canvas.DrawBitmap(image, 0, 0, null);
                    imagePadded.GetPixels(pixels, 0, width, 0, 0, width, height);
                    imagePadded.Recycle();
                }
            }
            else
            {
                image.GetPixels(pixels, 0, width, 0, 0, width, height);
            }
            image.Recycle();

            // Convert from ARGB to ABGR
            ConvertToABGR(height, width, pixels);

            Texture2D texture = null;
            Threading.BlockOnUIThread(() =>
            {
                texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
                texture.SetData<int>(pixels);
            });

            return texture;
        }
#endif

        private void FillTextureFromStream(Stream stream)
        {
#if ANDROID
            using (Bitmap image = BitmapFactory.DecodeStream(stream, null, new BitmapFactory.Options
            {
                InScaled = false,
                InDither = false,
                InJustDecodeBounds = false,
                InPurgeable = true,
                InInputShareable = true,
            }))
            {
                var width = image.Width;
                var height = image.Height;

                int[] pixels = new int[width * height];
                image.GetPixels(pixels, 0, width, 0, 0, width, height);

                // Convert from ARGB to ABGR
                ConvertToABGR(height, width, pixels);

                this.SetData<int>(pixels);
                image.Recycle();
            }
#endif
        }

        private void PlatformSaveAsJpeg(Stream stream, int width, int height)
        {
#if DESKTOPGL || MONOMAC
			SaveAsImage(stream, width, height, ImageFormat.Jpeg);
#elif ANDROID
            SaveAsImage(stream, width, height, Bitmap.CompressFormat.Jpeg);
#else
            throw new NotImplementedException();
#endif
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
#if ANDROID
            SaveAsImage(stream, width, height, Bitmap.CompressFormat.Png);
#else
            var pngWriter = new PngWriter();
            pngWriter.Write(this, stream);
#endif
        }

#if DESKTOPGL || MONOMAC
        internal void SaveAsImage(Stream stream, int width, int height, ImageFormat format)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream", "'stream' cannot be null (Nothing in Visual Basic)");
			}
			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException("width", width, "'width' cannot be less than or equal to zero");
			}
			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException("height", height, "'height' cannot be less than or equal to zero");
			}
			if (format == null)
			{
				throw new ArgumentNullException("format", "'format' cannot be null (Nothing in Visual Basic)");
			}

			byte[] data = null;
			GCHandle? handle = null;
			Bitmap bitmap = null;
			try
			{
				data = new byte[width * height * 4];
				handle = GCHandle.Alloc(data, GCHandleType.Pinned);
				GetData(data);

				// internal structure is BGR while bitmap expects RGB
				for(int i = 0; i < data.Length; i += 4)
				{
					byte temp = data[i + 0];
					data[i + 0] = data[i + 2];
					data[i + 2] = temp;
				}

				bitmap = new Bitmap(width, height, width * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, handle.Value.AddrOfPinnedObject());

				bitmap.Save(stream, format);
			}
			finally
			{
				if (bitmap != null)
				{
					bitmap.Dispose();
				}
				if (handle.HasValue)
				{
					handle.Value.Free();
				}
				if (data != null)
				{
					data = null;
				}
			}
		}
#elif ANDROID
        private void SaveAsImage(Stream stream, int width, int height, Bitmap.CompressFormat format)
        {
            int[] data = new int[width * height];
            GetData(data);
            // internal structure is BGR while bitmap expects RGB
            for (int i = 0; i < data.Length; ++i)
            {
                uint pixel = (uint)data[i];
                data[i] = (int)((pixel & 0xFF00FF00) | ((pixel & 0x00FF0000) >> 16) | ((pixel & 0x000000FF) << 16));
            }
            using (Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888))
            {
                bitmap.SetPixels(data, 0, width, 0, 0, width, height);
                bitmap.Compress(format, 100, stream);
                bitmap.Recycle();
            }
        }
#endif

        // This method allows games that use Texture2D.FromStream
        // to reload their textures after the GL context is lost.
        private void PlatformReload(Stream textureStream)
        {
            var prev = GraphicsExtensions.GetBoundTexture2D();

            GenerateGLTextureIfRequired();
            FillTextureFromStream(textureStream);

            GL.BindTexture(TextureTarget.Texture2D, prev);
        }

        private void GenerateGLTextureIfRequired()
        {
            if (this.glTexture < 0)
            {
                GL.GenTextures(1, out this.glTexture);
                GraphicsExtensions.CheckGLError();

                // For best compatibility and to keep the default wrap mode of XNA, only set ClampToEdge if either
                // dimension is not a power of two.
                var wrap = TextureWrapMode.Repeat;
                if (((width & (width - 1)) != 0) || ((height & (height - 1)) != 0))
                    wrap = TextureWrapMode.ClampToEdge;

                GL.BindTexture(TextureTarget.Texture2D, this.glTexture);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                                (_levelCount > 1) ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                                (int)TextureMagFilter.Linear);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrap);
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrap);
                GraphicsExtensions.CheckGLError();
            }
        }
    }
}

