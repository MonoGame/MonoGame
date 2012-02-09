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
using System.Runtime.InteropServices;
using System.IO;

#if !IPHONE
using System.Drawing.Imaging;
#endif

#if MONOMAC
using MonoMac.AppKit;
using MonoMac.CoreGraphics;
using MonoMac.Foundation;
using MonoMac.OpenGL;
using GLPixelFormat = MonoMac.OpenGL.PixelFormat;
#elif WINDOWS
using OpenTK.Graphics.OpenGL;
using GLPixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;
#else

#if IPHONE
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
#endif

#if ES11
using OpenTK.Graphics.ES11;
#if IPHONE
using TextureTarget = OpenTK.Graphics.ES11.All;
using PixelType = OpenTK.Graphics.ES11.All;
using TextureParameterName = OpenTK.Graphics.ES11.All;
using PixelInternalFormat = OpenTK.Graphics.ES11.All;
using VertexPointerType = OpenTK.Graphics.ES11.All;
using GLPixelFormat = OpenTK.Graphics.ES11.All;
using TexCoordPointerType = OpenTK.Graphics.ES11.All;
using BeginMode = OpenTK.Graphics.ES11.All;
#endif
#else
using OpenTK.Graphics.ES20;

#if IPHONE
using TextureTarget = OpenTK.Graphics.ES20.All;
using PixelType = OpenTK.Graphics.ES20.All;
using TextureParameterName = OpenTK.Graphics.ES20.All;
using PixelInternalFormat = OpenTK.Graphics.ES20.All;
using VertexPointerType = OpenTK.Graphics.ES20.All;
using GLPixelFormat = OpenTK.Graphics.ES20.All;
#else
using GLPixelFormat = OpenTK.Graphics.ES20.PixelFormat;
#endif

#endif

#endif

namespace Microsoft.Xna.Framework.Graphics
{
	internal class ESTexture2D : IDisposable
	{
		private uint _name;
		private Size _size;
		private int _width, _height;
		private SurfaceFormat _format;
		private float _maxS, _maxT;
		private IntPtr _pixelData;
		internal byte[] _pixelInfo;
		
		public ESTexture2D (IntPtr data, int dataLength, SurfaceFormat pixelFormat, int width, int height, Size size, All filter)
		{
			InitWithData (data, dataLength, pixelFormat, width, height, size, filter);
		}
		
#if !IPHONE
        public ESTexture2D(Bitmap image, All filter)
        {
            InitWithBitmap(image, filter);
        }

        public void InitWithBitmap(Bitmap image, All filter)
        {
            BitmapData bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly,
                           System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            _format = SurfaceFormat.Color;
            InitWithData(bitmapData.Scan0, image.Width*image.Height*4, _format, image.Width, image.Height, new Size(image.Width, image.Height), filter);
            image.UnlockBits(bitmapData);
        }
#endif
		
#if MONOMAC
		public ESTexture2D (NSImage nsImage, All filter)
		{
			// TODO InitWithCGImage(nsImage,filter);
			CGImage image = nsImage.AsCGImage (RectangleF.Empty, null, null);
			InitWithCGImage (image, filter);
		}
#elif IPHONE
		public ESTexture2D(UIImage uiImage, All filter)
		{
			InitWithCGImage(uiImage.CGImage,filter);
		}
#endif

#if MONOMAC || IPHONE
		public ESTexture2D (CGImage cgImage, All filter)
		{
			InitWithCGImage (cgImage, filter);
		}

		private void InitWithCGImage (CGImage image, All filter)
		{
			int	width, height, i;
			CGContext context = null;
			IntPtr data;
			CGColorSpace colorSpace;
			IntPtr tempData;
			bool hasAlpha;
			CGImageAlphaInfo info;
			CGAffineTransform transform;
			Size imageSize;
			SurfaceFormat pixelFormat;
			

			if (image == null) {
				throw new ArgumentException (" NSImage is invalid! " );
			}

			info = image.AlphaInfo;
			hasAlpha = ((info == CGImageAlphaInfo.PremultipliedLast) || (info == CGImageAlphaInfo.PremultipliedFirst) || (info == CGImageAlphaInfo.Last) || (info == CGImageAlphaInfo.First) ? true : false);

			if (image.ColorSpace != null) {
				if (hasAlpha) {
					pixelFormat = SurfaceFormat.Color;
				} else {
					pixelFormat = SurfaceFormat.Color;
				}
			} else {	
				pixelFormat = SurfaceFormat.Alpha8;
			}

			imageSize = new Size (image.Width,image.Height);
			transform = CGAffineTransform.MakeIdentity ();
			width = imageSize.Width;
			
			// Take out the width and height adjustments for power of 2
			//  If not then GetData and SetData is messed up.
			
			// The Mac opengl version supports non power of 2 textures
			// so we do not have to make them so
			// TODO: Check for the extension instead
#if IPHONE
			bool sizeToFit = false;
			if ((width != 1) && ((width & (width - 1)) != 0)) {
				i = 1;
				while ((sizeToFit ? 2 * i : i) < width)
					i *= 2;
				width = i;
			}
#endif
			
			height = imageSize.Height;

#if IPHONE
			if ((height != 1) && ((height & (height - 1)) != 0)) {
				i = 1;
				while ((sizeToFit ? 2 * i : i) < height)
					i *= 2;
				height = i;
			}
			// TODO: kMaxTextureSize = 1024
//			while ((width > 1024) || (height > 1024)) {
//				width /= 2;
//				height /= 2;
//				transform = CGAffineTransform.MakeScale (0.5f, 0.5f);
//				imageSize.Width /= 2;
//				imageSize.Height /= 2;
//			}

			// I am going to take the following out right now to see how it reacts.  It seems to be causing
			// a few problems for people.  We may need to come up with another way to solve these issues as
			// one size is not fitting all.
//			float size = Math.Max(width,height);
//			if(size > 1024) 
//			{
//				float ratio = 1024 / size;
//				width = (int)(width * ratio);
//				height = (int)(height * ratio);
//				transform = CGAffineTransform.MakeScale(ratio, ratio);
//				imageSize.Width = (int)(imageSize.Width * ratio);
//				imageSize.Height = (int)(imageSize.Height * ratio);;
//			}
#endif
			
			int dataLength;
			switch (pixelFormat) {		
			case SurfaceFormat.Color:
				colorSpace = CGColorSpace.CreateDeviceRGB ();
				dataLength = height * width * 4;
				data = Marshal.AllocHGlobal (dataLength);
				context = new CGBitmapContext (data,
				                               width, height,
				                               8, 4 * width,
				                               colorSpace,
				                               CGImageAlphaInfo.PremultipliedLast);
				colorSpace.Dispose ();
				break;	
			case SurfaceFormat.Alpha8:
				dataLength = height * width;
				data = Marshal.AllocHGlobal (dataLength);
				context = new CGBitmapContext (data, width, height, 8, width, null, CGImageAlphaInfo.Only);
				break;				
			default:
				throw new NotSupportedException ("Invalid pixel format"); 
			}

			context.ClearRect (new RectangleF (0,0,width,height));
			context.TranslateCTM (0, height - imageSize.Height);

			if (!transform.IsIdentity) {
				context.ConcatCTM (transform);
			}

			context.DrawImage (new RectangleF (0, 0, image.Width, image.Height), image);

			//Convert "RRRRRRRRRGGGGGGGGBBBBBBBBAAAAAAAA" to "RRRRRGGGGGGBBBBB"
			/*
			if(pixelFormat == SurfaceFormat.Rgb32) {
				tempData = Marshal.AllocHGlobal(height * width * 2);

				int d32;
				short d16;
				int inPixel32Count=0,outPixel16Count=0;
				for(i = 0; i < width * height; ++i, inPixel32Count+=sizeof(int))
				{
					d32 = Marshal.ReadInt32(data,inPixel32Count);
					short R = (short)((((d32 >> 0) & 0xFF) >> 3) << 11);
					short G = (short)((((d32 >> 8) & 0xFF) >> 2) << 5);
					short B = (short)((((d32 >> 16) & 0xFF) >> 3) << 0);
					d16 = (short)  (R | G | B);
					Marshal.WriteInt16(tempData,outPixel16Count,d16);
					outPixel16Count += sizeof(short);
				}
				Marshal.FreeHGlobal(data);
				data = tempData;			
			}
			*/

			InitWithData (data, dataLength, pixelFormat, width, height, imageSize, filter);

			context.Dispose ();
			Marshal.FreeHGlobal (data);	
		}
#endif

		public void Dispose ()
		{
			if (_name != 0) {
				GL.DeleteTextures (1, ref _name);
			}
		}

		private static byte GetBits64 (ulong source, int first, int length, int shift)
		{
			uint[] bitmasks = { 0x00, 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff };
			uint bitmask = bitmasks [length];
			source = source >> first;
			source = source & bitmask;
			source = source << shift;
			return (byte)source;
		}

		private static byte GetBits (uint source, int first, int length, int shift)
		{
			uint[] bitmasks = { 0x00, 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff };

			uint bitmask = bitmasks [length];
			source = source >> first;
			source = source & bitmask;
			source = source << shift;
			return (byte)source;
		}

		private static void SetColorFromPacked (byte[] data, int offset, byte alpha, uint packed)
		{
			byte r = (byte)(GetBits (packed, 0, 8, 0));
			byte g = (byte)(GetBits (packed, 8, 8, 0));
			byte b = (byte)(GetBits (packed, 16, 8, 0));
			data [offset] = r;
			data [offset + 1] = g;
			data [offset + 2] = b;
			data [offset + 3] = alpha;
		}

		private static void ColorsFromPacked (uint[] colors, uint c0, uint c1, bool flag)
		{
			uint rb0, rb1, rb2, rb3, g0, g1, g2, g3;

			rb0 = (c0 << 3 | c0 << 8) & 0xf800f8;
			rb1 = (c1 << 3 | c1 << 8) & 0xf800f8;
			rb0 += (rb0 >> 5) & 0x070007;
			rb1 += (rb1 >> 5) & 0x070007;
			g0 = (c0 << 5) & 0x00fc00;
			g1 = (c1 << 5) & 0x00fc00;
			g0 += (g0 >> 6) & 0x000300;
			g1 += (g1 >> 6) & 0x000300;

			colors [0] = rb0 + g0;
			colors [1] = rb1 + g1;

			if (c0 > c1 || flag) {
				rb2 = (((2 * rb0 + rb1) * 21) >> 6) & 0xff00ff;
				rb3 = (((2 * rb1 + rb0) * 21) >> 6) & 0xff00ff;
				g2 = (((2 * g0 + g1) * 21) >> 6) & 0x00ff00;
				g3 = (((2 * g1 + g0) * 21) >> 6) & 0x00ff00;
				colors [3] = rb3 + g3;
			} else {
				rb2 = ((rb0 + rb1) >> 1) & 0xff00ff;
				g2 = ((g0 + g1) >> 1) & 0x00ff00;
				colors [3] = 0;
			}

			colors [2] = rb2 + g2;
		}

		public static byte[] GetBits (int width, int length, int height, BinaryReader rdr)
		{
			int xoffset = 0;
			int yoffset = 0;
			int rowLength = width * 4;
			byte[] b = new byte[length];
			ulong alpha;
			ushort c0, c1;
			uint[] colors = new uint[4];
			uint lu;
			for (int y = 0; y < height / 4; y++) {
				yoffset = y * 4;
				for (int x = 0; x < width / 4; x++) {
					xoffset = x * 4;
					alpha = rdr.ReadUInt64 ();
					c0 = rdr.ReadUInt16 ();
					c1 = rdr.ReadUInt16 ();
					ColorsFromPacked (colors, c0, c1, true);
					lu = rdr.ReadUInt32 ();
					for (int i = 0; i < 16; i++) {
						int idx = GetBits (lu, 30 - i * 2, 2, 0);
						uint ci = colors [idx];
						int ii = 15 - i;
						byte a = (byte)(GetBits64 (alpha, ii * 4, 4, 0));
						a += (byte)(a << 4);
						int yy = yoffset + (ii / 4);
						int xx = xoffset + (ii % 4);
						int offset = yy * rowLength + xx * 4;
						SetColorFromPacked (b, offset, a, ci);
					}
				}
			}
			return b;
		}

		public void InitWithData (IntPtr data, int dataLength, SurfaceFormat pixelFormat, int width, int height, Size size, All filter)
		{		
#if IPHONE
			GL.GenTextures (1, ref _name);
#else			
			GL.GenTextures (1, out _name);
#endif			
			GL.BindTexture (TextureTarget.Texture2D, _name);
			GL.TexParameter (TextureTarget.Texture2D,
			                 TextureParameterName.TextureMinFilter, (int)filter);
			GL.TexParameter (TextureTarget.Texture2D,
			                 TextureParameterName.TextureMagFilter, (int)filter);
			Console.WriteLine (pixelFormat.ToString ());
			switch (pixelFormat) {				
			case SurfaceFormat.Color /*kTexture2DPixelFormat_RGBA8888*/:
				GL.TexImage2D (TextureTarget.Texture2D, 0,
#if IPHONE
				               (int)PixelInternalFormat.Rgba,
#else				               
				               PixelInternalFormat.Rgba,
#endif
				               (int)width, (int)height, 0,
				               GLPixelFormat.Rgba, PixelType.UnsignedByte, data);
				break;
#if !IPHONE
			case SurfaceFormat.Dxt1:
				GL.CompressedTexImage2D (TextureTarget.Texture2D, 0,
				                         PixelInternalFormat.CompressedRgbaS3tcDxt1Ext,
				                         (int)width, (int)height, 0,
				                         dataLength, data);
				break;
			case SurfaceFormat.Dxt3:
				GL.CompressedTexImage2D (TextureTarget.Texture2D, 0,
				                         PixelInternalFormat.CompressedRgbaS3tcDxt3Ext,
				                         (int)width, (int)height, 0,
				                         dataLength, data);
				break;
			case SurfaceFormat.Dxt5:
				GL.CompressedTexImage2D (TextureTarget.Texture2D, 0,
				                         PixelInternalFormat.CompressedRgbaS3tcDxt5Ext,
				                         (int)width, (int)height, 0,
				                         dataLength, data);
				break;
#endif
			case SurfaceFormat.Bgra4444 /*kTexture2DPixelFormat_RGBA4444*/:
				GL.TexImage2D (TextureTarget.Texture2D, 0,
				               
#if IPHONE
				               (int)PixelInternalFormat.Rgba,
#else				               
				               PixelInternalFormat.Rgba,
#endif
				               (int)width, (int)height, 0,
				               GLPixelFormat.Rgba, PixelType.UnsignedShort4444, data);
				break;
			case SurfaceFormat.Bgra5551 /*kTexture2DPixelFormat_RGB5A1*/:
				GL.TexImage2D (TextureTarget.Texture2D, 0, 
#if IPHONE
				               (int)PixelInternalFormat.Rgba,
#else				               
				               PixelInternalFormat.Rgba,
#endif
				               (int)width, (int)height, 0,
				               GLPixelFormat.Rgba, PixelType.UnsignedShort5551, data);
				break;
			case SurfaceFormat.Alpha8 /*kTexture2DPixelFormat_A8*/:
				GL.TexImage2D (TextureTarget.Texture2D, 0,
#if IPHONE
				               (int)PixelInternalFormat.Alpha,
#else				               
				               PixelInternalFormat.Alpha,
#endif
				               (int)width, (int)height, 0,
				               GLPixelFormat.Alpha, PixelType.UnsignedByte, data);
				break;
			default:
				throw new NotSupportedException ("Texture format");
				;					
			}

			_size = size;
			_width = width;
			_height = height;
			_format = pixelFormat;
			_maxS = size.Width / (float)width;
			_maxT = size.Height / (float)height;

			_pixelData = data;
			//IntPtr pData = texture.texture.PixelData;
//			int size3 = (_width * _height) * 4;
//			byte[] _pixelInfo = new byte[size3];
//
//			Marshal.Copy(data, _pixelInfo, 0, size3);			
			//_pixelInfo = data;
		}
		
		
		public void SetPixel (int x, int y, byte red, byte green, byte blue, byte alpha)
		{

			GL.BindTexture (TextureTarget.Texture2D, _name);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
			GL.TexParameter (TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
			
			switch (_format) {				
			case SurfaceFormat.Color /*kTexture2DPixelFormat_RGBA8888*/:
				byte[] pixelInfo = new byte[4] { red, green, blue, alpha };
				Marshal.Copy (pixelInfo, ((y-1) * _width) + (x-1), _pixelData, 4);
				GL.TexImage2D (TextureTarget.Texture2D, 0,
#if IPHONE
				               (int)PixelInternalFormat.Rgba,
#else				               
				               PixelInternalFormat.Rgba,
#endif
				               _width, _height, 0,
				               GLPixelFormat.Rgba, PixelType.UnsignedByte, _pixelData);
				break;
			// TODO: Implement the rest of these but lack of knowledge and examples prevents this for now
			case SurfaceFormat.Bgra4444 /*kTexture2DPixelFormat_RGBA4444*/:
				GL.TexImage2D (TextureTarget.Texture2D, 0,
#if IPHONE
				               (int)PixelInternalFormat.Rgba,
#else				               
				               PixelInternalFormat.Rgba,
#endif
				               _width, _height, 0,
				               GLPixelFormat.Rgba, PixelType.UnsignedShort4444, _pixelData);
				break;
			case SurfaceFormat.Bgra5551 /*kTexture2DPixelFormat_RGB5A1*/:
				GL.TexImage2D (TextureTarget.Texture2D, 0, 
#if IPHONE
				               (int)PixelInternalFormat.Rgba,
#else				               
				               PixelInternalFormat.Rgba,
#endif
				               _width, _height, 0,
				               GLPixelFormat.Rgba, PixelType.UnsignedShort5551, _pixelData);
				break;
			case SurfaceFormat.Alpha8 /*kTexture2DPixelFormat_A8*/:
				GL.TexImage2D (TextureTarget.Texture2D, 0,
#if IPHONE
				               (int)PixelInternalFormat.Alpha,
#else				               
				               PixelInternalFormat.Alpha,
#endif
				               _width, _height, 0,
				               GLPixelFormat.Alpha, PixelType.UnsignedByte, _pixelData);
				break;
			default:
				throw new NotSupportedException ("Texture format");
				;					
			}
			
		}

		public void DrawAtPoint (Vector2 point)
		{
			float []coordinates = { 0,	_maxT, _maxS, _maxT, 0, 0,_maxS, 0 };
			float width = (float)_width * _maxS;
			float height = (float)_height * _maxT;
			float []vertices = {	-width / 2.0f + point.X, -height / 2.0f + point.Y,	0.0f,
								width / 2.0f + point.X,	-height / 2.0f + point.Y,	0.0f,
								-width / 2.0f + point.X,	height / 2.0f + point.Y,	0.0f,
								width / 2.0f + point.X,	height / 2.0f + point.Y,	0.0f };
#if ES11
			GL.BindTexture (TextureTarget.Texture2D, _name);
			GL.VertexPointer (3, VertexPointerType.Float, 0, vertices);
			GL.TexCoordPointer (2, TexCoordPointerType.Float, 0, coordinates);
			GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);
#else
			throw new NotImplementedException();
#endif			
		}

		public void DrawInRect (Rectangle rect)
		{
			float[]	 coordinates = {  0, _maxT,_maxS, _maxT,0, 0,_maxS,	0  };
			float[]	vertices = { rect.Left,	rect.Top, 0.0f, rect.Right, rect.Top,0.0f,rect.Left,rect.Bottom,0.0f,rect.Right,rect.Bottom,0.0f };
			
#if ES11
			GL.BindTexture (TextureTarget.Texture2D, _name);
			GL.VertexPointer (3, VertexPointerType.Float, 0, vertices);
			GL.TexCoordPointer (2, TexCoordPointerType.Float, 0, coordinates);
			GL.DrawArrays (BeginMode.TriangleStrip, 0, 4);
#else
			throw new NotImplementedException();
#endif			
		}

		public Size ContentSize {
			get {
				return _size;
			}
		}

		public SurfaceFormat PixelFormat {
			get {
				return _format;
			}
		}

		public int PixelsWide {
			get {
				return _width;
			}
		}

		public int PixelsHigh {
			get {
				return _height;
			}
		}

		public uint Name {
			get {
				return _name;
			}
		}

		public float MaxS {
			get {
				return _maxS;
			}
		}

		public float MaxT {
			get {
				return _maxT;
			}
		}

		public IntPtr PixelData {
			get {
				return _pixelData;
			}
		}
	}
}
