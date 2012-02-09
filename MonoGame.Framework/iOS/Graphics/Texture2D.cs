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

using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

#if ES11
using OpenTK.Graphics.ES11;
using GL_Oes = OpenTK.Graphics.ES11.GL.Oes;
#else
using OpenTK.Graphics.ES20;
using GL_Oes = OpenTK.Graphics.ES20.GL;
using TextureTarget = OpenTK.Graphics.ES20.All;
#endif

using Microsoft.Xna.Framework.Content;


namespace Microsoft.Xna.Framework.Graphics
{
    public class Texture2D : Texture
    {
		
		//OpenGL ES1.1 consts
#if ES11
		const All GLFramebuffer = All.FramebufferOes;
		const All GLRenderbuffer = All.RenderbufferOes;
		const All GLDepthAttachment = All.DepthAttachmentOes;
		const All GLColorAttachment0 = All.ColorAttachment0Oes;
		const All GLDepthComponent24 = All.DepthComponent24Oes;
		const All GLFramebufferComplete = All.FramebufferCompleteOes;
#else
		const All GLFramebuffer = All.Framebuffer;
		const All GLRenderbuffer = All.Renderbuffer;
		const All GLDepthAttachment = All.DepthAttachment;
		const All GLColorAttachment0 = All.ColorAttachment0;
		const All GLDepthComponent24 = All.DepthComponent24Oes;
		const All GLFramebufferComplete = All.FramebufferComplete;
#endif
		
		
		
		private ESImage texture;
		
		// Moved as per kjpou1 protected int textureId = -1;
		protected int _width;
		protected int _height;
		private bool _mipmap;
		private Rectangle _bounds = new Rectangle(0, 0, 0, 0);
		
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
		
        public Rectangle Bounds
        {
            get
            {
				_bounds.Width = _width;
				_bounds.Height = _height;
                return _bounds;
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
		
		public Texture2D(GraphicsDevice graphicsDevice, int width, int height): 
			this (graphicsDevice, width, height, false, SurfaceFormat.Color)
		{
			
		}
		
		public Texture2D(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, SurfaceFormat format)
		{
			this.graphicsDevice = graphicsDevice;	
			
			this._format = format;
			this._mipmap = mipMap;
				
#if ES11
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
				
			generateOpenGLTexture();
#else
			this._width = width;
			this._height = height;
			texture = new ESImage(width, height);
#endif
		}
		
		private void generateOpenGLTexture() 
		{
			// modeled after this
			// http://steinsoft.net/index.php?site=Programming/Code%20Snippets/OpenGL/no9
			
			GL.GenTextures(1,ref _textureId);
			GL.BindTexture(All.Texture2D, _textureId);
			
			if (_mipmap)
			{
				// Taken from http://www.flexicoder.com/blog/index.php/2009/11/iphone-mipmaps/
				GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.LinearMipmapNearest);
				GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
#if ES11
				GL.TexParameter(All.Texture2D, All.GenerateMipmap, (int)All.True);
#else
				GL.TexParameter(All.Texture2D, All.GenerateMipmapHint, (int)All.True);
#endif		
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
		
		public override void Dispose()
		{
			base.Dispose();

			if (texture != null) {
				texture.Dispose();
			}
			if (_textureId!=0) {
				GL.DeleteTextures(1, ref _textureId);
			}
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
			GL.BindTexture(All.Texture2D, _textureId);
			GL.CopyTexImage2D(All.Texture2D, 0, All.Rgba, 0, 0, Width, Height, 0);
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
		
        internal static Texture2D FromImage(GraphicsDevice graphicsDevice, UIImage image)
        {
			
			if (image == null)			
			{
				throw new ContentLoadException("Error loading Texture2D Stream");
			}
			
			ESImage theTexture = new ESImage(image, graphicsDevice.PreferedFilter);			
			Texture2D result = new Texture2D(theTexture);
			
			return result;
        }

        public static Texture2D FromStream(GraphicsDevice graphicsDevice, Stream textureStream)
        {
            MonoTouch.Foundation.NSData nsData = MonoTouch.Foundation.NSData.FromStream(textureStream);

			UIImage image = UIImage.LoadFromData(nsData);
			
			if (image == null)			
			{
				throw new ContentLoadException("Error loading Texture2D Stream");
			}
			
			ESImage theTexture = new ESImage(image, graphicsDevice.PreferedFilter);			
			Texture2D result = new Texture2D(theTexture);
			
			return result;
        }


        public static Texture2D FromStream(GraphicsDevice graphicsDevice, string filename, int width, int height)
        {
			UIImage image;
			if(filename.Contains(".pdf"))
				image = Extender.FromPdf(filename,width,height);
			else
				image = UIImage.FromFile(filename);
			if (image == null)
			{
				throw new ContentLoadException("Error loading file: " + filename);
			}			
			
			ESImage theTexture;
			
			if ( width == 0 && height == 0 )
			{
				theTexture = new ESImage(image, graphicsDevice.PreferedFilter);
			}
			else
			{
				var small = image.Scale (new SizeF (width, height));
				theTexture = new ESImage(small, graphicsDevice.PreferedFilter);
			}
			Texture2D result = new Texture2D(theTexture);
			// result.Name = Path.GetFileNameWithoutExtension(filename);
			result.Name = filename;
			return result;					
        }

		// Not sure what this should do in iOS will leave it non implemented for now.
		internal void Reload(Stream textureStream)
		{
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
			GL_Oes.GenFramebuffers(1, ref framebufferId);
			GL_Oes.BindFramebuffer(GLFramebuffer, framebufferId);
			
			//renderBufferIDs = new int[currentRenderTargets];
			GL_Oes.GenRenderbuffers(1, ref renderBufferID);
			
			// attach the texture to FBO color attachment point
			GL_Oes.FramebufferTexture2D(GLFramebuffer, GLColorAttachment0,
				All.Texture2D, ID,0);
			
			// create a renderbuffer object to store depth info
			GL_Oes.BindRenderbuffer(GLRenderbuffer, renderBufferID);
			GL_Oes.RenderbufferStorage(GLRenderbuffer, GLDepthComponent24,
				_width, _height);
			
			// attach the renderbuffer to depth attachment point
			GL_Oes.FramebufferRenderbuffer(GLFramebuffer, GLDepthAttachment,
				GLRenderbuffer, renderBufferID);
				
			All status = GL_Oes.CheckFramebufferStatus(GLFramebuffer);
			
			if (status != GLFramebufferComplete)
				throw new Exception("Error creating framebuffer: " + status);
			
			byte[] imageInfo;
			int sz = 0;
			
			switch (_format) {
			case SurfaceFormat.Color : //kTexture2DPixelFormat_RGBA8888
				
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
			
			GL.ReadPixels(0,0, _width, _height, All.Rgba, All.UnsignedByte, imageInfo);

			// Detach the render buffers.
			GL_Oes.FramebufferRenderbuffer(GLFramebuffer, GLDepthAttachment,
					GLRenderbuffer, 0);
			// delete the RBO's
			GL_Oes.DeleteRenderbuffers(1,ref renderBufferID);
			// delete the FBO
			GL_Oes.DeleteFramebuffers(1, ref framebufferId);
			// Set the frame buffer back to the system window buffer
			GL_Oes.BindFramebuffer(GLFramebuffer, 0);			

			return imageInfo;
					
		}
		
		
		public void GetData<T> (T[] data)
		{	

			if (data == null) {
				throw new ArgumentException ("data cannot be null");
			}
			
			if (data == null) {
				throw new ArgumentException ("data cannot be null");
			}
			
			GetData(0, null, data, 0, data.Length);
			
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

				if (texture == null) {
					// For rendertargets we need to loop through and load the elements
					// backwards because the texture data is flipped vertically and horizontally
					var dataEnd = (rWidth * rHeight) - 1;
					var dataPos = 0;
					var dataRowColOffset = 0;
					for (int y = r.Top; y < rHeight; y++) {
						for (int x = r.Left; x < rWidth; x++) {
							var result = new Color (0, 0, 0, 0);						
							dataRowColOffset = ((y * rWidth) + x);
							switch (_format) {
							case SurfaceFormat.Color : //kTexture2DPixelFormat_RGBA8888
								
								dataPos = dataRowColOffset * 4;								
															
								result.R = imageInfo [dataPos];
								result.G = imageInfo [dataPos + 1];
								result.B = imageInfo [dataPos + 2];
								result.A = imageInfo [dataPos + 3];
								break;
							default:
								throw new NotSupportedException ("Texture format");
							}
							data [dataEnd - dataRowColOffset] = (T)(object)result;
						}


					}
				} else {
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
				}
			} else {
				throw new NotImplementedException ("GetData not implemented for type.");
			}
		}
		
		internal override TextureTarget GLTarget {
			get { return TextureTarget.Texture2D; }
		}
	}
}

