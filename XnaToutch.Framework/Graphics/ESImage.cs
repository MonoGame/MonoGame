#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

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
using MonoTouch.UIKit;
using OpenTK.Graphics.ES11;

namespace XnaTouch.Framework.Graphics
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
		// The maximum texture coordinate width maximum 1.0f
		private float maxTexWidth;
		// The maximum texture coordinate height maximum 1.0f
		private float maxTexHeight;
		// The texture width to pixel ratio
		private float texWidthRatio;
		// The texture height to pixel ratio
		private float texHeightRatio;
		// The X offset to use when looking for our image
		private int	textureOffsetX;
		// The Y offset to use when looking for our image
		private int textureOffsetY;
		// Angle to which the image should be rotated
		private float _rotation;
		// Scale at which to draw the image
		private float _vscale, _hscale;
		// Colour Filter = Red, Green, Blue, Alpha
		private Vector4 colourFilter;
		// Horizontal Flip
		private bool _flipHorizontal;
		// Vertical Flip
		private bool _flipVertical;
		// Image origin
		private Vector2 _origin;
		
		
		public ESImage ()
		{
			imageWidth = 0;
			imageHeight = 0;
			textureWidth = 0;
			textureHeight = 0;
			texWidthRatio = 0.0f;
			texHeightRatio = 0.0f;
			maxTexWidth = 0.0f;
			maxTexHeight = 0.0f;
			textureOffsetX = 0;
			textureOffsetY = 0;
			_rotation = 0.0f;
			_hscale = 1.0f;
			_vscale = 1.0f;
			colourFilter.X = 1.0f;
			colourFilter.Y = 1.0f;
			colourFilter.Z = 1.0f;
			colourFilter.W = 1.0f;
			_flipVertical = false;
			_flipHorizontal = false;
			_origin = Vector2.Zero;
		}
		
		private void initImpl()
		{
			imageWidth = texture.ContentSize.Width;
			imageHeight = texture.ContentSize.Height;
			textureWidth = texture.PixelsWide;
			textureHeight = texture.PixelsHigh;
			maxTexWidth = imageWidth / (float)textureWidth;
			maxTexHeight = imageHeight / (float)textureHeight;
			texWidthRatio = 1.0f / (float)textureWidth;
			texHeightRatio = 1.0f / (float)textureHeight;
			textureOffsetX = 0;
			textureOffsetY = 0;
			_rotation = 0.0f;
			colourFilter.X = 1.0f;
			colourFilter.Y = 1.0f;
			colourFilter.Z = 1.0f;
			colourFilter.W = 1.0f;
			_origin = Vector2.Zero;
		}

		
		public ESImage(ESTexture2D tex)
		{
			texture = tex;
			_hscale = 1.0f;
			_vscale = 1.0f;
			initImpl();
		}

		public ESImage(ESTexture2D tex, float imageScale)
		{
			texture = tex;
			_hscale = imageScale;
			_vscale = imageScale;
			initImpl();
		}
		
		public ESImage(UIImage image)
		{
			// By default set the scale to 1.0f and the filtering to GL_NEAREST
			texture = new ESTexture2D(image,All.Nearest);
			_hscale = 1.0f;
			_vscale = 1.0f;
			initImpl();
		}

		public ESImage(UIImage image, All filter)
		{			
			// By default set the scale to 1.0f
			texture = new ESTexture2D(image,filter);
			_hscale = 1.0f;
			_vscale = 1.0f;
			initImpl();
		}
		
		public ESImage(UIImage image, float imageScale, All filter)
		{
			texture = new ESTexture2D(image,filter);
			_hscale = imageScale;
			_vscale = imageScale;
			initImpl();
		}
		
		public float HorizontalScale 
		{
			get 
			{
				return _hscale;
			}
			set 
			{
				_hscale = value;
			}
		}
		
		public float VerticalScale 
		{
			get 
			{
				return _vscale;
			}
			set 
			{
				_vscale = value;
			}
		}
		
		public Vector2 Origin
		{
			get
			{
				return _origin;
			}
			
			set 
			{
				_origin = value;
			}
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
		
		public bool FlipHorizontal
		{
			get
			{
				return _flipHorizontal;
			}
			set 
			{
				_flipHorizontal = value;
			}
		}
		
		public bool FlipVertical
		{
			get
			{
				return _flipVertical;
			}
			set 
			{
				_flipVertical = value;
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
		
		public float Rotation
		{
			get 
			{
				return _rotation;
			}
			set 
			{
				_rotation = value;
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
	
			// Set the rotatoin of the subImage to match the current images rotation
			subImage.Rotation = _rotation;
	
			return subImage;
		}
		
		public void GetTextureVertices (float[] quadVertices, int index, float subImageWidth, float subImageHeight)
		{
			float quadWidth = subImageWidth * _hscale;
			float quadHeight = subImageHeight * _vscale;
			
			if (!_flipVertical && !_flipHorizontal) {
				quadVertices[0+index] = quadWidth - _origin.X;
				quadVertices[1+index] = quadHeight - _origin.Y;
				quadVertices[2+index] = quadWidth - _origin.X;
				quadVertices[3+index] = -_origin.Y;
				quadVertices[4+index] = -_origin.X;
				quadVertices[5+index] = quadHeight - _origin.Y;
				quadVertices[6+index] = -_origin.X;
				quadVertices[7+index] = -_origin.Y;
			}
			if (!_flipVertical && _flipHorizontal) {
				quadVertices[0+index] = -_origin.X;
				quadVertices[1+index] = quadHeight - _origin.Y;
				quadVertices[2+index] = -_origin.X;
				quadVertices[3+index] = -_origin.Y;
				quadVertices[4+index] = quadWidth - _origin.X;
				quadVertices[5+index] = quadHeight - _origin.Y;
				quadVertices[6+index] = quadWidth - _origin.X;
				quadVertices[7+index] = -_origin.Y;
			}
			if (_flipVertical && !_flipHorizontal) {
				quadVertices[0+index] = quadWidth - _origin.X;
				quadVertices[1+index] = -_origin.Y;
				quadVertices[2+index] = quadWidth - _origin.X;
				quadVertices[3+index] = quadHeight - _origin.Y;
				quadVertices[4+index] = -_origin.X;
				quadVertices[5+index] = -_origin.Y;
				quadVertices[6+index] = -_origin.X;
				quadVertices[7+index] = quadHeight - _origin.Y;
			}
			if (_flipVertical && _flipHorizontal) {
				quadVertices[0+index] = -_origin.X;
				quadVertices[1+index] = -_origin.Y;
				quadVertices[2+index] = -_origin.X;
				quadVertices[3+index] = quadHeight - _origin.Y;
				quadVertices[4+index] = quadWidth - _origin.X;
				quadVertices[5+index] = -_origin.Y;
				quadVertices[6+index] = quadWidth - _origin.X;
				quadVertices[7+index] = quadHeight - _origin.Y;
			}			
		}

		public void GetTextureCoordinates (float[] textureCoordinates, int index, Rectangle TextureRect)
		{	
			textureCoordinates [0+index] = texWidthRatio * TextureRect.Width + (texWidthRatio * TextureRect.Left);
			textureCoordinates [1+index] = texHeightRatio * TextureRect.Top;
			textureCoordinates [2+index] = texWidthRatio * TextureRect.Width + (texWidthRatio * TextureRect.Left);
			textureCoordinates [3+index] = texHeightRatio * TextureRect.Height + (texHeightRatio * TextureRect.Top);
			textureCoordinates [4+index] = texWidthRatio * TextureRect.Left;
			textureCoordinates [5+index] = texHeightRatio * TextureRect.Top;
			textureCoordinates [6+index] = texWidthRatio * TextureRect.Left;
			textureCoordinates [7+index] = texHeightRatio * TextureRect.Height + (texHeightRatio * TextureRect.Top);
		}

		public Vector4 FilterColor
		{
			set 
			{
				colourFilter = value;
			}
			get 
			{
				return colourFilter;
			}
			
		}
		
		public void SetAlpha(float alpha)
		{
			colourFilter.W = alpha;
		}
		
		public uint Name {
			get
			{
				return texture.Name;
			}
		}
	}
}
