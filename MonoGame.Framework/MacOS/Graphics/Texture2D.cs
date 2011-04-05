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
using MonoMac.Foundation;
using MonoMac.OpenGL;

using Microsoft.Xna.Framework.Content;


namespace Microsoft.Xna.Framework.Graphics
{
    public class Texture2D : Texture
    {
		private ESImage texture;
		private string name;
		
		internal bool IsSpriteFontTexture {get;set;}
		
		// my change
		// --------
		public uint ID
		{
			get
			{ 
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
                return new Rectangle(0,0,texture.ImageWidth, texture.ImageHeight);
            }
        }
		
		internal Texture2D(ESImage theImage)
		{
			texture = theImage;
		}
		
        public Texture2D(GraphicsDevice graphicsDevice, int width, int height, int numberLevels, TextureUsage usage, SurfaceFormat format)
        {
			throw new NotImplementedException();
        }

        public Texture2D(Texture2D source, Color color)
        {
            throw new NotImplementedException();
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
				case SurfaceFormat.Rgba32 /*kTexture2DPixelFormat_RGBA8888*/:
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
				case SurfaceFormat.Rgb32 /*kTexture2DPixelFormat_RGB565*/:
					sz = 2;	
					pos = ( (y * Width) + x ) * sz;
					pixelOffset = new IntPtr(texture.PixelData.ToInt64() + pos);					
					Marshal.Copy(pixelOffset, pixel, 0, 4);	
				
					result.R = pixel[0];
					result.G = pixel[1];
					result.B = pixel[2];					
					result.A = 255;
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

        public void SetData<T>(T[] data)
        {
			throw new NotImplementedException();
        }

        public int Width
        {
            get
            {
                return texture.ImageWidth;
            }
        }

        public int Height
        {
            get
            {
                return texture.ImageHeight;
            }
        }

        public SurfaceFormat Format
        {
            get 
			{ 
				return texture.Format;
			}
        }

        public TextureUsage TextureUsage
        {
            get 
			{ 
				throw new NotImplementedException();
			}
        }
        
        public static Texture2D FromFile(GraphicsDevice graphicsDevice, Stream textureStream)
        {
            NSData nsData = NSData.FromStream(textureStream);

			NSImage image = new NSImage(nsData);
			
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
			throw new NotImplementedException();
			
            // return FromFile( graphicsDevice, filename);
			// Resizing texture before returning it, not yet implemented			
        }

        public static Texture2D FromFile(GraphicsDevice graphicsDevice, string filename)
		{
			NSImage image = new NSImage(filename);
			if (image == null)
			{
				throw new ContentLoadException("Error loading file: " + filename);
			}
			
			ESImage theTexture = new ESImage(image, graphicsDevice.PreferedFilter);
			Texture2D result = new Texture2D(theTexture);
			result.Name = Path.GetFileNameWithoutExtension(filename);
			return result;
        }
		
        public void SetData<T>(T[] data, int startIndex, int elementCount, SetDataOptions options)
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount, SetDataOptions options)
        {
            throw new NotImplementedException();
        }

        public void GetData<T>(ref T[] data)
        {	
			if (data == null )
			{
				throw new ArgumentException("data cannot be null");
			}
			
			/*int mult = (this.Format == SurfaceFormat.Alpha8) ? 1 : 4;
			
			if (data.Length < Width * Height * mult)
			{
				throw new ArgumentException("data is the wrong length for Pixel Format");
			}*/
			
			int sz = 0;
						
			byte[] pixel = new byte[4];
			int pos;
			IntPtr pixelOffset;
			// Get the Color values
			if ((typeof(T) == typeof(Color))) 
			{	
				for(int y = 0; y < Height; y++ )
				{
					for( int x = 0; x < Width; x++ )
					{
						var result = new Color(0, 0, 0, 0);						
						
						switch(this.Format) 
						{
							case SurfaceFormat.Rgba32 /*kTexture2DPixelFormat_RGBA8888*/:
							case SurfaceFormat.Dxt3 :
							    sz = 4;
								pos = ( (y * Width) + x ) * sz;
								pixelOffset = new IntPtr(texture.PixelData.ToInt32() + pos);
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
							case SurfaceFormat.Rgb32 /*kTexture2DPixelFormat_RGB565*/:
								sz = 2;	
								pos = ( (y * Width) + x ) * sz;
								pixelOffset = new IntPtr(texture.PixelData.ToInt64() + pos);
								Marshal.Copy(pixelOffset, pixel, 0, 4);	
							
								result.R = pixel[0];
								result.G = pixel[1];
								result.B = pixel[2];					
								result.A = 255;
								break;
							case SurfaceFormat.Alpha8 /*kTexture2DPixelFormat_A8*/:
								sz = 1;
								pos = ( (y * Width) + x ) * sz;
								pixelOffset = new IntPtr(texture.PixelData.ToInt64() + pos);								
								Marshal.Copy(pixelOffset, pixel, 0, 4);	
							
								result.A = pixel[0];
								break;
							default:
								throw new NotSupportedException("Texture format");
						}
						data[((y * Width) + x)] = (T)(object)result;						
					}
				}
			}	
        }

        public void GetData<T>(T[] data, int startIndex, int elementCount)
        {
            throw new NotImplementedException();
        }

        public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
            throw new NotImplementedException();
        }
	}
}

