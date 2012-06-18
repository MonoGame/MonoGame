#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009 The MonoGame Team

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
#if !PSS
using System.Drawing;
#endif
using System.IO;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
#elif IPHONE
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
#endif

#if MONOMAC
using MonoMac.OpenGL;
using GLPixelFormat = MonoMac.OpenGL.PixelFormat;
#elif WINDOWS || LINUX
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using GLPixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
#elif PSS
using PssTexture2D = Sce.Pss.Core.Graphics.Texture2D;
#elif GLES
using OpenTK.Graphics.ES20;
using GLPixelFormat = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
using TextureParameterName = OpenTK.Graphics.ES20.All;
using TextureMinFilter = OpenTK.Graphics.ES20.All;
using PixelInternalFormat = OpenTK.Graphics.ES20.All;
using PixelType = OpenTK.Graphics.ES20.All;
using ErrorCode = OpenTK.Graphics.ES20.All;
#endif

using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

#if WINRT
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using System.Threading.Tasks;
#endif

#if ANDROID
using Android.Graphics;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public class Texture2D : Texture
    {
		protected int width;
		protected int height;

#if PSS
		internal PssTexture2D _texture2D;

#elif OPENGL
		PixelInternalFormat glInternalFormat;
		GLPixelFormat glFormat;
		PixelType glType;
#endif
	
        public Rectangle Bounds
        {
            get
            {
				return new Rectangle(0, 0, this.width, this.height);
            }
        }

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
            : this(graphicsDevice, width, height, mipmap, format, false)
        {
        }
		
		internal Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, bool renderTarget)
		{
            if (graphicsDevice == null)
                throw new ArgumentNullException("Graphics Device Cannot Be Null");

            this.graphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;
            this.format = format;
            this.levelCount = 1;

            if (mipmap)
            {
                int size = Math.Max(this.width, this.height);
                while (size > 1)
                {
                    size = size / 2;
                    this.levelCount++;
                }
            }

#if DIRECTX

            // TODO: Move this to SetData() if we want to make Immutable textures!
            var desc = new SharpDX.Direct3D11.Texture2DDescription();
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = mipmap ? 0 : 1;
            desc.ArraySize = 1;
            desc.Format = SharpDXHelper.ToFormat(format);
            desc.BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource;
            desc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;
            desc.Usage = SharpDX.Direct3D11.ResourceUsage.Default;
            desc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;

            if (renderTarget)
                desc.BindFlags |= SharpDX.Direct3D11.BindFlags.RenderTarget;

            _texture = new SharpDX.Direct3D11.Texture2D(graphicsDevice._d3dDevice, desc);

#elif PSS
			_texture2D = new Sce.Pss.Core.Graphics.Texture2D(width, height, mipmap, PSSHelper.ToFormat(format));
#else

            this.glTarget = TextureTarget.Texture2D;
            
            Threading.BlockOnUIThread(() =>
            {
#if IPHONE || ANDROID
                GL.GenTextures(1, ref this.glTexture);
#else
			    GL.GenTextures(1, out this.glTexture);
#endif
                // For best compatibility and to keep the default wrap mode of XNA, only set ClampToEdge if either
                // dimension is not a power of two.
                TextureWrapMode wrap = TextureWrapMode.Repeat;
                if (((width & (width - 1)) != 0) || ((height & (height - 1)) != 0))
                    wrap = TextureWrapMode.ClampToEdge;

                GL.BindTexture(TextureTarget.Texture2D, this.glTexture);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                                mipmap ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                                (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)wrap);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrap);

                format.GetGLFormat(out glInternalFormat, out glFormat, out glType);

                if (glFormat == (GLPixelFormat)All.CompressedTextureFormats)
                {
                    var imageSize = 0;
                    switch (format)
                    {
                        case SurfaceFormat.RgbPvrtc2Bpp:
                        case SurfaceFormat.RgbaPvrtc2Bpp:
                            imageSize = (Math.Max(this.width, 8) * Math.Max(this.height, 8) * 2 + 7) / 8;
                            break;
                        case SurfaceFormat.RgbPvrtc4Bpp:
                        case SurfaceFormat.RgbaPvrtc4Bpp:
                            imageSize = (Math.Max(this.width, 16) * Math.Max(this.height, 8) * 4 + 7) / 8;
                            break;
                        case SurfaceFormat.Dxt1:
                            imageSize = ((this.width + 3) / 4) * ((this.height + 3) / 4) * 8 * 1;
                            break;
                        case SurfaceFormat.Dxt3:
                        case SurfaceFormat.Dxt5:
                            imageSize = ((this.width + 3) / 4) * ((this.height + 3) / 4) * 16 * 1;
                            break;
                        default:
                            throw new NotImplementedException();
                    }

                    GL.CompressedTexImage2D(TextureTarget.Texture2D, 0, glInternalFormat,
                                            this.width, this.height, 0,
                                            imageSize, IntPtr.Zero);
                }
                else
                {
                    GL.TexImage2D(TextureTarget.Texture2D, 0,
#if IPHONE || ANDROID
                        (int)glInternalFormat,
#else				           
					    glInternalFormat,
#endif
                        this.width, this.height, 0,
                        glFormat, glType, IntPtr.Zero);
                }
            });
#endif
        }
        
#if PSS
        private Texture2D(GraphicsDevice graphicsDevice, Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            _texture2D = new PssTexture2D(bytes, false);
            width = _texture2D.Width;
            height = _texture2D.Height;
            this.format = SurfaceFormat.Color; //FIXME HACK
            this.levelCount = 1;
        }
#endif
				
		public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
            : this(graphicsDevice, width, height, false, SurfaceFormat.Color, false)
		{			
		}

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
		}

        public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct 
        {
            if (data == null)
				throw new ArgumentNullException("data");

#if OPENGL
            Threading.BlockOnUIThread(() =>
            {
#endif
#if !PSS
                var elementSizeInByte = Marshal.SizeOf(typeof(T));
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                var startBytes = startIndex * elementSizeInByte;
                var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
#endif
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
                }

#if DIRECTX

                var box = new SharpDX.DataBox(dataPtr, GetPitch(w), 0);

                var region = new SharpDX.Direct3D11.ResourceRegion();
                region.Top = y;
                region.Front = 0;
                region.Back = 1;
                region.Bottom = y + h;
                region.Left = x;
                region.Right = x + w;

                // TODO: We need to deal with threaded contexts here!
                lock (graphicsDevice._d3dContext)
                    graphicsDevice._d3dContext.UpdateSubresource(box, _texture, level, region);

#elif PSS
                _texture2D.SetPixels(level, data, _texture2D.Format, startIndex, w, x, y, w, h);


#elif OPENGL
                GL.BindTexture(TextureTarget.Texture2D, this.glTexture);
                if (glFormat == (GLPixelFormat)All.CompressedTextureFormats)
                {
                    if (rect.HasValue)
                        GL.CompressedTexSubImage2D(TextureTarget.Texture2D, level, x, y, w, h, glInternalFormat, data.Length - startBytes, dataPtr);
                    else
                        GL.CompressedTexImage2D(TextureTarget.Texture2D, level, glInternalFormat, w, h, 0, data.Length - startBytes, dataPtr);
                }
                else
                {
                    if (rect.HasValue)
                    {
                        GL.TexSubImage2D(TextureTarget.Texture2D, level,
                                        x, y, w, h,
                                        glFormat, glType, dataPtr);
                    }
                    else
                    {
                        GL.TexImage2D(TextureTarget.Texture2D, level,
#if GLES
                                  (int)glInternalFormat,
#else
                                  glInternalFormat,
#endif
                                  w, h, 0, glFormat, glType, dataPtr);
                    }

                }

                Debug.Assert(GL.GetError() == ErrorCode.NoError);

#endif // OPENGL

#if !PSS
                dataHandle.Free();
#endif

#if OPENGL
            });
#endif
        }
		
		public void SetData<T>(T[] data, int startIndex, int elementCount) where T : struct
        {
            this.SetData(0, null, data, startIndex, elementCount);
        }
		
		public void SetData<T>(T[] data) where T : struct
        {
			this.SetData(0, null, data, 0, data.Length);
        }
		
		public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount) where T : struct
        {
#if IPHONE || ANDROID
			throw new NotImplementedException();

#elif PSS
            throw new NotImplementedException();
#elif DIRECTX

            // Create a temp staging resource for copying the data.
            // 
            // TODO: We should probably be pooling these staging resources
            // and not creating a new one each time.
            //
            var desc = new SharpDX.Direct3D11.Texture2DDescription();
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = 1;
            desc.ArraySize = 1;
            desc.Format = SharpDXHelper.ToFormat(format);
            desc.BindFlags = SharpDX.Direct3D11.BindFlags.None;
            desc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;
            desc.Usage = SharpDX.Direct3D11.ResourceUsage.Staging;
            desc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;

            using (var stagingTex = new SharpDX.Direct3D11.Texture2D(graphicsDevice._d3dDevice, desc))
            lock (graphicsDevice._d3dContext)
            {
                // Copy the data from the GPU to the staging texture.
                if (rect.HasValue)
                {
                    // TODO: Need to deal with subregion copies!
                    throw new NotImplementedException();
                }
                else
                    graphicsDevice._d3dContext.CopySubresourceRegion(_texture, level, null, stagingTex, 0, 0, 0, 0);

                // Copy the data to the array.
                SharpDX.DataStream stream;
                graphicsDevice._d3dContext.MapSubresource(stagingTex, 0, SharpDX.Direct3D11.MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out stream);
                stream.ReadRange(data, startIndex, elementCount);
                stream.Dispose();
            }

#else

			GL.BindTexture(TextureTarget.Texture2D, this.glTexture);

			if (rect.HasValue) {
				throw new NotImplementedException();
			}

			if (glFormat == (GLPixelFormat)All.CompressedTextureFormats) {
				throw new NotImplementedException();
			} else {
				GL.GetTexImage(TextureTarget.Texture2D, level, this.glFormat, this.glType, data);
			}

#endif
        }

		public void GetData<T>(T[] data, int startIndex, int elementCount) where T : struct
		{
			this.GetData(0, null, data, startIndex, elementCount);
		}
		
		public void GetData<T> (T[] data) where T : struct
		{
			this.GetData(0, null, data, 0, data.Length);
		}
		
		public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream stream)
		{
            //todo: partial classes would be cleaner
#if IPHONE || MONOMAC
            


#if IPHONE
			using (var uiImage = UIImage.LoadFromData(NSData.FromStream(stream)))
#elif MONOMAC
			using (var nsImage = NSImage.FromStream (stream))
#endif
			{
#if IPHONE
				var cgImage = uiImage.CGImage;
#elif MONOMAC
				var cgImage = nsImage.AsCGImage (RectangleF.Empty, null, null);
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
#elif ANDROID
            // Work-around for "The program 'Mono' has exited with code 255 (0xff)."
			// Based on http://stackoverflow.com/questions/7535503/mono-for-android-exit-code-255-on-bitmapfactory-decodestream
            //Bitmap image = BitmapFactory.DecodeStream(stream);
			Bitmap image = null;
			using (MemoryStream memStream = new MemoryStream())
			{
				stream.CopyTo(memStream);
				image = BitmapFactory.DecodeByteArray(memStream.GetBuffer(), 0, (int)memStream.Length);
			}
            var width = image.Width;
            var height = image.Height;

            int[] pixels = new int[width * height];
            if ((width != image.Width) || (height != image.Height))
            {
                Bitmap imagePadded = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
                Canvas canvas = new Canvas(imagePadded);
                canvas.DrawARGB(0, 0, 0, 0);
                canvas.DrawBitmap(image, 0, 0, null);
                imagePadded.GetPixels(pixels, 0, width, 0, 0, width, height);
            }
            else
            {
                image.GetPixels(pixels, 0, width, 0, 0, width, height);
            }

			// Convert from ARGB to ABGR
			for (int i = 0; i < width * height; ++i)
			{
				uint pixel = (uint)pixels[i];
				pixels[i] = (int)((pixel & 0xFF00FF00) | ((pixel & 0x00FF0000) >> 16) | ((pixel & 0x000000FF) << 16));
			}

            Texture2D texture = null;
            Threading.BlockOnUIThread(() =>
            {
                texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
                texture.SetData<int>(pixels);
            });

            return texture;

#elif DIRECTX
            throw new NotImplementedException();
#elif PSS
            return new Texture2D(graphicsDevice, stream);
#else
            using (Bitmap image = (Bitmap)Bitmap.FromStream(stream))
            {
                // Fix up the Image to match the expected format
                image.RGBToBGR();

                var data = new byte[image.Width * image.Height * 4];

                BitmapData bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                if (bitmapData.Stride != image.Width * 4) throw new NotImplementedException();
                Marshal.Copy(bitmapData.Scan0, data, 0, data.Length);
                image.UnlockBits(bitmapData);

                Texture2D texture = null;
                Threading.BlockOnUIThread(() =>
                {
                    texture = new Texture2D(graphicsDevice, image.Width, image.Height);
                    texture.SetData(data);
                });

                return texture;
            }
#endif
        }

        public void SaveAsJpeg(Stream stream, int width, int height)
        {
#if WINRT
            SaveAsImage(BitmapEncoder.JpegEncoderId, stream, width, height);
#else
            throw new NotImplementedException();
#endif
        }

        public void SaveAsPng(Stream stream, int width, int height)
        {
#if WINRT
            SaveAsImage(BitmapEncoder.PngEncoderId, stream, width, height);
#else
            throw new NotImplementedException();
#endif
        }

#if WINRT
        private void SaveAsImage(Guid encoderId, Stream stream, int width, int height)
        {
            var pixelData = new byte[Width * Height * GraphicsExtensions.Size(Format)];
            GetData(pixelData);

            // TODO: We need to convert from Format to R8G8B8A8!

            // TODO: We should implement async SaveAsPng() for WinRT.
            Task.Run(async () =>
            {
                // Create a temporary memory stream for writing the png.
                var memstream = new InMemoryRandomAccessStream();

                // Write the png.
                var encoder = await BitmapEncoder.CreateAsync(encoderId, memstream);
                encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, (uint)width, (uint)height, 96, 96, pixelData);
                await encoder.FlushAsync();

                // Copy the memory stream into the real output stream.
                memstream.Seek(0);
                memstream.AsStreamForRead().CopyTo(stream);

            }).Wait();
        }
#endif // WINRT

        //What was this for again?
		internal void Reload(Stream textureStream)
		{
		}
	}
}

