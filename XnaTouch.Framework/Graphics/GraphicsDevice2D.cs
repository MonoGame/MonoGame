// #region License
// /*
// Microsoft Public License (Ms-PL)
// XnaTouch - Copyright © 2009 The XnaTouch Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 

using System;
using System.Collections.Generic;
using OpenTK.Graphics.ES11;
using System.Diagnostics;

namespace XnaTouch.Framework.Graphics
{
	internal class TextureComparer : IComparer<SpriteBuffer>
    {
        public int Compare(SpriteBuffer r1, SpriteBuffer r2)
        {
            if (r1.RenderData.Texture.Image.Name > r2.RenderData.Texture.Image.Name)
            {
                return -1;
            }
            if (r1.RenderData.Texture.Image.Name < r2.RenderData.Texture.Image.Name)
            {
                return 1;
            }
            return 0;
        }
    }
	
	internal class BackToFrontComparer : IComparer<SpriteBuffer>
    {
        public int Compare(SpriteBuffer r1, SpriteBuffer r2)
        {
            if (r1.RenderData.LayerDepth > r2.RenderData.LayerDepth)
            {
                return -1;
            }
            if (r1.RenderData.LayerDepth < r2.RenderData.LayerDepth)
            {
                return 1;
            }
            return 0;
        }
    }

    internal class FrontToBackComparer : IComparer<SpriteBuffer>
    {
        public int Compare(SpriteBuffer r1, SpriteBuffer r2)
        {
            if (r1.RenderData.LayerDepth > r2.RenderData.LayerDepth)
            {
                return 1;
            }
            if (r1.RenderData.LayerDepth < r2.RenderData.LayerDepth)
            {
                return -1;
            }
            return 0;
        }
    }
	
	internal class RenderMode
	{
		public RenderMode()
		{
			Rotation = 0.0f;
			HorizontalScale = 1.0f;
			VerticalScale = 1.0f;
			FilterColor = Color.White;
			Origin = Vector2.Zero;
			LayerDepth = 0.0f;
		}
		
		public bool IsCompatible(RenderMode mode)
		{
			if (FilterColor != mode.FilterColor)
			{
				return false;
			}
			
			return true;
		}
		
		public float Rotation {get;set;}
		public Color FilterColor {get;set;}
		public Vector2 Origin {get;set;}
		public bool FlipVertical {get;set;}
		public bool FlipHorizontal {get;set;}
		public float HorizontalScale {get;set;}
		public float VerticalScale {get;set;}
		public float LayerDepth {get;set;}
		public Texture2D Texture {get;set;}
	}
	
	internal class SpriteBuffer
	{
		public SpriteBuffer()
		{
		}
					
		public Vector2 Position {get;set;}
		public Rectangle TextureRect {get;set;}
		public RenderMode RenderData {get;set;}
	}
		
	internal class GraphicsDevice2D
	{
		private GraphicsDevice _device;
		private readonly ReusableItemList<SpriteBuffer> _sprites = new ReusableItemList<SpriteBuffer>();
		private readonly ReusableItemList<SpriteBuffer> _sortedSprites = new ReusableItemList<SpriteBuffer>();
		private SpriteBlendMode _actualBlendMode, _previousBlendMode = SpriteBlendMode.None;
		private SpriteSortMode _actualSortMode = SpriteSortMode.Deferred; 
		private Vector2 []_spritVertices = new Vector2[4]; 
		
		public GraphicsDevice2D (GraphicsDevice Device)
		{
			_device = Device;
		}
		
		public void GetSpriteVerticesToPoint(Vector2 position, int index, float[] quadVertices, float Width, float Height, RenderMode renderMode)
		{
			float quadWidth = Width * renderMode.HorizontalScale;
			float quadHeight = Height * renderMode.VerticalScale;
			
			if (!renderMode.FlipVertical && !renderMode.FlipHorizontal) 
			{
				_spritVertices[0] = new Vector2(quadWidth,quadHeight);
				_spritVertices[1] = new Vector2(quadWidth,0);
				_spritVertices[2] = new Vector2(0,quadHeight);
				_spritVertices[3] = new Vector2(0,0);				
			}
			if (!renderMode.FlipVertical && renderMode.FlipHorizontal) 
			{				
				_spritVertices[0] = new Vector2(0,quadHeight);
				_spritVertices[1] = new Vector2(0,0);
				_spritVertices[2] = new Vector2(quadWidth,quadHeight);
				_spritVertices[3] = new Vector2(quadWidth,0);	
			}
			if (renderMode.FlipVertical && !renderMode.FlipHorizontal) 
			{				
				_spritVertices[0] = new Vector2(quadWidth,0);
				_spritVertices[1] = new Vector2(quadWidth,quadHeight);
				_spritVertices[2] = new Vector2(0,0);
				_spritVertices[3] = new Vector2(0,quadHeight);	
			}
			if (renderMode.FlipVertical && renderMode.FlipHorizontal) 
			{
				_spritVertices[0] = new Vector2(0,0);
				_spritVertices[1] = new Vector2(0,quadHeight);
				_spritVertices[2] = new Vector2(quadWidth,0);
				_spritVertices[3] = new Vector2(quadWidth,quadHeight);	
			}
			
			// Adjusted origin
			Vector2 adjustableOrigin;
			
			// Rotate
			if (renderMode.Rotation != 0.0f) 
			{				
				Matrix rotation = Matrix.CreateRotationZ(renderMode.Rotation);
				for (int i = 0; i < 4; i++)
                	_spritVertices[i] = Vector2.Transform(_spritVertices[i]-renderMode.Origin, rotation);
				
				// Set the origin
				adjustableOrigin = new Vector2(0,-renderMode.Origin.Y*2);					
			}
			else 
			{
				// Set the origin
				adjustableOrigin = new Vector2(renderMode.Origin.X,-renderMode.Origin.Y);					
			}
						
			// Translate to sprite positon
			Matrix translation = Matrix.CreateTranslation(position.X-adjustableOrigin.X,position.Y-adjustableOrigin.Y,0);
			for (int i = 0; i < 4; i++)
                _spritVertices[i] = Vector2.Transform(_spritVertices[i], translation);
			
			// put in the array
			quadVertices[0+index] = _spritVertices[0].X;
			quadVertices[1+index] = _spritVertices[0].Y;
			quadVertices[2+index] = _spritVertices[1].X;
			quadVertices[3+index] = _spritVertices[1].Y;
			quadVertices[4+index] = _spritVertices[2].X;
			quadVertices[5+index] = _spritVertices[2].Y;
			quadVertices[6+index] = _spritVertices[3].X;
			quadVertices[7+index] = _spritVertices[3].Y;
		}		
			
		public void RenderSprites(int itemCount, Vector2 point, float[] texCoords, float[] quadVertices, RenderMode renderMode)
		{
			if (itemCount == 0) return;
		
			// Enable Texture_2D
			GL.Enable(All.Texture2D);

			// Set the glColor to apply alpha to the image
			Vector4 color = renderMode.FilterColor.ToEAGLColor();			
			GL.Color4(color.X, color.Y, color.Z, color.W);
	
			// Set client states so that the Texture Coordinate Array will be used during rendering
			GL.EnableClientState(All.TextureCoordArray);
				
			// Bind to the texture that is associated with this image
			if (_device.ActiveTexture != renderMode.Texture.Image.Name) 
			{
				GL.BindTexture(All.Texture2D, renderMode.Texture.Image.Name);
				_device.ActiveTexture = (int) renderMode.Texture.Image.Name;
			}
			
			// Set up the VertexPointer to point to the vertices we have defined
			GL.VertexPointer(2, All.Float, 0, quadVertices);
			
			// Set up the TexCoordPointer to point to the texture coordinates we want to use
			GL.TexCoordPointer(2, All.Float, 0, texCoords);

			// Draw the vertices to the screen
			if (itemCount > 1) 
			{
				ushort[] indices = new ushort[itemCount*6];
				for (int i=0;i<itemCount;i++)
				{
					indices[i*6+0] = (ushort) (i*4+0);
					indices[i*6+1] = (ushort) (i*4+1);
					indices[i*6+2] = (ushort) (i*4+2);
					indices[i*6+5] = (ushort) (i*4+1);
					indices[i*6+4] = (ushort) (i*4+2);
					indices[i*6+3] = (ushort) (i*4+3);			
				}
				// Draw triangles
				GL.DrawElements(All.Triangles,itemCount*6,All.UnsignedShort,indices);
			}
			else {				
				// Draw the vertices to the screen
				GL.DrawArrays(All.TriangleStrip, 0, 4);
			}
			// Disable as necessary
			GL.DisableClientState(All.TextureCoordArray);
			
			// Disable 2D textures
			GL.Disable(All.Texture2D);
		}		
		
		public void StartSpriteBatch(SpriteBlendMode blendMode, SpriteSortMode sortMode)
		{
			if (_sprites.Count > 0)
			{
				throw new InvalidOperationException("SpriteBatch in incorrect state");
			}
			
			_actualBlendMode = blendMode;
			_actualSortMode = sortMode;
		}
		
		public void EndSpriteBatch()
		{
			if (_actualSortMode != SpriteSortMode.Deferred)
			{
				SortSprites();
			}
			DrawSprites(_sprites);
			_sprites.Clear();
		}
		
		private void DrawSprites(ReusableItemList<SpriteBuffer> spritesToDraw)
		{
			// Draw the current spritebatch
			if (spritesToDraw.Count > 0)
			{				
				if (_previousBlendMode != _actualBlendMode)
				{
					switch (_previousBlendMode)
					{
						case SpriteBlendMode.Additive :
						case SpriteBlendMode.AlphaBlend :
							GL.Disable(All.Blend);
							break;
						case SpriteBlendMode.None :
							break;
					}
					_previousBlendMode = _actualBlendMode;
					_device.ActiveTexture = -1;
				}
				// Set the current blend mode
				switch (_actualBlendMode)
				{
					case SpriteBlendMode.Additive :					
						GL.Enable(All.Blend);
						GL.BlendFunc(All.SrcAlpha,All.One);
						break;
					case SpriteBlendMode.AlphaBlend :
						GL.Enable(All.Blend);
						GL.BlendFunc(All.SrcAlpha, All.OneMinusSrcAlpha);					
						break;
					case SpriteBlendMode.None :
						break;
				}
				
				float[] Vertices = new float[spritesToDraw.Count*8];
				float[] TextureCoords = new float[spritesToDraw.Count*8];
				int i=0;
				RenderMode actualMode = spritesToDraw[0].RenderData;
				
				// Draw sprite vertices
				foreach(SpriteBuffer sb in spritesToDraw)
				{
					if ((!actualMode.IsCompatible(sb.RenderData)) || (actualMode.Texture.Image.Name != sb.RenderData.Texture.Image.Name))
					{
						// Render sprites
						RenderSprites(i/8,Vector2.Zero,TextureCoords,Vertices,actualMode);
						i=0;
						actualMode = sb.RenderData;
					}
						    						
					// put the vertices in vertex array
					GetSpriteVerticesToPoint(sb.Position,i,Vertices,sb.TextureRect.Width,sb.TextureRect.Height,sb.RenderData);
					sb.RenderData.Texture.Image.GetTextureCoordinates(TextureCoords,i,sb.TextureRect);
					i+=8;
				}
				
				// Render the sprites
				RenderSprites(i/8,Vector2.Zero,TextureCoords,Vertices,spritesToDraw[spritesToDraw.Count-1].RenderData);
			}						
		}
		
		public void AddToSpriteBuffer(RenderMode renderInfo, Vector2 point, Rectangle textureRect)
		{
			SpriteBuffer sb = _sprites.GetNewItem();
			if (sb == null) 
			{
				sb = new SpriteBuffer();
				_sprites.Add(sb);
			}
			
			sb.Position = point;
			sb.TextureRect = textureRect;
			sb.RenderData = renderInfo;
		}
		
		private void SortSprites()
        {
			IComparer<SpriteBuffer> comparer;
			
            switch (_actualSortMode)
            {
                case SpriteSortMode.Texture:
                    comparer = new TextureComparer();
                    break;

                case SpriteSortMode.BackToFront:
                    comparer = new BackToFrontComparer();
                    break;

                case SpriteSortMode.FrontToBack:
                    comparer = new FrontToBackComparer();
                    break;
                default:
                    throw new NotSupportedException();
            }
			_sprites.Sort(comparer);
        }
	}
}
