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

using System.IO;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content;
using OpenTK.Graphics.OpenGL;
using Path = System.IO.Path;
using PixelType = OpenTK.Graphics.OpenGL.PixelType;


namespace Microsoft.Xna.Framework.Graphics
{
    public class Texture2D : Texture
    {
		private ESImage texture;
		
		// moved to Texture object..  Kenneth J. Pouncey
		//protected int _textureId = -1;
		protected int _width;
		protected int _height;
		private bool _mipmap;
		private byte[] textureData = null;
		
		internal bool IsSpriteFontTexture {get;set;}
		
		// my change
		// --------
		internal uint ID
		{
			get
			{ 
				if (texture == null)
					return (uint)_textureId;
				else
					return texture.Name;
			}
		}
		// --------
		internal ESImage Image
		{
			get 
			{
				return texture;
			}
		}
		
        public Rectangle SourceRect
        {
            get
            {
                return new Rectangle(0,0,_width, _height);
            }
        }
		
		public Rectangle Bounds {
			get {
				return new Rectangle (0,0,_width, _height);
			}
		}		

		internal Texture2D(ESImage theImage)
		{
			texture = theImage;
			_width = texture.ImageWidth;
			_height = texture.ImageHeight;
			_format = texture.Format;
			_textureId = (int)theImage.Name;
		}
		
		public Texture2D(GraphicsDevice graphicsDevice, int width, int height) : 
			this (graphicsDevice, width, height, false, SurfaceFormat.Color)
		{

		}
		
		public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format)
		{
			this.graphicsDevice = graphicsDevice;
			this._width = width;
			this._height = height;
			this._format = format;
			this._mipmap = mipMap;
			
			generateOpenGLTexture();
		}
		
		protected override void DoDisposing(EventArgs e)
		{
			base.DoDisposing(e);
			if (texture != null)
				texture.Dispose();
			texture = null;
		}

        public Color GetPixel(int x, int y)
        {
			var result = new Color(0, 0, 0, 0);
			
			if((x < 0 ) || ( y < 0) )
				return result;
			
			if(x >= Width || y >= Height) 
				return result;
			
			int sz = 0;
			
			byte[] pixel = new byte[4];;
			int pos;
			IntPtr pixelOffset;
			switch(this.Format) 
			{
				case SurfaceFormat.Color /*kTexture2DPixelFormat_RGBA8888*/:
				case SurfaceFormat.Dxt3 :
				    sz = 4;
					pos = ( (y * Width) + x ) * sz;
					pixelOffset = new IntPtr(texture.PixelData.ToInt64() + pos);
					Marshal.Copy(pixelOffset, pixel, 0, 4);	
					result.R = pixel[0];
					result.G = pixel[1];
					result.B = pixel[2];
					result.A = pixel[3];
					break;
				case SurfaceFormat.Bgra4444 /*kTexture2DPixelFormat_RGBA4444*/:
					sz = 2;
					pos = ( (y * Width) + x ) * sz;
					pixelOffset = new IntPtr(texture.PixelData.ToInt64() + pos);					
					Marshal.Copy(pixelOffset, pixel, 0, 4);	
				
					result.R = pixel[0];
					result.G = pixel[1];
					result.B = pixel[2];
					result.A = pixel[3];
					break;
				case SurfaceFormat.Bgra5551 /*kTexture2DPixelFormat_RGB5A1*/:
					sz = 2;
					pos = ( (y * Width) + x ) * sz;
					pixelOffset = new IntPtr(texture.PixelData.ToInt64() + pos);					
					Marshal.Copy(pixelOffset, pixel, 0, 4);	
				
					result.R = pixel[0];
					result.G = pixel[1];
					result.B = pixel[2];
					result.A = pixel[3];
					break;
				case SurfaceFormat.Alpha8 /*kTexture2DPixelFormat_A8*/:
					sz = 1;
					pos = ( (y * Width) + x ) * sz;
					pixelOffset = new IntPtr(texture.PixelData.ToInt64() + pos);
					Marshal.Copy(pixelOffset, pixel, 0, 1);	
				
					result.A = pixel[0];
					break;
				default:
					throw new NotSupportedException("Texture format");
			}
			
			return result;
        }

        public void SetPixel(int x, int y, byte red, byte green, byte blue, byte alpha)
        {
            throw new NotImplementedException();
        }

        public int Width
        {
            get
            {
                return _width;
            }
        }

        public int Height
        {
            get
            {
                return _height;
            }
        }

        public static Texture2D FromFile(GraphicsDevice graphicsDevice, Stream textureStream)
        {
            Bitmap image = (Bitmap)Bitmap.FromStream(textureStream);
			
			if (image == null)			
			{
				throw new ContentLoadException("Error loading Texture2D Stream");
			}
			
			ESImage theTexture = new ESImage(image, graphicsDevice.PreferedFilter);			
			Texture2D result = new Texture2D(theTexture);
			
			return result;
        }

        public static Texture2D FromFile(GraphicsDevice graphicsDevice, Stream textureStream, int numberBytes)
        {
            throw new NotImplementedException();
        }

        public static Texture2D FromFile(GraphicsDevice graphicsDevice, string filename, int width, int height)
        {
            Bitmap image = (Bitmap)Bitmap.FromFile(filename);
			if (image == null)
			{
				throw new ContentLoadException("Error loading file: " + filename);
			}
			
			// TODO resize
			ESImage theTexture = new ESImage(image, graphicsDevice.PreferedFilter);
			Texture2D result = new Texture2D(theTexture);
			result.Name = Path.GetFileNameWithoutExtension(filename);
			return result;
        }

        public static Texture2D FromFile(GraphicsDevice graphicsDevice, string filename)
		{
			return FromFile( graphicsDevice, filename, 0, 0 );
        }
		
		private void generateOpenGLTexture ()
		{
			// modeled after this
			// http://steinsoft.net/index.php?site=Programming/Code%20Snippets/OpenGL/no9
			
			GL.GenTextures(1,out _textureId);
			GL.BindTexture(TextureTarget.Texture2D, _textureId);
			
			if (_mipmap)
			{
				// Taken from http://www.flexicoder.com/blog/index.php/2009/11/iphone-mipmaps/
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapNearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (int)All.True);
			}
			else
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			}
			
			textureData = new byte[(_width * _height) * 4];
			
			GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _width, _height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureData);			
			
			GL.BindTexture(TextureTarget.Texture2D, 0);

			// The following is left here for testing purposes
//			NSImage image = new NSImage (new SizeF (_width, _height));
//			image.LockFocus ();
//			// We set this to be cleared/initialized
//			NSColor.Clear.Set ();
//			NSGraphics.RectFill (new RectangleF (0,0,_width,_height));
//			image.UnlockFocus ();
//
//			if (image == null) {
//				throw new Exception ("Error Creating Texture2D.");
//			}			
//
//			texture = new ESImage (image, graphicsDevice.PreferedFilter);
//			_textureId = (int)texture.Name;
		}		
		
		private void SetData (int index, byte red, byte green, byte blue, byte alpha) 
		{
			switch (_format) {				
			case SurfaceFormat.Color /*kTexture2DPixelFormat_RGBA8888*/:
			case SurfaceFormat.Dxt1:
			case SurfaceFormat.Dxt3:
				index *= 4;
				textureData [index] = red;
				textureData [index + 1] = green;
				textureData [index + 2] = blue;
				textureData [index + 3] = alpha;				
				break;
				
			// TODO: Implement the rest of these but lack of knowledge and examples prevents this for now
			case SurfaceFormat.Bgra4444 /*kTexture2DPixelFormat_RGBA4444*/:
				break;
			case SurfaceFormat.Bgra5551 /*kTexture2DPixelFormat_RGB5A1*/:
				break;
			case SurfaceFormat.Alpha8 /*kTexture2DPixelFormat_A8*/:
				break;
			default:
				throw new NotSupportedException ("Texture format");
				;					
			}
		}
		
		internal void Apply() 
		{
			
			GL.BindTexture (TextureTarget.Texture2D, (uint)_textureId);			
			if (_mipmap)
			{
				// Taken from http://www.flexicoder.com/blog/index.php/2009/11/iphone-mipmaps/
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapNearest);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (int)All.True);
			}
			else
			{
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			}			

			GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _width, _height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, textureData);
		}		
		
		public void SetData<T> (T[] data)
		{
			if (data == null) {
				// we offload to ESImage here
			}
			else {
				// we now have a texture not based on an outside image source
				// now we check what type was passed
				if (typeof(T) == typeof(Color)) {
					int y = 0;
					for (int x = data.Length - 1; x >= 0; x--) {
						var color = (Color)(object)data[x];
						SetData(y++,color.R, color.G, color.B, color.A);
					}
				}
				
				// when we are all done we need apply the changes
				Apply();
			}
		}		
		
		public void SetData<T> (T[] data, int startIndex, int elementCount, SetDataOptions options)
		{
			throw new NotImplementedException ();
		}

		public void SetData<T> (int level, Rectangle? rect, T[] data, int startIndex, int elementCount, SetDataOptions options)
		{
			throw new NotImplementedException ();
		}

		public void GetData<T>(T[] data) where T : struct
		{
			if (typeof(T) == typeof(Color) && texture == null) {
				GL.BindTexture (TextureTarget.Texture2D, _textureId);
				GL.GetTexImage<T> (TextureTarget.Texture2D, 0, PixelFormat.Bgra, PixelType.UnsignedByte, data);

				// OpenGL likes its images bottom-up instead of
				// top-down.  So we must flip it--flip it good.
				var tempRow = new T [Width];
				int indexLowRow = 0;
				int indexHighRow = Width * (Height - 1);
				do {
					Array.Copy (data, indexLowRow, tempRow, 0, Width);
					Array.Copy (data, indexHighRow, data, indexLowRow, Width);
					Array.Copy (tempRow, 0, data, indexHighRow, Width);
					indexLowRow += Width;
					indexHighRow -= Width;
				} while (indexLowRow < indexHighRow);
			} else {
				throw new NotImplementedException();
			}
			// TODO Causese AV on Device, but not simulator GetData<T>(0, null, data, 0, Width * Height);
//			
//			if (data == null )
//			{
//				throw new ArgumentException("data cannot be null");
//			}
//			
//			int sz = 0;
//						
//			byte[] pixel = new byte[4];
//			int pos;
//			IntPtr pixelOffset;
//			// Get the Color values
//			if ((typeof(T) == typeof(Color))) 
//			{	
//				// Load up texture into memory
//				NSImage nsImage = NSImage.ImageNamed(this.Name);
//				if (nsImage == null)
//				{
//					throw new ContentLoadException("Error loading file via UIImage: " + Name);
//				}
//				
//				CGImage image = nsImage.AsCGImage(RectangleF.Empty, null, null);
//				if (image == null)
//				{
//					throw new ContentLoadException("Error with CGIamge: " + Name);
//				}
//				
//				int	width,height,i;
//		        CGContext context = null;
//		        IntPtr imageData;
//		        CGColorSpace colorSpace;
//		        IntPtr tempData;
//		        bool hasAlpha;
//		        CGImageAlphaInfo info;
//		        CGAffineTransform transform;
//		        Size imageSize;
//		        SurfaceFormat pixelFormat;
//		        bool sizeToFit = false;
//				
//				info = image.AlphaInfo;
//				hasAlpha = ((info == CGImageAlphaInfo.PremultipliedLast) || (info == CGImageAlphaInfo.PremultipliedFirst) || (info == CGImageAlphaInfo.Last) || (info == CGImageAlphaInfo.First) ? true : false);
//				
//				if (image.ColorSpace != null)
//				{
//					pixelFormat = SurfaceFormat.Color;
//				}
//				else 
//				{	
//					pixelFormat = SurfaceFormat.Alpha8;
//				}
//		
//				imageSize = new Size(image.Width,image.Height);
//				transform = CGAffineTransform.MakeIdentity();
//				width = imageSize.Width;
//		
//				if((width != 1) && ((width & (width - 1))!=0)) {
//					i = 1;
//					while((sizeToFit ? 2 * i : i) < width)
//						i *= 2;
//					width = i;
//				}
//				height = imageSize.Height;
//				if((height != 1) && ((height & (height - 1))!=0)) {
//					i = 1;
//					while((sizeToFit ? 2 * i : i) < height)
//						i *= 2;
//					height = i;
//				}
//				// TODO: kMaxTextureSize = 1024
//				while((width > 1024) || (height > 1024)) 
//				{
//					width /= 2;
//					height /= 2;
//					transform = CGAffineTransform.MakeScale(0.5f,0.5f);
//					imageSize.Width /= 2;
//					imageSize.Height /= 2;
//				}
//				
//				switch(pixelFormat) 
//				{		
//					case SurfaceFormat.Color:
//						colorSpace = CGColorSpace.CreateDeviceRGB();
//						imageData = Marshal.AllocHGlobal(height * width * 4);
//						context = new CGBitmapContext(imageData, width, height, 8, 4 * width, colorSpace,CGImageAlphaInfo.PremultipliedLast);
//						colorSpace.Dispose();
//						break;		
//					case SurfaceFormat.Alpha8:
//						imageData = Marshal.AllocHGlobal(height * width);
//						context = new CGBitmapContext(imageData, width, height, 8, width, null, CGImageAlphaInfo.Only);
//						break;				
//					default:
//						throw new NotSupportedException("Invalid pixel format"); 
//				}
//					
//				context.ClearRect(new RectangleF(0,0,width,height));
//	 			context.TranslateCTM(0, height - imageSize.Height);
//				
//				if (!transform.IsIdentity)
//				{
//					context.ConcatCTM(transform);
//				}
//				
//				context.DrawImage(new RectangleF(0, 0, image.Width, image.Height), image);
//				
//				//Convert "RRRRRRRRRGGGGGGGGBBBBBBBBAAAAAAAA" to "RRRRRGGGGGGBBBBB"
//				/*
//				if(pixelFormat == SurfaceFormat.Rgb32) {
//					tempData = Marshal.AllocHGlobal(height * width * 2);
//					
//					int d32;
//					short d16;
//					int inPixel32Count=0,outPixel16Count=0;
//					for(i = 0; i < width * height; ++i, inPixel32Count+=sizeof(int))
//					{
//						d32 = Marshal.ReadInt32(imageData,inPixel32Count);
//						short R = (short)((((d32 >> 0) & 0xFF) >> 3) << 11);
//						short G = (short)((((d32 >> 8) & 0xFF) >> 2) << 5);
//						short B = (short)((((d32 >> 16) & 0xFF) >> 3) << 0);
//						d16 = (short)  (R | G | B);
//						Marshal.WriteInt16(tempData,outPixel16Count,d16);
//						outPixel16Count += sizeof(short);
//					}
//					Marshal.FreeHGlobal(imageData);
//					imageData = tempData;			
//				}									
//				*/
//				
//				// Loop through and extract the data
//				for(int y = 0; y < imageSize.Height; y++ )
//				{
//					for( int x = 0; x < imageSize.Width; x++ )
//					{
//						var result = new Color(0, 0, 0, 0);						
//						
//						switch(pixelFormat) 
//						{
//							case SurfaceFormat.Color : //kTexture2DPixelFormat_RGBA8888
//							case SurfaceFormat.Dxt3 :
//							    sz = 4;
//								pos = ( (y * imageSize.Width) + x ) * sz;								
//								pixelOffset = new IntPtr(imageData.ToInt64() + pos);							
//								Marshal.Copy(pixelOffset, pixel, 0, 4);	
//								result.R = pixel[0];
//								result.G = pixel[1];
//								result.B = pixel[2];
//								result.A = pixel[3];
//								break;
//							case SurfaceFormat.Bgra4444 : //kTexture2DPixelFormat_RGBA4444
//								sz = 2;
//								pos = ( (y * imageSize.Width) + x ) * sz;
//								pixelOffset = new IntPtr(imageData.ToInt64() + pos);
//
//								Marshal.Copy(pixelOffset, pixel, 0, 4);	
//							
//								result.R = pixel[0];
//								result.G = pixel[1];
//								result.B = pixel[2];
//								result.A = pixel[3];
//								break;
//							case SurfaceFormat.Bgra5551 : //kTexture2DPixelFormat_RGB5A1
//								sz = 2;
//								pos = ( (y * imageSize.Width) + x ) * sz;
//								pixelOffset = new IntPtr(imageData.ToInt64() + pos);
//								Marshal.Copy(pixelOffset, pixel, 0, 4);	
//							
//								result.R = pixel[0];
//								result.G = pixel[1];
//								result.B = pixel[2];
//								result.A = pixel[3];
//								break;
//							case SurfaceFormat.Alpha8 :  // kTexture2DPixelFormat_A8 
//								sz = 1;
//								pos = ( (y * imageSize.Width) + x ) * sz;
//								pixelOffset = new IntPtr(imageData.ToInt64() + pos);								
//								Marshal.Copy(pixelOffset, pixel, 0, 4);	
//							
//								result.A = pixel[0];
//								break;
//							default:
//								throw new NotSupportedException("Texture format");
//						}
//						data[((y * imageSize.Width) + x)] = (T)(object)result;
//					}
//				}
//								
//				context.Dispose();
//				Marshal.FreeHGlobal(imageData);	
//			}
        }

        public void GetData<T>(T[] data, int startIndex, int elementCount)
        {
            GetData<T>(0, null, data, startIndex, elementCount);
        }

        public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
			throw new NotImplementedException();
			
//            if (data == null )
//			{
//				throw new ArgumentException("data cannot be null");
//			}
//			
//			if (data.Length < startIndex + elementCount)
//			{
//				throw new ArgumentException("The data passed has a length of " + data.Length + " but " + elementCount + " pixels have been requested.");
//			}
//			
//			Rectangle r;
//			if (rect != null)
//			{
//				r = rect.Value;
//			}
//			else
//			{
//				r = new Rectangle(0, 0, Width, Height);
//			}
//			
//			int sz = 0;
//						
//			byte[] pixel = new byte[4];
//			int pos;
//			IntPtr pixelOffset;
//			// Get the Color values
//			if ((typeof(T) == typeof(Color))) 
//			{	
//				// Load up texture into memory
//				NSImage nsImage = NSImage.ImageNamed(this.Name);
//				if (nsImage == null)
//				{
//					throw new ContentLoadException("Error loading file via UIImage: " + Name);
//				}
//				
//				CGImage image = nsImage.AsCGImage(RectangleF.Empty, null, null);
//				if (image == null)
//				{
//					throw new ContentLoadException("Error with CGIamge: " + Name);
//				}
//				
//				int	width,height,i;
//		        CGContext context = null;
//		        IntPtr imageData;
//		        CGColorSpace colorSpace;
//		        IntPtr tempData;
//		        bool hasAlpha;
//		        CGImageAlphaInfo info;
//		        CGAffineTransform transform;
//		        Size imageSize;
//		        SurfaceFormat pixelFormat;
//		        bool sizeToFit = false;
//				
//				info = image.AlphaInfo;
//				hasAlpha = ((info == CGImageAlphaInfo.PremultipliedLast) || (info == CGImageAlphaInfo.PremultipliedFirst) || (info == CGImageAlphaInfo.Last) || (info == CGImageAlphaInfo.First) ? true : false);
//				
//				if (image.ColorSpace != null)
//				{
//					pixelFormat = SurfaceFormat.Color;
//				}
//				else 
//				{	
//					pixelFormat = SurfaceFormat.Alpha8;
//				}
//		
//				imageSize = new Size(image.Width,image.Height);
//				transform = CGAffineTransform.MakeIdentity();
//				width = imageSize.Width;
//		
//				if((width != 1) && ((width & (width - 1))!=0)) {
//					i = 1;
//					while((sizeToFit ? 2 * i : i) < width)
//						i *= 2;
//					width = i;
//				}
//				height = imageSize.Height;
//				if((height != 1) && ((height & (height - 1))!=0)) {
//					i = 1;
//					while((sizeToFit ? 2 * i : i) < height)
//						i *= 2;
//					height = i;
//				}
//				// TODO: kMaxTextureSize = 1024
//				while((width > 1024) || (height > 1024)) 
//				{
//					width /= 2;
//					height /= 2;
//					transform = CGAffineTransform.MakeScale(0.5f,0.5f);
//					imageSize.Width /= 2;
//					imageSize.Height /= 2;
//				}
//				
//				switch(pixelFormat) 
//				{		
//					case SurfaceFormat.Color:
//						colorSpace = CGColorSpace.CreateDeviceRGB();
//						imageData = Marshal.AllocHGlobal(height * width * 4);
//						context = new CGBitmapContext(imageData, width, height, 8, 4 * width, colorSpace,CGImageAlphaInfo.PremultipliedLast);
//						colorSpace.Dispose();
//						break;
//					case SurfaceFormat.Alpha8:
//						imageData = Marshal.AllocHGlobal(height * width);
//						context = new CGBitmapContext(imageData, width, height, 8, width, null, CGImageAlphaInfo.Only);
//						break;				
//					default:
//						throw new NotSupportedException("Invalid pixel format"); 
//				}
//					
//				context.ClearRect(new RectangleF(0,0,width,height));
//	 			context.TranslateCTM(0, height - imageSize.Height);
//				
//				if (!transform.IsIdentity)
//				{
//					context.ConcatCTM(transform);
//				}
//				
//				context.DrawImage(new RectangleF(0, 0, image.Width, image.Height), image);
//				
//				//Convert "RRRRRRRRRGGGGGGGGBBBBBBBBAAAAAAAA" to "RRRRRGGGGGGBBBBB"
//				/*
//				if(pixelFormat == SurfaceFormat.Rgb32) {
//					tempData = Marshal.AllocHGlobal(height * width * 2);
//					
//					int d32;
//					short d16;
//					int inPixel32Count=0,outPixel16Count=0;
//					for(i = 0; i < width * height; ++i, inPixel32Count+=sizeof(int))
//					{
//						d32 = Marshal.ReadInt32(imageData,inPixel32Count);
//						short R = (short)((((d32 >> 0) & 0xFF) >> 3) << 11);
//						short G = (short)((((d32 >> 8) & 0xFF) >> 2) << 5);
//						short B = (short)((((d32 >> 16) & 0xFF) >> 3) << 0);
//						d16 = (short)  (R | G | B);
//						Marshal.WriteInt16(tempData,outPixel16Count,d16);
//						outPixel16Count += sizeof(short);
//					}
//					Marshal.FreeHGlobal(imageData);
//					imageData = tempData;			
//				}									
//				*/
//				
//				int count = 0;
//				
//				// Loop through and extract the data
//				for(int y = r.Top; y < r.Bottom; y++ )
//				{
//					for( int x = r.Left; x < r.Right; x++ )
//					{
//						var result = new Color(0, 0, 0, 0);						
//						
//						switch(this.Format) 
//						{
//							case SurfaceFormat.Color /*kTexture2DPixelFormat_RGBA8888*/:
//							case SurfaceFormat.Dxt3 :
//							    sz = 4;
//								pos = ( (y * imageSize.Width) + x ) * sz;								
//								pixelOffset = new IntPtr(imageData.ToInt64() + pos);							
//								Marshal.Copy(pixelOffset, pixel, 0, 4);	
//								result.R = pixel[0];
//								result.G = pixel[1];
//								result.B = pixel[2];
//								result.A = pixel[3];
//								break;
//							case SurfaceFormat.Bgra4444 /*kTexture2DPixelFormat_RGBA4444*/:
//								sz = 2;
//								pos = ( (y * imageSize.Width) + x ) * sz;
//								pixelOffset = new IntPtr(imageData.ToInt64() + pos);
//
//								Marshal.Copy(pixelOffset, pixel, 0, 4);	
//							
//								result.R = pixel[0];
//								result.G = pixel[1];
//								result.B = pixel[2];
//								result.A = pixel[3];
//								break;
//							case SurfaceFormat.Bgra5551 /*kTexture2DPixelFormat_RGB5A1*/:
//								sz = 2;
//								pos = ( (y * imageSize.Width) + x ) * sz;
//								pixelOffset = new IntPtr(imageData.ToInt64() + pos);
//								Marshal.Copy(pixelOffset, pixel, 0, 4);	
//							
//								result.R = pixel[0];
//								result.G = pixel[1];
//								result.B = pixel[2];
//								result.A = pixel[3];
//								break;
//							case SurfaceFormat.Alpha8 /*kTexture2DPixelFormat_A8*/:
//								sz = 1;
//								pos = ( (y * imageSize.Width) + x ) * sz;
//								pixelOffset = new IntPtr(imageData.ToInt64() + pos);								
//								Marshal.Copy(pixelOffset, pixel, 0, 4);	
//							
//								result.A = pixel[0];
//								break;
//							default:
//								throw new NotSupportedException("Texture format");
//						}
//						data[((y * imageSize.Width) + x)] = (T)(object)result;
//						
//						count++;
//						if (count >= elementCount) 
//							return;
//					}
//				}
//								
//				context.Dispose();
//				Marshal.FreeHGlobal(imageData);	
//			}	
//			else
//			{
//				throw new NotImplementedException();
//			}

           
		}

        internal void Reload(Stream Stream)
        {
        }
	}
}

