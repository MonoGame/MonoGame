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
		protected int textureId = -1;
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
					return (uint)textureId;
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
		}
		
		public Texture2D(GraphicsDevice graphicsDevice, int width, int height, int numberLevels, TextureUsage usage, SurfaceFormat format)
        {
			throw new NotImplementedException();
        }
		
        public Texture2D(Texture2D source, Color color)
        {
            throw new NotImplementedException();
        }
		
		public Texture2D(GraphicsDevice graphicsDevice, int width, int height)
		{
			throw new NotImplementedException();
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
		
		private void generateOpenGLTexture() 
		{
			// modeled after this
			// http://steinsoft.net/index.php?site=Programming/Code%20Snippets/OpenGL/no9
			
			GL.GenTextures(1,ref textureId);
			GL.BindTexture(All.Texture2D, textureId);
			
			GL.TexParameter(All.Texture2D, All.TextureMinFilter, (int)All.Linear);
			GL.TexParameter(All.Texture2D, All.TextureMagFilter, (int)All.Linear);
			
			GL.BindTexture(All.Texture2D, 0);
			
		}

        public Color GetPixel(int x, int y)
        {
            throw new NotImplementedException();
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

        public void GetData<T>(T[] data)
        {
            throw new NotImplementedException();
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

