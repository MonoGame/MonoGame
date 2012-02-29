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
using System.Drawing;
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
#else
using OpenTK.Graphics.ES20;
using GLPixelFormat = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
using TextureParameterName = OpenTK.Graphics.ES20.All;
using TextureMinFilter = OpenTK.Graphics.ES20.All;
using PixelInternalFormat = OpenTK.Graphics.ES20.All;
using PixelType = OpenTK.Graphics.ES20.All;
#endif

using Microsoft.Xna.Framework.Content;

#if ANDROID
using Android.Graphics;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    public class Texture2D : Texture
    {
		protected int width;
		protected int height;

		PixelInternalFormat glInternalFormat;
		GLPixelFormat glFormat;
		PixelType glType;
		
        public Rectangle Bounds
        {
            get
            {
				return new Rectangle(0, 0, this.width, this.height);
            }
        }
		
		public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format)
		{
			if (graphicsDevice == null)
            {
                throw new ArgumentNullException("Graphics Device Cannot Be Null");
            }
			this.graphicsDevice = graphicsDevice;
			this.width = width;
			this.height = height;
			this.format = format;
			this.levelCount = 1;

			this.glTarget = TextureTarget.Texture2D;
			
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
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS,	(int)wrap);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)wrap);
			
			format.GetGLFormat(out glInternalFormat, out glFormat, out glType);

			if (glFormat == (GLPixelFormat)All.CompressedTextureFormats)
			{
				var imageSize = 0;
				switch (format) {
				case SurfaceFormat.RgbPvrtc2Bpp:
				case SurfaceFormat.RgbaPvrtc2Bpp:
					imageSize = (Math.Max(this.width, 8) * Math.Max(this.height, 8) * 2 + 7) / 8;
					break;
				case SurfaceFormat.RgbPvrtc4Bpp:
				case SurfaceFormat.RgbaPvrtc4Bpp:
					imageSize = (Math.Max(this.width, 16) * Math.Max(this.height, 8) * 4 + 7) / 8;
					break;
				case SurfaceFormat.Dxt1:
					imageSize = ((this.width+3)/4)*((this.height+3)/4)*8 * 1;
					break;
				case SurfaceFormat.Dxt3:
				case SurfaceFormat.Dxt5:
					imageSize = ((this.width+3)/4)*((this.height+3)/4)*16 * 1;
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
			
			if (mipmap)
			{
#if IPHONE || ANDROID
				GL.GenerateMipmap(TextureTarget.Texture2D);
#else
				GL.TexParameter (TextureTarget.Texture2D,
				                 TextureParameterName.GenerateMipmap,
				                 (int)All.True);
#endif
				
				int size = Math.Max(this.width, this.height);
				while (size > 1)
				{
					size = size / 2;
					this.levelCount++;
				}
			}
		}
				
		public Texture2D(GraphicsDevice graphicsDevice, int width, int height) : 
			this(graphicsDevice, width, height, false, SurfaceFormat.Color)
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
            if (data == null) throw new ArgumentNullException("data");

            var elementSizeInByte = Marshal.SizeOf(typeof(T));
			var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
			var startBytes = startIndex * elementSizeInByte;
			var dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startBytes);
			
			var xOffset = 0;
			var yOffset = 0;
			var width = this.width;
			var height = this.height;
			
			if (rect.HasValue)
			{
				xOffset = rect.Value.X;
				yOffset = rect.Value.Y;
				width = rect.Value.Width;
				height = rect.Value.Height;
			}

            GL.BindTexture(TextureTarget.Texture2D, this.glTexture);
			
			if (glFormat == (GLPixelFormat)All.CompressedTextureFormats)
			{
				GL.CompressedTexSubImage2D(TextureTarget.Texture2D, level,
				                           0, 0, width, height,
				                           (GLPixelFormat)glInternalFormat, data.Length-startBytes, dataPtr);
			}
			else
            	GL.TexSubImage2D(TextureTarget.Texture2D, level,
			                 xOffset, yOffset, width, height,
			                 glFormat, glType, dataPtr);
			
			dataHandle.Free();
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
				
				var texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);			
				var data = new byte[texture.width * texture.height * 4];
				
				var colorSpace = CGColorSpace.CreateDeviceRGB();
				var bitmapContext = new CGBitmapContext(data, texture.width, texture.height, 8, texture.width * 4, colorSpace, CGBitmapFlags.PremultipliedLast);
				bitmapContext.DrawImage(new RectangleF(0, 0, texture.width, texture.height), cgImage);
				bitmapContext.Dispose();
				colorSpace.Dispose();
				
				texture.SetData(data);
			
				return texture;
			}
#elif ANDROID
            Bitmap image = BitmapFactory.DecodeStream(stream);

            var width = image.Width;
            var height = image.Height;
#if ES11
            //scale up bitmap to be power of 2 dimensions but dont exceed 1024x1024.
            //Note: may not have to do this with OpenGL 2+
            width = (int)Math.Pow(2, Math.Min(10, Math.Ceiling(Math.Log10(imageSource.Width) / Math.Log10(2))));
            height = (int)Math.Pow(2, Math.Min(10, Math.Ceiling(Math.Log10(imageSource.Height) / Math.Log10(2))));
#endif
            var texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);

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

			texture.SetData<int>(pixels);

            return texture;
#else
            using (Bitmap image = (Bitmap)Bitmap.FromStream(stream))
            {
                // Fix up the Image to match the expected format
                image.RGBToBGR();

                Texture2D texture = new Texture2D(graphicsDevice, image.Width, image.Height);
                var data = new byte[texture.Width * texture.Height * 4];

                BitmapData bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                if (bitmapData.Stride != image.Width * 4) throw new NotImplementedException();
                Marshal.Copy(bitmapData.Scan0, data, 0, data.Length);
                image.UnlockBits(bitmapData);

                texture.SetData(data);

                return texture;
            }
#endif
        }

        public void SaveAsJpeg(Stream stream, int width, int height)
        {
            throw new NotImplementedException();
        }

        //What was this for again?
		internal void Reload(Stream textureStream)
		{
		}
	}
}

