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
#if !PSM
using System.Drawing;
#else
using Sce.PlayStation.Core.Graphics;
using Sce.PlayStation.Core.Imaging;
#endif
using System.IO;
using System.Runtime.InteropServices;

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
#elif IOS
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
#endif

#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
using GLPixelFormat = MonoMac.OpenGL.PixelFormat;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
using GLPixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
#elif GLES
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
#elif PSM
using PssTexture2D = Sce.PlayStation.Core.Graphics.Texture2D;
#endif

#if WINDOWS || LINUX || MONOMAC
using System.Drawing.Imaging;
#endif
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;

#if WINRT
#if WINDOWS_PHONE
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
#else
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
#endif
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
        internal protected enum SurfaceType
        {
            Texture,
            RenderTarget,
            SwapChainRenderTarget,
        }

		internal int width;
		internal int height;

#if PSM
		internal PssTexture2D _texture2D;

#elif OPENGL
		PixelInternalFormat glInternalFormat;
		GLPixelFormat glFormat;
		PixelType glType;
#endif

#if DIRECTX
        private bool _shared;

        private bool _renderTarget;
        private bool _mipmap;
#endif

        public Rectangle Bounds
        {
            get
            {
				return new Rectangle(0, 0, this.width, this.height);
            }
        }

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
            : this(graphicsDevice, width, height, false, SurfaceFormat.Color, SurfaceType.Texture, false)
        {
        }

        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
            : this(graphicsDevice, width, height, mipmap, format, SurfaceType.Texture, false)
        {
        }

        protected Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type)
            : this(graphicsDevice, width, height, mipmap, format, type, false)
        {
        }

        protected Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared)
		{
            if (graphicsDevice == null)
                throw new ArgumentNullException("Graphics Device Cannot Be Null");

            this.GraphicsDevice = graphicsDevice;
            this.width = width;
            this.height = height;
            this._format = format;
            this._levelCount = mipmap ? CalculateMipLevels(width, height) : 1;

            // Texture will be assigned by the swap chain.
		    if (type == SurfaceType.SwapChainRenderTarget)
		        return;

#if DIRECTX
            _shared = shared;

            _renderTarget = (type == SurfaceType.RenderTarget);
            _mipmap = mipmap;

            // Create texture
            GetTexture();

#elif PSM
            PixelBufferOption option = PixelBufferOption.None;
            if (type == SurfaceType.RenderTarget)
			    option = PixelBufferOption.Renderable;
            _texture2D = new Sce.PlayStation.Core.Graphics.Texture2D(width, height, mipmap, PSSHelper.ToFormat(format),option);
#else

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
#endif
        }

#if PSM
        private Texture2D(GraphicsDevice graphicsDevice, Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            _texture2D = new PssTexture2D(bytes, false);
            width = _texture2D.Width;
            height = _texture2D.Height;
            this._format = SurfaceFormat.Color; //FIXME HACK
            this._levelCount = 1;
        }
#endif			

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
#if !PSM
                var elementSizeInByte = Marshal.SizeOf(typeof(T));
                var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                // Use try..finally to make sure dataHandle is freed in case of an error
                try
                {
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
#if DIRECTX
                            w = (w + 3) & ~3;
                            h = (h + 3) & ~3;
#else
                            if (w > 4)
                                w = (w + 3) & ~3;
                            if (h > 4)
                                h = (h + 3) & ~3;
#endif
                        }
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
                    var d3dContext = GraphicsDevice._d3dContext;
                    lock (d3dContext)
						d3dContext.UpdateSubresource(box, GetTexture(), level, region);

#elif PSM
                    _texture2D.SetPixels(level, data, _texture2D.Format, startIndex, 0, x, y, w, h);
#elif OPENGL

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
#if GLES
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

#endif // OPENGL

#if !PSM
                }
                finally
                {
                    dataHandle.Free();
                }
#endif

#if OPENGL
#if !ANDROID
                // Required to make sure that any texture uploads on a thread are completed
                // before the main thread tries to use the texture.
                GL.Finish();
#endif
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
            if (data == null || data.Length == 0)
                throw new ArgumentException("data cannot be null");
            if (data.Length < startIndex + elementCount)
                throw new ArgumentException("The data passed has a length of " + data.Length + " but " + elementCount + " pixels have been requested.");

#if IOS

            // Reading back a texture from GPU memory is unsupported
            // in OpenGL ES 2.0 and no work around has been implemented.           
            throw new NotSupportedException("OpenGL ES 2.0 does not support texture reads.");

#elif ANDROID

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
#elif PSM
            Rectangle r;
            if (rect.HasValue)
            {
                r = rect.Value;
            }
            else
            {
                r = new Rectangle(0, 0, Width, Height);
            }
            
            int rWidth = r.Width;
            int rHeight = r.Height;
            
            var sz = 4;         
            
            // Loop through and extract the data but we need to load it 
            var dataRowColOffset = 0;
            
            var pixelOffset = 0;
            var result = new Color(0, 0, 0, 0);
            
            byte[] imageInfo = new byte[(rWidth * rHeight) * sz];
            
            ImageRect old_scissor = GraphicsDevice.Context.GetScissor();
            ImageRect old_viewport = GraphicsDevice.Context.GetViewport();
            FrameBuffer old_frame_buffer = GraphicsDevice.Context.GetFrameBuffer();

            ColorBuffer color_buffer = new ColorBuffer(rWidth, rHeight, PixelFormat.Rgba);
            FrameBuffer frame_buffer = new FrameBuffer();
            frame_buffer.SetColorTarget(color_buffer);
             
            GraphicsDevice.Context.SetFrameBuffer(frame_buffer);

            GraphicsDevice.Context.SetTexture(0, this._texture2D);
            GraphicsDevice.Context.ReadPixels(imageInfo, PixelFormat.Rgba, 0, 0, rWidth, rHeight);

            GraphicsDevice.Context.SetFrameBuffer(old_frame_buffer);
            GraphicsDevice.Context.SetScissor(old_scissor);
            GraphicsDevice.Context.SetViewport(old_viewport);
            
            for (int y = r.Top; y < rHeight; y++)
            {
                for (int x = r.Left; x < rWidth; x++)
                {
                    dataRowColOffset = ((y * r.Width) + x);
                    
                    pixelOffset = dataRowColOffset * sz;
                    result.R = imageInfo[pixelOffset];
                    result.G = imageInfo[pixelOffset + 1];
                    result.B = imageInfo[pixelOffset + 2];
                    result.A = imageInfo[pixelOffset + 3];
                    
                    data[dataRowColOffset] = (T)(object)result;
                }
            }

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
            desc.Format = SharpDXHelper.ToFormat(_format);
            desc.BindFlags = SharpDX.Direct3D11.BindFlags.None;
            desc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.Read;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;
            desc.Usage = SharpDX.Direct3D11.ResourceUsage.Staging;
            desc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;

		    var d3dContext = GraphicsDevice._d3dContext;
            using (var stagingTex = new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, desc))
                lock (d3dContext)
                {
                    // Copy the data from the GPU to the staging texture.
                    if (rect.HasValue)
                    {
                        d3dContext.CopySubresourceRegion(GetTexture(), level, new SharpDX.Direct3D11.ResourceRegion(rect.Value.Left,rect.Value.Top,0, rect.Value.Right, rect.Value.Bottom, 0), stagingTex, 0, 0, 0, 0);
                    }
                    else
                        d3dContext.CopySubresourceRegion(GetTexture(), level, null, stagingTex, 0, 0, 0, 0);

                    // Copy the data to the array.
                    SharpDX.DataStream stream;
                    d3dContext.MapSubresource(stagingTex, 0, SharpDX.Direct3D11.MapMode.Read, SharpDX.Direct3D11.MapFlags.None, out stream);
                    stream.ReadRange(data, startIndex, elementCount);
                    stream.Dispose();
                }

#else

			GL.BindTexture(TextureTarget.Texture2D, this.glTexture);

			if (glFormat == (GLPixelFormat)All.CompressedTextureFormats) {
				throw new NotImplementedException();
			} else {
				if (rect.HasValue) {
					var temp = new T[this.width*this.height];
					GL.GetTexImage(TextureTarget.Texture2D, level, this.glFormat, this.glType, temp);
					int z = 0, w = 0;

					for(int y= rect.Value.Y; y < rect.Value.Y+ rect.Value.Height; y++) {
						for(int x=rect.Value.X; x < rect.Value.X + rect.Value.Width; x++) {
							data[z*rect.Value.Width+w] = temp[(y*width)+x];
							w++;
						}
						z++;
					}
				} else {
					GL.GetTexImage(TextureTarget.Texture2D, level, this.glFormat, this.glType, data);
				}
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
#elif ANDROID
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
            }
#elif WINDOWS_PHONE
            throw new NotImplementedException();

#elif WINDOWS_STOREAPP || DIRECTX

            // For reference this implementation was ultimately found through this post:
            // http://stackoverflow.com/questions/9602102/loading-textures-with-sharpdx-in-metro 
            Texture2D toReturn = null;
			SharpDX.WIC.BitmapDecoder decoder;
			
            using(var bitmap = LoadBitmap(stream, out decoder))
			using (decoder)
			{
				SharpDX.Direct3D11.Texture2D sharpDxTexture = CreateTex2DFromBitmap(bitmap, graphicsDevice);

				toReturn = new Texture2D(graphicsDevice, bitmap.Size.Width, bitmap.Size.Height);

				toReturn._texture = sharpDxTexture;
			}
            return toReturn;
#elif PSM
            return new Texture2D(graphicsDevice, stream);
#else
            using (Bitmap image = (Bitmap)Bitmap.FromStream(stream))
            {
                // Fix up the Image to match the expected format
                image.RGBToBGR();

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
                for (int i = 0; i < width * height; ++i)
                {
                    uint pixel = (uint)pixels[i];
                    pixels[i] = (int)((pixel & 0xFF00FF00) | ((pixel & 0x00FF0000) >> 16) | ((pixel & 0x000000FF) << 16));
                }

                this.SetData<int>(pixels);
                image.Recycle();
            }
#endif
        }

        public void SaveAsJpeg(Stream stream, int width, int height)
        {
#if WINDOWS_STOREAPP
            SaveAsImage(BitmapEncoder.JpegEncoderId, stream, width, height);
#elif WINDOWS_PHONE

            var pixelData = new byte[Width * Height * GraphicsExtensions.Size(Format)];
            GetData(pixelData);

            //We Must convert from BGRA to RGBA
            ConvertToRGBA(height, width, pixelData);

            var waitEvent = new ManualResetEventSlim(false);
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var bitmap = new WriteableBitmap(width, height);
                System.Buffer.BlockCopy(pixelData, 0, bitmap.Pixels, 0, pixelData.Length);
                bitmap.SaveJpeg(stream, width, height, 0, 100);
                waitEvent.Set();
            });

            waitEvent.Wait();
#elif MONOMAC
			SaveAsImage(stream, width, height, ImageFormat.Jpeg);
#else
            throw new NotImplementedException();
#endif
        }

        //Converts Pixel Data from BGRA to RGBA
        private static void ConvertToRGBA(int pixelHeight, int pixelWidth, byte[] pixels)
        {
            int offset = 0;

            for (int row = 0; row < (uint)pixelHeight; row++)
            {
                for (int col = 0; col < (uint)pixelWidth; col++)
                {
                    offset = (row * (int)pixelWidth * 4) + (col * 4);

                    byte B = pixels[offset];
                    byte R = pixels[offset + 2];

                    pixels[offset] = R;
                    pixels[offset + 2] = B;
                }
            }
        }

        public void SaveAsPng(Stream stream, int width, int height)
        {
#if WINDOWS_STOREAPP
            SaveAsImage(BitmapEncoder.PngEncoderId, stream, width, height);
#elif MONOMAC
			SaveAsImage(stream, width, height, ImageFormat.Png);
#else
            // TODO: We need to find a simple stand alone
            // PNG encoder if we want to support this.
            throw new NotImplementedException();
#endif
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

#if WINDOWS_STOREAPP

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
#endif
#if DIRECTX && !WINDOWS_PHONE
		
        [CLSCompliant(false)]
        public static SharpDX.Direct3D11.Texture2D CreateTex2DFromBitmap(SharpDX.WIC.BitmapSource bsource, GraphicsDevice device)
        {

            SharpDX.Direct3D11.Texture2DDescription desc;
            desc.Width = bsource.Size.Width;
            desc.Height = bsource.Size.Height;
            desc.ArraySize = 1;
            desc.BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource;
            desc.Usage = SharpDX.Direct3D11.ResourceUsage.Default;
            desc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None;
            desc.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
            desc.MipLevels = 1;
            desc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;

			SharpDX.Direct3D11.Texture2D dx11Texture;
			
            using(SharpDX.DataStream s = new SharpDX.DataStream(bsource.Size.Height * bsource.Size.Width * 4, true, true))
			{
				bsource.CopyPixels(bsource.Size.Width * 4, s);

				SharpDX.DataRectangle rect = new SharpDX.DataRectangle(s.DataPointer, bsource.Size.Width * 4);

				dx11Texture = new SharpDX.Direct3D11.Texture2D(device._d3dDevice, desc, rect);
			}
            
			return dx11Texture;
        }

        static SharpDX.WIC.ImagingFactory imgfactory = null;
        private static SharpDX.WIC.BitmapSource LoadBitmap(Stream stream, out SharpDX.WIC.BitmapDecoder decoder)
        {
            if (imgfactory == null)
            {
                imgfactory = new SharpDX.WIC.ImagingFactory();
            }
			
			SharpDX.WIC.FormatConverter fconv = null;
			
            decoder = new SharpDX.WIC.BitmapDecoder(
                imgfactory,
                stream,
                SharpDX.WIC.DecodeOptions.CacheOnDemand
                );

			fconv = new SharpDX.WIC.FormatConverter(imgfactory);

			fconv.Initialize(
				decoder.GetFrame(0),
				SharpDX.WIC.PixelFormat.Format32bppPRGBA,
				SharpDX.WIC.BitmapDitherType.None, null,
				0.0, SharpDX.WIC.BitmapPaletteType.Custom);

			return fconv;
        }
		
		
#endif // WINRT

        // This method allows games that use Texture2D.FromStream 
        // to reload their textures after the GL context is lost.
        public void Reload(Stream textureStream)
        {
#if OPENGL
            GenerateGLTextureIfRequired();
            FillTextureFromStream(textureStream);
#endif
}

#if OPENGL
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
#endif

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

#if DIRECTX

        internal override SharpDX.Direct3D11.Resource CreateTexture()
		{
            // TODO: Move this to SetData() if we want to make Immutable textures!
            var desc = new SharpDX.Direct3D11.Texture2DDescription();
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = _levelCount;
            desc.ArraySize = 1;
            desc.Format = SharpDXHelper.ToFormat(_format);
            desc.BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource;
            desc.CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None;
            desc.SampleDescription.Count = 1;
            desc.SampleDescription.Quality = 0;
            desc.Usage = SharpDX.Direct3D11.ResourceUsage.Default;
            desc.OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None;

            if (_renderTarget)
            {
                desc.BindFlags |= SharpDX.Direct3D11.BindFlags.RenderTarget;
                if (_mipmap)
                {
                    // Note: XNA 4 does not have a method Texture.GenerateMipMaps() 
                    // because generation of mipmaps is not supported on the Xbox 360.
                    // TODO: New method Texture.GenerateMipMaps() required.
                    desc.OptionFlags |= SharpDX.Direct3D11.ResourceOptionFlags.GenerateMipMaps;
                }
            }
            if (_shared)
                desc.OptionFlags |= SharpDX.Direct3D11.ResourceOptionFlags.Shared;

            return new SharpDX.Direct3D11.Texture2D(GraphicsDevice._d3dDevice, desc);
        }

#endif
	}
}

