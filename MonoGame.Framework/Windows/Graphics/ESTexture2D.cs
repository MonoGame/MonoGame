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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;
using OpenTK.Graphics.OpenGL;
using Buffer = System.Buffer;


namespace Microsoft.Xna.Framework.Graphics
{
	internal class ESTexture2D : IDisposable
	{
		private uint _name;
		private Size _size;
		private int _width,_height;
		private SurfaceFormat _format;
		private float _maxS,_maxT;
        private IntPtr _pixelData;
		
		public ESTexture2D (IntPtr data, SurfaceFormat pixelFormat, int width, int height, Size size, All filter)
		{
			InitWithData(data,pixelFormat,width,height,size, filter);
		}
		
		public ESTexture2D(Bitmap image, All filter)
		{
            InitWithBitmap(image, filter);
		}

        public void InitWithBitmap(Bitmap image, All filter)
        {
            BitmapData bitmapData = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly,
                           System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            _format = SurfaceFormat.Color;
            InitWithData(bitmapData.Scan0, _format, image.Width, image.Height, new Size(image.Width, image.Height), filter);
            image.UnlockBits(bitmapData);
        }

        public void InitWithData(IntPtr data, SurfaceFormat pixelFormat, int width, int height, Size size, All filter)
        {
            GL.GenTextures(1, out _name);
            GL.BindTexture(TextureTarget.Texture2D, _name);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)filter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)filter);

            int sz = 0;

            switch (pixelFormat) {
				case SurfaceFormat.Color /*kTexture2DPixelFormat_RGBA8888*/:
                case SurfaceFormat.Dxt3:
                    sz = 4;
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)width, (int)height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, data);
                    break;
                case SurfaceFormat.Bgra4444 /*kTexture2DPixelFormat_RGBA4444*/:
                    sz = 2;
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)width, (int)height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedShort4444, data);
                    break;
                case SurfaceFormat.Bgra5551 /*kTexture2DPixelFormat_RGB5A1*/:
                    sz = 2;
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, (int)width, (int)height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedShort5551, data);
                    break;
                case SurfaceFormat.Alpha8 /*kTexture2DPixelFormat_A8*/:
                    sz = 1;
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Alpha, (int)width, (int)height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.UnsignedByte, data);
                    break;
                default:
                    throw new NotSupportedException("Texture format");
            }
            
            _size = size;
            _width = width;
            _height = height;
            _format = pixelFormat;
            _maxS = size.Width / (float)width;
            _maxT = size.Height / (float)height;

            _pixelData = data;
        }

        public void SetPixel(int x, int y, byte red, byte green, byte blue, byte alpha)
        {

            GL.BindTexture(TextureTarget.Texture2D, _name);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);

            switch (_format)
            {
                case SurfaceFormat.Color /*kTexture2DPixelFormat_RGBA8888*/:
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt3:
                    byte[] pixelInfo = new byte[4] { red, green, blue, alpha };
                    Marshal.Copy(pixelInfo, ((y - 1) * _width) + (x - 1), _pixelData, 4);
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _width, _height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, _pixelData);
                    break;

                // TODO: Implement the rest of these but lack of knowledge and examples prevents this for now
                case SurfaceFormat.Bgra4444 /*kTexture2DPixelFormat_RGBA4444*/:
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _width, _height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedShort4444, _pixelData);
                    break;
                case SurfaceFormat.Bgra5551 /*kTexture2DPixelFormat_RGB5A1*/:
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _width, _height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedShort5551, _pixelData);
                    break;
                case SurfaceFormat.Alpha8 /*kTexture2DPixelFormat_A8*/:
                    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Alpha, _width, _height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Alpha, PixelType.UnsignedByte, _pixelData);
                    break;
                default:
                    throw new NotSupportedException("Texture format");
                    ;
            }

        }
				
		public void Dispose ()
		{
			if(_name != 0) 
			{
	 			GL.DeleteTextures(1, ref _name);
			}
		}
		
		private static byte GetBits64(ulong source, int first, int length, int shift)
        {
			uint[] bitmasks = { 0x00, 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff };
            uint bitmask = bitmasks[length];
            source = source >> first;
            source = source & bitmask;
            source = source << shift;
            return (byte)source;
        }
		
		private static byte GetBits(uint source, int first, int length, int shift)
        {
			uint[] bitmasks = { 0x00, 0x01, 0x03, 0x07, 0x0f, 0x1f, 0x3f, 0x7f, 0xff };
			
            uint bitmask = bitmasks[length];
            source = source >> first;
            source = source & bitmask;
            source = source << shift;
            return (byte)source;
        }

		
		private static void SetColorFromPacked(byte[] data, int offset, byte alpha, uint packed)
        {
            byte r = (byte)(GetBits(packed, 0, 8, 0));
            byte g = (byte)(GetBits(packed, 8, 8, 0));
            byte b = (byte)(GetBits(packed, 16, 8, 0));
            data[offset] = r;
            data[offset + 1] = g;
            data[offset + 2] = b;
            data[offset + 3] = alpha;
        }

		private static void ColorsFromPacked(uint[] colors, uint c0, uint c1, bool flag)
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

            colors[0] = rb0 + g0;
            colors[1] = rb1 + g1;

            if (c0 > c1 || flag)
            {
                rb2 = (((2 * rb0 + rb1) * 21) >> 6) & 0xff00ff;
                rb3 = (((2 * rb1 + rb0) * 21) >> 6) & 0xff00ff;
                g2 = (((2 * g0 + g1) * 21) >> 6) & 0x00ff00;
                g3 = (((2 * g1 + g0) * 21) >> 6) & 0x00ff00;
                colors[3] = rb3 + g3;
            }
            else
            {
                rb2 = ((rb0 + rb1) >> 1) & 0xff00ff;
                g2 = ((g0 + g1) >> 1) & 0x00ff00;
                colors[3] = 0;
            }

            colors[2] = rb2 + g2;
        }
		
		static public ESTexture2D InitiFromDxt3File(BinaryReader rdr, int length, int width, int height)
        {
            byte [] b = GetBits (width, length, height, rdr);
			
			// Copy bits
			IntPtr pointer = Marshal.AllocHGlobal(length);
			Marshal.Copy (b, 0, pointer, length);
            ESTexture2D result = new ESTexture2D(pointer,SurfaceFormat.Dxt3,width,height,new Size(width,height),All.Linear);
			Marshal.FreeHGlobal(pointer);
			return result;
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
						uint ci = colors[idx];
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

        public void DrawAtPoint(Vector2 point)
		{
			float []coordinates = { 0,	_maxT, _maxS, _maxT, 0, 0,_maxS, 0 };
			float width = (float)_width * _maxS;
			float height = (float)_height * _maxT;
			float []vertices = {	-width / 2.0f + point.X, -height / 2.0f + point.Y,	0.0f,
								width / 2.0f + point.X,	-height / 2.0f + point.Y,	0.0f,
								-width / 2.0f + point.X,	height / 2.0f + point.Y,	0.0f,
								width / 2.0f + point.X,	height / 2.0f + point.Y,	0.0f };

            GL.BindTexture(TextureTarget.Texture2D, _name);
			GL.VertexPointer(3, VertexPointerType.Float, 0, vertices);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, coordinates);
			GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
		}
		
		public void DrawInRect(Rectangle rect)
		{
			float[]	 coordinates = {  0, _maxT,_maxS, _maxT,0, 0,_maxS,	0  };
			float[]	vertices = { rect.Left,	rect.Top, 0.0f, rect.Right, rect.Top,0.0f,rect.Left,rect.Bottom,0.0f,rect.Right,rect.Bottom,0.0f };
			
			GL.BindTexture(TextureTarget.Texture2D, _name);
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertices);
            GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, coordinates);
			GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
		}
		
		public Size ContentSize
		{
			get 
			{
				return _size;
			}
		}
		
		public SurfaceFormat PixelFormat
		{
			get 
			{
				return _format;
			}
		}
		
		public int PixelsWide 
		{
			get 
			{
				return _width;
			}
		}
		
		public int PixelsHigh 
		{
			get 
			{
				return _height;
			}
		}
		
		public uint Name 
		{
            get { return _name; }
		}
		
		public float MaxS 
		{
			get 
			{
				return _maxS;
			}
		}
		
		public float MaxT 
		{
			get 
			{
				return _maxT;
			}
		}

        public IntPtr PixelData
        {
            get
            {
                return _pixelData;
            }
        }
		
	}
}
