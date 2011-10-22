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
using Android.Graphics;
using Microsoft.Xna.Framework.Content;
using OpenTK.Graphics.ES11;
using Path = System.IO.Path;

namespace Microsoft.Xna.Framework.Graphics
{
    public class Texture2D : Texture
    {
		private ESImage texture;
		protected int _width;
		protected int _height;
		private bool _mipmap;
		
		internal bool IsSpriteFontTexture {get;set;}
		
		// my change
		// --------
		public uint ID
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
                return new Rectangle(0,0,texture.ImageWidth, texture.ImageHeight);
            }
        }
		
		public Rectangle Bounds {
			get {
				return new Rectangle (0,0,texture.ImageWidth, texture.ImageHeight);
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
			
			// This is needed in OpenGL ES 1.1 as it only supports power of 2 textures
            int xTexSize = 1;
            int yTexSize = 1;
            while (width > xTexSize && height > yTexSize)
            {
                if (width > xTexSize) xTexSize *= 2;
                if (height > yTexSize) yTexSize *= 2;
            }

            this._width = xTexSize;
            this._height = yTexSize;
			
			this._format = format;
			this._mipmap = mipMap;
			
			generateOpenGLTexture();
		}
		
		private void generateOpenGLTexture() 
		{
			// modeled after this
			// http://steinsoft.net/index.php?site=Programming/Code%20Snippets/OpenGL/no9

            GL.Enable(All.Texture2D);
			GL.GenTextures(1,ref _textureId);
			GL.BindTexture(All.Texture2D, _textureId);
			
			if (_mipmap)
			{
				// Taken from http://www.flexicoder.com/blog/index.php/2009/11/iphone-mipmaps/
				GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.LinearMipmapNearest);
				GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
				GL.TexParameter(All.Texture2D, All.GenerateMipmap, (int)All.True);
			}
			else
			{
				GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Linear);
				GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
			}
			
			byte[] textureData = new byte[(_width * _height) * 4];
			
			GL.TexImage2D(All.Texture2D, 0, (int)All.Rgba, _width, _height, 0, All.Rgba, All.UnsignedByte, textureData);
			
			GL.BindTexture(All.Texture2D, 0);
			
		}

        public Color GetPixel(int x, int y)
        {
            var result = new Color(0, 0, 0, 0);

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

        public TextureUsage TextureUsage
        {
            get 
			{ 
				throw new NotImplementedException();
			}
        }

        public static Texture2D FromFile(GraphicsDevice graphicsDevice, Stream textureStream)
        {
            Bitmap image = BitmapFactory.DecodeStream(textureStream);
			
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
            Bitmap image = BitmapFactory.DecodeFile(filename);
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

        private byte[] GetImageData(int level)
        {

            int framebufferId = -1;
            int renderBufferID = -1;

            // create framebuffer
            GL.Oes.GenFramebuffers(1, ref framebufferId);
            GL.Oes.BindFramebuffer(All.FramebufferOes, framebufferId);

            //renderBufferIDs = new int[currentRenderTargets];
            GL.Oes.GenRenderbuffers(1, ref renderBufferID);

            // attach the texture to FBO color attachment point
            GL.Oes.FramebufferTexture2D(All.FramebufferOes, All.ColorAttachment0Oes,
                All.Texture2D, ID, 0);

            // create a renderbuffer object to store depth info
            GL.Oes.BindRenderbuffer(All.RenderbufferOes, renderBufferID);
            GL.Oes.RenderbufferStorage(All.RenderbufferOes, All.DepthComponent24Oes,
                _width, _height);

            // attach the renderbuffer to depth attachment point
            GL.Oes.FramebufferRenderbuffer(All.FramebufferOes, All.DepthAttachmentOes,
                All.RenderbufferOes, renderBufferID);

            All status = GL.Oes.CheckFramebufferStatus(All.FramebufferOes);

            if (status != All.FramebufferCompleteOes)
                throw new Exception("Error creating framebuffer: " + status);

            byte[] imageInfo;
            int sz = 0;

            switch (_format)
            {
                case SurfaceFormat.Color: //kTexture2DPixelFormat_RGBA8888
                case SurfaceFormat.Dxt3:

                    sz = 4;
                    imageInfo = new byte[(_width * _height) * sz];
                    break;
                case SurfaceFormat.Bgra4444: //kTexture2DPixelFormat_RGBA4444
                    sz = 2;
                    imageInfo = new byte[(_width * _height) * sz];

                    break;
                case SurfaceFormat.Bgra5551: //kTexture2DPixelFormat_RGB5A1
                    sz = 2;
                    imageInfo = new byte[(_width * _height) * sz];
                    break;
                case SurfaceFormat.Alpha8:  // kTexture2DPixelFormat_A8 
                    sz = 1;
                    imageInfo = new byte[(_width * _height) * sz];
                    break;
                default:
                    throw new NotSupportedException("Texture format");
            }

            GL.ReadPixels(0, 0, _width, _height, All.Rgba, All.UnsignedByte, imageInfo);

            // Detach the render buffers.
            GL.Oes.FramebufferRenderbuffer(All.FramebufferOes, All.DepthAttachmentOes,
                    All.RenderbufferOes, 0);
            // delete the RBO's
            GL.Oes.DeleteRenderbuffers(1, ref renderBufferID);
            // delete the FBO
            GL.Oes.DeleteFramebuffers(1, ref framebufferId);
            // Set the frame buffer back to the system window buffer
            GL.Oes.BindFramebuffer(All.FramebufferOes, 0);

            return imageInfo;

        }


        public void GetData<T>(T[] data)
        {

            if (data == null)
            {
                throw new ArgumentException("data cannot be null");
            }

            if (data == null)
            {
                throw new ArgumentException("data cannot be null");
            }

            GetData(0, null, data, 0, data.Length);

        }

        public void GetData<T>(T[] data, int startIndex, int elementCount)
        {
            GetData<T>(0, null, data, startIndex, elementCount);
        }

        public void GetData<T>(int level, Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
            if (data == null)
            {
                throw new ArgumentException("data cannot be null");
            }

            if (data.Length < startIndex + elementCount)
            {
                throw new ArgumentException("The data passed has a length of " + data.Length + " but " + elementCount + " pixels have been requested.");
            }

            Rectangle r;
            if (rect != null)
            {
                r = rect.Value;
            }
            else
            {
                r = new Rectangle(0, 0, Width, Height);
            }

            byte[] imageInfo = GetImageData(0);

            // Get the Color values
            if ((typeof(T) == typeof(Color)))
            {


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

                if (texture == null)
                {
                    // For rendertargets we need to loop through and load the elements
                    // backwards because the texture data is flipped vertically and horizontally
                    var dataEnd = (rWidth * rHeight) - 1;
                    var dataPos = 0;
                    var dataRowColOffset = 0;
                    for (int y = r.Top; y < rHeight; y++)
                    {
                        for (int x = r.Left; x < rWidth; x++)
                        {
                            var result = new Color(0, 0, 0, 0);
                            dataRowColOffset = ((y * rWidth) + x);
                            switch (_format)
                            {
                                case SurfaceFormat.Color: //kTexture2DPixelFormat_RGBA8888
                                case SurfaceFormat.Dxt3:

                                    dataPos = dataRowColOffset * 4;

                                    result.R = imageInfo[dataPos];
                                    result.G = imageInfo[dataPos + 1];
                                    result.B = imageInfo[dataPos + 2];
                                    result.A = imageInfo[dataPos + 3];
                                    break;
                                default:
                                    throw new NotSupportedException("Texture format");
                            }
                            data[dataEnd - dataRowColOffset] = (T)(object)result;
                        }


                    }
                }
                else
                {
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
                            switch (_format)
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
            }
            else
            {
                throw new NotImplementedException("GetData not implemented for type.");
            }
        }
	}
}

