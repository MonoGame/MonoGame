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

using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;

using MonoMac.OpenGL;

using Microsoft.Xna.Framework.Content;

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

		internal bool IsSpriteFontTexture { get; set; }

		// my change
		// --------
		internal uint ID {
			get { 
				if (texture == null)
					return (uint)_textureId;
				else
					return texture.Name;
			}
		}
		// --------
		internal ESImage Image {
			get {
				return texture;
			}
		}

		public Rectangle SourceRect {
			get {
				return new Rectangle (0, 0, _width, _height);
			}
		}

		public Rectangle Bounds {
			get {
				return new Rectangle (0, 0, _width, _height);
			}
		}

		internal Texture2D (ESImage theImage)
		{
			texture = theImage;
			_width = texture.ImageWidth;
			_height = texture.ImageHeight;
			_format = texture.Format;
			_textureId = (int)theImage.Name;
		}

		public Texture2D (GraphicsDevice graphicsDevice, int width, int height) : 
			this (graphicsDevice, width, height, false, SurfaceFormat.Color)
		{

		}

		public Texture2D (GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format)
		{
			this.graphicsDevice = graphicsDevice;
			this._width = width;
			this._height = height;
			this._format = format;
			this._mipmap = mipMap;

			generateOpenGLTexture ();
		}
		
		private void generateOpenGLTexture ()
		{
			// modeled after this
			// http://steinsoft.net/index.php?site=Programming/Code%20Snippets/OpenGL/no9
			
			GL.GenTextures (1, out _textureId);
			GL.BindTexture (TextureTarget.Texture2D, _textureId);
			
			if (_mipmap) {
				// Taken from http://www.flexicoder.com/blog/index.php/2009/11/iphone-mipmaps/
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapNearest);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (int)All.True);
			} else {
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			}
			
			byte[] textureData = new byte[(_width * _height) * 4];
			
			GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _width, _height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureData);			
			
			GL.BindTexture (TextureTarget.Texture2D, 0);

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

		public Color GetPixel (int x, int y)
		{
			var result = new Color (0, 0, 0, 0);

			if ((x < 0) || (y < 0))
				return result;

			if (x >= Width || y >= Height) 
				return result;

			int sz = 0;

			byte[] pixel = new byte[4];
			;
			int pos;
			IntPtr pixelOffset;
			switch (this.Format) {
			case SurfaceFormat.Color /*kTexture2DPixelFormat_RGBA8888*/:
			case SurfaceFormat.Dxt3 :
				sz = 4;
				pos = ((y * Width) + x) * sz;
				pixelOffset = new IntPtr (texture.PixelData.ToInt64 () + pos);
				Marshal.Copy (pixelOffset, pixel, 0, 4);	
				result.R = pixel [0];
				result.G = pixel [1];
				result.B = pixel [2];
				result.A = pixel [3];
				break;
			case SurfaceFormat.Bgra4444 /*kTexture2DPixelFormat_RGBA4444*/:
				sz = 2;
				pos = ((y * Width) + x) * sz;
				pixelOffset = new IntPtr (texture.PixelData.ToInt64 () + pos);					
				Marshal.Copy (pixelOffset, pixel, 0, 4);	

				result.R = pixel [0];
				result.G = pixel [1];
				result.B = pixel [2];
				result.A = pixel [3];
				break;
			case SurfaceFormat.Bgra5551 /*kTexture2DPixelFormat_RGB5A1*/:
				sz = 2;
				pos = ((y * Width) + x) * sz;
				pixelOffset = new IntPtr (texture.PixelData.ToInt64 () + pos);					
				Marshal.Copy (pixelOffset, pixel, 0, 4);	

				result.R = pixel [0];
				result.G = pixel [1];
				result.B = pixel [2];
				result.A = pixel [3];
				break;
			case SurfaceFormat.Alpha8 /*kTexture2DPixelFormat_A8*/:
				sz = 1;
				pos = ((y * Width) + x) * sz;
				pixelOffset = new IntPtr (texture.PixelData.ToInt64 () + pos);
				Marshal.Copy (pixelOffset, pixel, 0, 1);	

				result.A = pixel [0];
				break;
			default:
				throw new NotSupportedException ("Texture format");
			}

			return result;
		}

		public int Width {
			get {
				return _width;
			}
		}

		public int Height {
			get {
				return _height;
			}
		}

		public static Texture2D FromFile (GraphicsDevice graphicsDevice, Stream textureStream)
		{
			MonoMac.Foundation.NSData nsData = MonoMac.Foundation.NSData.FromStream (textureStream);

			NSImage image = new NSImage (nsData);

			if (image == null) {
				throw new ContentLoadException ("Error loading Texture2D Stream");
			}

			ESImage theTexture = new ESImage (image, graphicsDevice.PreferedFilter);			
			Texture2D result = new Texture2D (theTexture);



			return result;
		}

		public static Texture2D FromFile (GraphicsDevice graphicsDevice, Stream textureStream, int numberBytes)
		{
			throw new NotImplementedException ();
		}

		public static Texture2D FromFile (GraphicsDevice graphicsDevice, string filename, int width, int height)
		{
			NSImage image = new NSImage (filename);
			if (image == null) {
				throw new ContentLoadException ("Error loading file: " + filename);
			}			

			ESImage theTexture;

			if (width == 0 && height == 0) {
				theTexture = new ESImage (image, graphicsDevice.PreferedFilter);
			} else {
				// TODO: figure out the scaling
				//var small = image.Scale (new SizeF (width, height));
				//theTexture = new ESImage(small, graphicsDevice.PreferedFilter);
				theTexture = new ESImage (image, graphicsDevice.PreferedFilter);
			}
			Texture2D result = new Texture2D (theTexture);
			// result.Name = Path.GetFileNameWithoutExtension(filename);
			result.Name = filename;
			//			_width = theTexture.ImageWidth;
			//			_height = theTexture.ImageHeight;
			//			_format = theTexture.Format;			
			return result;					
		}

		public static Texture2D FromFile (GraphicsDevice graphicsDevice, string filename)
		{
			return FromFile (graphicsDevice, filename, 0, 0);
		}
		
		internal void Apply ()
		{
			if (texture == null)
				return;

			GL.BindTexture (TextureTarget.Texture2D, (uint)_textureId);
			if (_mipmap) {
				// Taken from http://www.flexicoder.com/blog/index.php/2009/11/iphone-mipmaps/
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapNearest);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (int)All.True);
			} else {
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			}

			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapS,
                            (float)TextureWrapMode.Repeat);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureWrapT,
                            (float)TextureWrapMode.Repeat);
		}
		
		private void Apply (byte[] textureData)
		{
			
			GL.BindTexture (TextureTarget.Texture2D, (uint)_textureId);			
			if (_mipmap) {
				// Taken from http://www.flexicoder.com/blog/index.php/2009/11/iphone-mipmaps/
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapNearest);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (int)All.True);
			} else {
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			}			

			GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _width, _height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, textureData);
		}
		
		private void SetPixel (int x, int y, byte red, byte green, byte blue, byte alpha)
		{
			
//			if (textureData != null) {
//				GL.BindTexture (TextureTarget.Texture2D, (uint)_textureId);
//				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
//				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
//				int startIndex = ((y-1) * _width) + (x-1) * 4;
//				textureData [startIndex] = red;
//				textureData [startIndex + 1] = green;
//				textureData [startIndex + 2] = blue;
//				textureData [startIndex + 3] = alpha;
//				GL.TexImage2D (TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _width, _height, 0, MonoMac.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, textureData);
//			}
//			else {
//				texture.texture.SetPixel(x,y,red,green,blue,alpha);
//			}
		}

		private void SetData (int index, byte red, byte green, byte blue, byte alpha, byte[] textureData)
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

		private byte[] AllocColorData ()
		{
			switch (_format) {				
			case SurfaceFormat.Color /*kTexture2DPixelFormat_RGBA8888*/:
			case SurfaceFormat.Dxt1:
			case SurfaceFormat.Dxt3:
				return new byte[(_width * _height) * 4];
				
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
			
			return null;
		}
		
		
		/*
		* perform an in-place swap from Quadrant 1 to Quadrant III format
		* (upside-down PostScript/GL to right side up QD/CG raster format)
		* We do this in-place, which requires more copying, but will touch
		* only half the pages.
		* 
		* Pixel reformatting may optionally be done here if needed.
		*/
		private void flipImageData (byte[] mData, int mWidth, int mHeight, int mByteWidth)
		{

			long top, bottom;
			byte[] buffer;
			long topP;
			long bottomP;
			long rowBytes;

			top = 0;
			bottom = mHeight - 1;
			rowBytes = mWidth * mByteWidth;
			buffer = new byte[rowBytes];

			while (top < bottom) {
				topP = top * rowBytes;
				bottomP = bottom * rowBytes;

				/*
				* Save and swap scanlines.
				*
				* This code does a simple in-place exchange with a temp buffer.
				* If you need to reformat the pixels, replace the first two Array.Copy
				* calls with your own custom pixel reformatter.
				*/

				Array.Copy (mData, topP, buffer, 0, rowBytes);
				Array.Copy (mData, bottomP, mData, topP, rowBytes);
				Array.Copy (buffer, 0, mData, bottomP, rowBytes);

				++top;
				--bottom;

			}
		}

		public void SetData<T> (T[] data)
		{
			SetData (data, 0, data.Length, SetDataOptions.None);
		}
		
		public void SetData<T> (T[] data, int startIndex, int elementCount, SetDataOptions options)
		{
			if (data == null) {
				throw new ArgumentNullException ("Argument data can not be null.");
			}
			
			if (startIndex < 0 || startIndex > data.Length - 1) {
				throw new ArgumentNullException ("Argument startIndex in invalid.");
			}			

			if (elementCount < 0 || (startIndex + elementCount) > data.Length) {
				throw new ArgumentNullException ("Argument elementCount is invalid.");
			}			
			
			byte[] textureData = AllocColorData ();
			// we now have a texture not based on an outside image source
			// now we check what type was passed
			if (typeof(T) == typeof(Color)) {

				for (int x = startIndex; x < elementCount; x++) {
					var color = (Color)(object)data [x];
					SetData (x, color.R, color.G, color.B, color.A, textureData);
					
				}
				
				// For RenderTextures we need to flip the data.
				if (texture == null) {
					flipImageData (textureData, _width, _height, 4);
				}
			}
			
			// when we are all done we need apply the changes
			Apply (textureData);
		}

		public void SetData<T> (int level, Rectangle? rect, T[] data, int startIndex, int elementCount, SetDataOptions options)
		{
			if (data == null) {
				throw new ArgumentException ("data cannot be null");
			}

			if (data.Length < startIndex + elementCount) {
				throw new ArgumentException ("The data passed has a length of " + data.Length + " but " + elementCount + " pixels have been requested.");
			}

			Rectangle r;
			if (rect != null) {
				r = rect.Value;
			} else {
				r = new Rectangle (0, 0, Width, Height);
			}
		}
		
		private byte[] GetImageData (int level)
		{
			
			byte[] imageInfo;
			int sz = 0;
			
			GL.BindTexture (TextureTarget.Texture2D, ID);
			if (_mipmap) {
				// Taken from http://www.flexicoder.com/blog/index.php/2009/11/iphone-mipmaps/
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.LinearMipmapNearest);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, (int)All.True);
			} else {
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
				GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			}			

			switch (_format) {
			case SurfaceFormat.Color : //kTexture2DPixelFormat_RGBA8888
			case SurfaceFormat.Dxt3 :
				
				sz = 4;
				imageInfo = new byte[(_width * _height) * sz];
				break;
			case SurfaceFormat.Bgra4444 : //kTexture2DPixelFormat_RGBA4444
				sz = 2;
				imageInfo = new byte[(_width * _height) * sz];
				
				break;
			case SurfaceFormat.Bgra5551 : //kTexture2DPixelFormat_RGB5A1
				sz = 2;
				imageInfo = new byte[(_width * _height) * sz];
				break;
			case SurfaceFormat.Alpha8 :  // kTexture2DPixelFormat_A8 
				sz = 1;
				imageInfo = new byte[(_width * _height) * sz];
				break;
			default:
				throw new NotSupportedException ("Texture format");
			}
			
			GL.GetTexImage (TextureTarget.Texture2D, level, PixelFormat.Rgba, PixelType.UnsignedByte, imageInfo);

			return imageInfo;
					
		}
		
		public void GetData<T> (T[] data)
		{	
			if (data == null) {
				throw new ArgumentException ("data cannot be null");
			}
			
			GetData (0, null, data, 0, data.Length);

		}

		public void GetData<T> (T[] data, int startIndex, int elementCount)
		{
			GetData<T> (0, null, data, startIndex, elementCount);
		}

		public void GetData<T> (int level, Rectangle? rect, T[] data, int startIndex, int elementCount)
		{
			if (data == null) {
				throw new ArgumentException ("data cannot be null");
			}

			if (data.Length < startIndex + elementCount) {
				throw new ArgumentException ("The data passed has a length of " + data.Length + " but " + elementCount + " pixels have been requested.");
			}

			Rectangle r;
			if (rect != null) {
				r = rect.Value;
			} else {
				r = new Rectangle (0, 0, Width, Height);
			}

			byte[] imageInfo = GetImageData (0);
			
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
						colors[i].R << 24 |
						colors[i].G << 16 |
						colors[i].B << 8 |
						colors[i].A
					);
				}
			}
			// Get the Color values
			else if ((typeof(T) == typeof(Color))) {


				// Left this here for documentation - Not sure what it does but the
				// routine looks important

				//Convert "RRRRRRRRRGGGGGGGGBBBBBBBBAAAAAAAA" to "RRRRRGGGGGGBBBBB"
				/*
				if(pixelFormat == SurfaceFormat.Rgb32) {
					tempData = Marshal.AllocHGlobal(height * width * 2);

					int d32;
					short d16;
					int inPixel32Count=0,outPixel16Count=0;
					for(i = 0; i < width * height; ++i, inPixel32Count+=sizeof(int))
					{
						d32 = Marshal.ReadInt32(imageData,inPixel32Count);
						short R = (short)((((d32 >> 0) & 0xFF) >> 3) << 11);
						short G = (short)((((d32 >> 8) & 0xFF) >> 2) << 5);
						short B = (short)((((d32 >> 16) & 0xFF) >> 3) << 0);
						d16 = (short)  (R | G | B);
						Marshal.WriteInt16(tempData,outPixel16Count,d16);
						outPixel16Count += sizeof(short);
					}
					Marshal.FreeHGlobal(imageData);
					imageData = tempData;			
				}									
				*/

				int rWidth = r.Width;
				int rHeight = r.Height;

//				if (texture == null) {
//					// For rendertargets we need to loop through and load the elements
//					// backwards because the texture data is flipped vertically and horizontally
//					var dataEnd = (rWidth * rHeight) - 1;
//					var dataPos = 0;
//					var dataRowColOffset = 0;
//					for (int y = r.Top; y < rHeight; y++) {
//						for (int x = r.Left; x < rWidth; x++) {
//							var result = new Color (0, 0, 0, 0);						
//							dataRowColOffset = ((y * rWidth) + x);
//							switch (_format) {
//							case SurfaceFormat.Color : //kTexture2DPixelFormat_RGBA8888
//							case SurfaceFormat.Dxt3 :
//								
//								dataPos = dataRowColOffset * 4;								
//															
//								result.R = imageInfo [dataPos];
//								result.G = imageInfo [dataPos + 1];
//								result.B = imageInfo [dataPos + 2];
//								result.A = imageInfo [dataPos + 3];
//								break;
//							default:
//								throw new NotSupportedException ("Texture format");
//							}
//							data [dataEnd - dataRowColOffset] = (T)(object)result;
//						}
//
//
//					}
//				} else {
					// Loop through and extract the data but we need to load it 
					var dataRowColOffset = 0;
					var sz = 0;
					var pixelOffset = 0;
					for (int y = r.Top; y < r.Top + rHeight; y++) {
						for (int x = r.Left; x < r.Left + rWidth; x++) {
							var result = new Color (0, 0, 0, 0);
							dataRowColOffset = ((y * _width) + x);
							switch (_format) {
							case SurfaceFormat.Color : //kTexture2DPixelFormat_RGBA8888
							case SurfaceFormat.Dxt3 :
								sz = 4;
								pixelOffset = dataRowColOffset * sz;
								result.R = imageInfo [pixelOffset];
								result.G = imageInfo [pixelOffset + 1];
								result.B = imageInfo [pixelOffset + 2];
								result.A = imageInfo [pixelOffset + 3];
								break;
							case SurfaceFormat.Bgra4444 : //kTexture2DPixelFormat_RGBA4444
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
								result.R = imageInfo [pixelOffset];
								result.G = imageInfo [pixelOffset + 1];
								result.B = imageInfo [pixelOffset + 2];
								result.A = imageInfo [pixelOffset + 3];
								break;							
							case SurfaceFormat.Bgra5551 : //kTexture2DPixelFormat_RGB5A1
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
								result.R = imageInfo [pixelOffset];
								result.G = imageInfo [pixelOffset + 1];
								result.B = imageInfo [pixelOffset + 2];
								result.A = imageInfo [pixelOffset + 3];
								break;
							case SurfaceFormat.Alpha8 :  // kTexture2DPixelFormat_A8
//								sz = 1;
//								pos = ((y * imageSize.Width) + x) * sz;
//								pixelOffset = new IntPtr (imageData.ToInt64 () + pos);
//								Marshal.Copy (pixelOffset, pixel, 0, 4);
//	
//								result.A = pixel [0];
								sz = 1;
								pixelOffset = dataRowColOffset * sz;
								result.A = imageInfo [pixelOffset];
								break;
							default:
								throw new NotSupportedException ("Texture format");
							}
							data [((y - r.Top) * r.Width) + (x - r.Left)] = (T)(object)result;
						}
					}
				//}
			} else {
				throw new NotImplementedException ("GetData not implemented for type.");
			}
		}
	}
}

