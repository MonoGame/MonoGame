// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
#endif

#if IOS
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
#endif

#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
using GLPixelFormat = MonoMac.OpenGL.PixelFormat;
#endif

#if WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
using GLPixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
#endif

#if GLES
using OpenTK.Graphics.ES20;
using GLPixelFormat = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
using TextureParameterName = OpenTK.Graphics.ES20.All;
using TextureMinFilter = OpenTK.Graphics.ES20.All;
using PixelInternalFormat = OpenTK.Graphics.ES20.All;
using PixelType = OpenTK.Graphics.ES20.All;
using PixelStoreParameter = OpenTK.Graphics.ES20.All;
using ErrorCode = OpenTK.Graphics.ES20.All;
#endif

#if ANDROID
using Android.Graphics;
#endif
#endif // OPENGL

#if WINDOWS || LINUX || MONOMAC || ANGLE
using System.Drawing.Imaging;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Texture2D : Texture
    {
		PixelInternalFormat glInternalFormat;
		GLPixelFormat glFormat;
		PixelType glType;

        private void PlatformConstruct(int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
        {
            this.glTarget = TextureTarget.Texture2D;
            
            Threading.BlockOnUIThread(() =>
            {
                // Store the current bound texture.
                var prevTexture = GraphicsExtensions.GetBoundTexture2D();

                GenerateGLTextureIfRequired();

                format.GetGLFormat(out glInternalFormat, out glFormat, out glType);

                if (glFormat == (GLPixelFormat)All.CompressedTextureFormats)
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
                        case SurfaceFormat.RgbEtc1:
                        case SurfaceFormat.Dxt1:
                        case SurfaceFormat.Dxt1a:
                        case SurfaceFormat.Dxt3:
                        case SurfaceFormat.Dxt5:
                            imageSize = ((this.width + 3) / 4) * ((this.height + 3) / 4) * format.Size();
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
                    GL.TexImage2D(TextureTarget.Texture2D, 0,
#if IOS || ANDROID
                        (int)glInternalFormat,
#else				           
					    glInternalFormat,
#endif
                        this.width, this.height, 0,
                        glFormat, glType, IntPtr.Zero);
                    GraphicsExtensions.CheckGLError();
                }

                // Restore the bound texture.
                GL.BindTexture(TextureTarget.Texture2D, prevTexture);
                GraphicsExtensions.CheckGLError();
            });
        }

        private void PlatformSetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
            Threading.BlockOnUIThread(() =>
            {
            var elementSizeInByte = Marshal.SizeOf(typeof(T));
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            // Use try..finally to make sure dataHandle is freed in case of an error
            try
            {
                var startBytes = startIndex * elementSizeInByte;
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
                int x, y, w, h;
                if (rect.HasValue)
                {
                    x = rect.Value.X;
                    y = rect.Value.Y;
                    w = rect.Value.Width;
                    h = rect.Value.Height;
                }
                else
                {
                    x = 0;
                    y = 0;
                    w = Math.Max(width >> level, 1);
                    h = Math.Max(height >> level, 1);

                    // For DXT textures the width and height of each level is a multiple of 4.
                    // OpenGL only: The last two mip levels require the width and height to be 
                    // passed as 2x2 and 1x1, but there needs to be enough data passed to occupy 
                    // a 4x4 block. 
                    // Ref: http://www.mentby.com/Group/mac-opengl/issue-with-dxt-mipmapped-textures.html 
                    if (_format == SurfaceFormat.Dxt1 ||
                        _format == SurfaceFormat.Dxt1a ||
                        _format == SurfaceFormat.Dxt3 ||
                        _format == SurfaceFormat.Dxt5)
                    {
                            if (w > 4)
                                w = (w + 3) & ~3;
                            if (h > 4)
                                h = (h + 3) & ~3;
                    }
                }

                    // Store the current bound texture.
                    var prevTexture = GraphicsExtensions.GetBoundTexture2D();

                    GenerateGLTextureIfRequired();

                    GL.BindTexture(TextureTarget.Texture2D, this.glTexture);
                    GraphicsExtensions.CheckGLError();
                    if (glFormat == (GLPixelFormat)All.CompressedTextureFormats)
                    {
                        if (rect.HasValue)
                        {
                            GL.CompressedTexSubImage2D(TextureTarget.Texture2D,
                                level, x, y, w, h,
#if GLES
                                glInternalFormat,
#else
                                glFormat,
#endif
                                data.Length - startBytes, dataPtr);
                            GraphicsExtensions.CheckGLError();
                        }
                        else
                        {
                            GL.CompressedTexImage2D(TextureTarget.Texture2D, level, glInternalFormat, w, h, 0, data.Length - startBytes, dataPtr);
                            GraphicsExtensions.CheckGLError();
                        }
                    }
                    else
                    {
                        // Set pixel alignment to match texel size in bytes
                        GL.PixelStore(PixelStoreParameter.UnpackAlignment, GraphicsExtensions.Size(this.Format));
                        if (rect.HasValue)
                        {
                            GL.TexSubImage2D(TextureTarget.Texture2D, level,
                                            x, y, w, h,
                                            glFormat, glType, dataPtr);
                            GraphicsExtensions.CheckGLError();
                        }
                        else
                        {
                            GL.TexImage2D(TextureTarget.Texture2D, level,
#if GLES && !ANGLE
                                (int)glInternalFormat,
#else
                                glInternalFormat,
#endif
                                w, h, 0, glFormat, glType, dataPtr);
                            GraphicsExtensions.CheckGLError();
                        }
                        // Return to default pixel alignment
                        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
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

        private void PlatformGetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
#if IOS

            // Reading back a texture from GPU memory is unsupported
            // in OpenGL ES 2.0 and no work around has been implemented.           
            throw new NotSupportedException("OpenGL ES 2.0 does not support texture reads.");
#endif
#if ANDROID

            Rectangle r;
            if (rect != null)
            {
                r = rect.Value;
            }
            else
            {
                r = new Rectangle(0, 0, Width, Height);
            }
            			
			// Get the Color values
			if (typeof(T) == typeof(uint))
			{
				Color[] colors = new Color[elementCount];
				GetData<Color>(level, rect, colors, startIndex, elementCount);
				uint[] final = data as uint[];
				for (int i = 0; i < final.Length; i++)
				{
					final[i] = (uint)
					(
						// use correct xna byte order (and remember to convert it yourself as needed)
						colors[i].A << 24 |
						colors[i].B << 16 |
						colors[i].G << 8 |
						colors[i].R
					);
				}
			}
            // Get the Color values
            else if ((typeof(T) == typeof(Color)))
            {
				byte[] imageInfo = GetTextureData(0);

                int rWidth = r.Width;
                int rHeight = r.Height;
                
                // Loop through and extract the data but we need to load it 
                var dataRowColOffset = 0;
                var sz = 0;
                var pixelOffset = 0;
                for (int y = r.Top; y < rHeight; y++)
                {
                    for (int x = r.Left; x < rWidth; x++)
                    {
                        var result = new Color(0, 0, 0, 0);
                        dataRowColOffset = ((y * r.Width) + x);
                        switch (Format)
                        {
                            case SurfaceFormat.Color: //kTexture2DPixelFormat_RGBA8888
                            case SurfaceFormat.Dxt3:
                                sz = 4;
                                pixelOffset = dataRowColOffset * sz;
                                result.R = imageInfo[pixelOffset];
                                result.G = imageInfo[pixelOffset + 1];
                                result.B = imageInfo[pixelOffset + 2];
                                result.A = imageInfo[pixelOffset + 3];
                                break;
                            case SurfaceFormat.Bgra4444: //kTexture2DPixelFormat_RGBA4444
                                //								sz = 2;
                                //								pos = ((y * imageSize.Width) + x) * sz;
                                //								pixelOffset = new IntPtr (imageData.ToInt64 () + pos);
                                //	
                                //								Marshal.Copy (pixelOffset, pixel, 0, 4);	
                                //	
                                //								result.R = pixel [0];
                                //								result.G = pixel [1];
                                //								result.B = pixel [2];
                                //								result.A = pixel [3];
                                sz = 2;
                                pixelOffset = dataRowColOffset * sz;
                                result.R = imageInfo[pixelOffset];
                                result.G = imageInfo[pixelOffset + 1];
                                result.B = imageInfo[pixelOffset + 2];
                                result.A = imageInfo[pixelOffset + 3];
                                break;
                            case SurfaceFormat.Bgra5551: //kTexture2DPixelFormat_RGB5A1
                                //								sz = 2;
                                //								pos = ((y * imageSize.Width) + x) * sz;
                                //								pixelOffset = new IntPtr (imageData.ToInt64 () + pos);
                                //								Marshal.Copy (pixelOffset, pixel, 0, 4);	
                                //	
                                //								result.R = pixel [0];
                                //								result.G = pixel [1];
                                //								result.B = pixel [2];
                                //								result.A = pixel [3];
                                sz = 2;
                                pixelOffset = dataRowColOffset * sz;
                                result.R = imageInfo[pixelOffset];
                                result.G = imageInfo[pixelOffset + 1];
                                result.B = imageInfo[pixelOffset + 2];
                                result.A = imageInfo[pixelOffset + 3];
                                break;
                            case SurfaceFormat.Alpha8:  // kTexture2DPixelFormat_A8 
                                //								sz = 1;
                                //								pos = ((y * imageSize.Width) + x) * sz;
                                //								pixelOffset = new IntPtr (imageData.ToInt64 () + pos);								
                                //								Marshal.Copy (pixelOffset, pixel, 0, 4);	
                                //	
                                //								result.A = pixel [0];
                                sz = 1;
                                pixelOffset = dataRowColOffset * sz;
                                result.A = imageInfo[pixelOffset];
                                break;
                            default:
                                throw new NotSupportedException("Texture format");
                        }
                        data[dataRowColOffset] = (T)(object)result;
                    }                    
                }
            }
            else
            {
                throw new NotImplementedException("GetData not implemented for type.");
            }
#endif
#if !GLES
            GL.BindTexture(TextureTarget.Texture2D, this.glTexture);

            if (glFormat == (GLPixelFormat)All.CompressedTextureFormats)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (rect.HasValue)
                {
                    var temp = new T[this.width * this.height];
                    GL.GetTexImage(TextureTarget.Texture2D, level, this.glFormat, this.glType, temp);
                    int z = 0, w = 0;

                    for (int y = rect.Value.Y; y < rect.Value.Y + rect.Value.Height; ++y)
                    {
                        for (int x = rect.Value.X; x < rect.Value.X + rect.Value.Width; ++x)
                        {
                            data[z * rect.Value.Width + w] = temp[(y * width) + x];
                            ++w;
                        }
                        ++z;
                        w = 0;
                    }
                }
                else
                {
                    GL.GetTexImage(TextureTarget.Texture2D, level, this.glFormat, this.glType, data);
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
				var rectangle = RectangleF.Empty;
				var cgImage = nsImage.AsCGImage (ref rectangle, null, null);
#endif
				
				var width = cgImage.Width;
				var height = cgImage.Height;
				
				var data = new byte[width * height * 4];
				
				var colorSpace = CGColorSpace.CreateDeviceRGB();
				var bitmapContext = new CGBitmapContext(data, width, height, 8, width * 4, colorSpace, CGBitmapFlags.PremultipliedLast);
				bitmapContext.DrawImage(new RectangleF(0, 0, width, height), cgImage);
				bitmapContext.Dispose();
				colorSpace.Dispose();
				
                Texture2D texture = null;
                Threading.BlockOnUIThread(() =>
                {
				    texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);			
    				texture.SetData(data);
                });
			
				return texture;
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
#if WINDOWS || LINUX || ANGLE
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
#if MONOMAC
			SaveAsImage(stream, width, height, ImageFormat.Jpeg);
#else
            throw new NotImplementedException();
#endif
        }

        private void PlatformSaveAsPng(Stream stream, int width, int height)
        {
            // TODO: We need to find a simple stand alone
            // PNG encoder if we want to support this.
            throw new NotImplementedException();
        }

#if MONOMAC
		private void SaveAsImage(Stream stream, int width, int height, ImageFormat format)
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
#endif

        // This method allows games that use Texture2D.FromStream 
        // to reload their textures after the GL context is lost.
        private void PlatformReload(Stream textureStream)
        {
            GenerateGLTextureIfRequired();
            FillTextureFromStream(textureStream);
        }

        private void GenerateGLTextureIfRequired()
        {
            if (this.glTexture < 0)
            {
#if IOS || ANDROID
                GL.GenTextures(1, ref this.glTexture);
#else
                GL.GenTextures(1, out this.glTexture);
#endif
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

#if ANDROID
		private byte[] GetTextureData(int ThreadPriorityLevel)
		{
			int framebufferId = -1;
            int renderBufferID = -1;
            
			GL.GenFramebuffers(1, ref framebufferId);
            GraphicsExtensions.CheckGLError();
            GL.BindFramebuffer(All.Framebuffer, framebufferId);
            GraphicsExtensions.CheckGLError();
            //renderBufferIDs = new int[currentRenderTargets];
            GL.GenRenderbuffers(1, ref renderBufferID);
            GraphicsExtensions.CheckGLError();

            // attach the texture to FBO color attachment point
            GL.FramebufferTexture2D(All.Framebuffer, All.ColorAttachment0,
                All.Texture2D, this.glTexture, 0);
            GraphicsExtensions.CheckGLError();

            // create a renderbuffer object to store depth info
            GL.BindRenderbuffer(All.Renderbuffer, renderBufferID);
            GraphicsExtensions.CheckGLError();

			var glDepthFormat = GraphicsCapabilities.SupportsDepth24 ? All.DepthComponent24Oes : GraphicsCapabilities.SupportsDepthNonLinear ? (OpenTK.Graphics.ES20.All)0x8E2C /*GLDepthComponent16NonLinear */: All.DepthComponent16;
			GL.RenderbufferStorage(All.Renderbuffer, glDepthFormat, Width, Height);
            GraphicsExtensions.CheckGLError();

            // attach the renderbuffer to depth attachment point
            GL.FramebufferRenderbuffer(All.Framebuffer, All.DepthAttachment,
                All.Renderbuffer, renderBufferID);
            GraphicsExtensions.CheckGLError();

            All status = GL.CheckFramebufferStatus(All.Framebuffer);

            if (status != All.FramebufferComplete)
                throw new Exception("Error creating framebuffer: " + status);	
			byte[] imageInfo;
            int sz = 0;

            switch (this.Format)
            {
                case SurfaceFormat.Color: //kTexture2DPixelFormat_RGBA8888
                case SurfaceFormat.Dxt3:

                    sz = 4;
                    imageInfo = new byte[(Width * Height) * sz];
                    break;
                case SurfaceFormat.Bgra4444: //kTexture2DPixelFormat_RGBA4444
                    sz = 2;
                    imageInfo = new byte[(Width * Height) * sz];

                    break;
                case SurfaceFormat.Bgra5551: //kTexture2DPixelFormat_RGB5A1
                    sz = 2;
                    imageInfo = new byte[(Width * Height) * sz];
                    break;
                case SurfaceFormat.Alpha8:  // kTexture2DPixelFormat_A8 
                    sz = 1;
                    imageInfo = new byte[(Width * Height) * sz];
                    break;
                default:
                    throw new NotSupportedException("Texture format");
            }

			GL.ReadPixels(0,0,Width, Height, All.Rgba, All.UnsignedByte, imageInfo);
            GraphicsExtensions.CheckGLError();
            GL.FramebufferRenderbuffer(All.Framebuffer, All.DepthAttachment, All.Renderbuffer, 0);
            GraphicsExtensions.CheckGLError();
            GL.DeleteRenderbuffers(1, ref renderBufferID);
            GraphicsExtensions.CheckGLError();
            GL.DeleteFramebuffers(1, ref framebufferId);
            GraphicsExtensions.CheckGLError();
            GL.BindFramebuffer(All.Framebuffer, 0);
            GraphicsExtensions.CheckGLError();
            return imageInfo;
		}
#endif
	}
}

