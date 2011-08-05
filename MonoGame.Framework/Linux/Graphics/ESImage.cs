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
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class ESImage
	{
		// The OpenGL texture to be used for this image
		private ESTexture2D	texture;	
		// The width of the image
		private int	imageWidth;
		// The height of the image
		private int imageHeight;
		// The texture coordinate width to use to find the image
		private int textureWidth;
		// The texture coordinate height to use to find the image
		private int textureHeight;
		// The texture width to pixel ratio
		private float texWidthRatio;
		// The texture height to pixel ratio
		private float texHeightRatio;
		// The X offset to use when looking for our image
		private int	textureOffsetX;
		// The Y offset to use when looking for our image
		private int textureOffsetY;
		
		public ESImage ()
		{
			imageWidth = 0;
			imageHeight = 0;
			textureWidth = 0;
			textureHeight = 0;
			texWidthRatio = 0.0f;
			texHeightRatio = 0.0f;
			textureOffsetX = 0;
			textureOffsetY = 0;
		}
		
		private void Initialize( float scale )
		{
			imageWidth = texture.ContentSize.Width;
			imageHeight = texture.ContentSize.Height;
			textureWidth = (int)(texture.PixelsWide/scale);
			textureHeight = (int)(texture.PixelsHigh/scale);
			texWidthRatio = 1.0f / (float)textureWidth;
			texHeightRatio = 1.0f / (float)textureHeight;
			textureOffsetX = 0;
			textureOffsetY = 0;
		}

		public ESImage(ESTexture2D tex)
		{
			texture = tex;
			Initialize(1.0f);
		}

		public ESImage(ESTexture2D tex, float imageScale)
		{
			texture = tex;
			Initialize(1.0f);
		}
		
		public ESImage(Bitmap image)
		{
			// By default set the scale to 1.0f and the filtering to GL_NEAREST
			texture = new ESTexture2D(image,All.Nearest);
            Initialize(1.0f);			
		}

        public ESImage(Bitmap image, All filter)
		{			
			// By default set the scale to 1.0f
			texture = new ESTexture2D(image,filter);
            Initialize(1.0f);
		}

        public ESImage(Bitmap image, float imageScale, All filter)
		{
			texture = new ESTexture2D(image,filter);
            Initialize(imageScale);
		}
				
		public int TextureOffsetX 
		{
			get 
			{
				return textureOffsetX;
			}
			set 
			{
				textureOffsetX = value;
			}
		}
		
		public int TextureOffsetY 
		{
			get 
			{
				return textureOffsetY;
			}
			set 
			{
				textureOffsetY = value;
			}
		}
		
		public int ImageWidth 
		{
			get 
			{
				return imageWidth;
			}
			set 
			{
				imageWidth = value;
			}
		}
				
		public int ImageHeight
		{
			get 
			{
				return imageHeight;
			}
			set 
			{
				imageHeight = value;
			}
		}
			
		public ESImage GetSubImageAtPoint(Vector2 point, int subImageWidth, int subImageHeight, float subImageScale)
		{
			//Create a new Image instance using the texture which has been assigned to the current instance
			ESImage subImage = new ESImage(texture,subImageScale);
			// Define the offset of the subimage we want using the point provided
			subImage.TextureOffsetX = (int) point.X;
			subImage.TextureOffsetY = (int) point.Y;
	
			// Set the width and the height of the subimage
			subImage.ImageWidth = subImageWidth;
			subImage.ImageHeight = subImageHeight;
	
			return subImage;
		}
		
		public float GetTextureCoordX( int x )
		{
			return (x*texWidthRatio);
		}
		
		public float GetTextureCoordY( int y )
		{
			return (y*texHeightRatio);
		}
		
		public Vector2[] GetTextureCoordinates(Rectangle textureRect)
		{
			Vector2[] coordinates = new Vector2[4];
			
			coordinates[0] = new Vector2(texWidthRatio * textureRect.Width + (texWidthRatio * textureRect.Left),texHeightRatio * textureRect.Top);
			coordinates[1] = new Vector2(texWidthRatio * textureRect.Width + (texWidthRatio * textureRect.Left),texHeightRatio * textureRect.Height + (texHeightRatio * textureRect.Top));
			coordinates[2] = new Vector2(texWidthRatio * textureRect.Left ,texHeightRatio * textureRect.Top);
			coordinates[3] = new Vector2(texWidthRatio * textureRect.Left,texHeightRatio * textureRect.Height + (texHeightRatio * textureRect.Top));
			
			return coordinates;		
		}
		
		public uint Name 
		{
			get
			{
				return texture.Name;
			}
		}
		
		public IntPtr PixelData
		{
			get
			{
				return texture.PixelData;
			}
		}
		
		public SurfaceFormat Format
        {
            get 
			{ 
				return texture.PixelFormat;
			}
        }
	}
}
