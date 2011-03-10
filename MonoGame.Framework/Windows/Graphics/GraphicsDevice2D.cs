// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
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
using System.Diagnostics;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
	internal class TextureComparer : IComparer<SpriteBatchRenderItem>
    {
        public int Compare(SpriteBatchRenderItem r1, SpriteBatchRenderItem r2)
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
	
	internal class BackToFrontComparer : IComparer<SpriteBatchRenderItem>
    {
        public int Compare(SpriteBatchRenderItem r1, SpriteBatchRenderItem r2)
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

    internal class FrontToBackComparer : IComparer<SpriteBatchRenderItem>
    {
        public int Compare(SpriteBatchRenderItem r1, SpriteBatchRenderItem r2)
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
	
	internal class SpriteBatchRenderItem
	{
		public Vector2 Position {get;set;}
		public Vector2[] Vertices {get;set;}
		public Vector2[] TextureCoordinates {get;set;}
		public RenderMode RenderData {get;set;}
	}
		
	internal class GraphicsDevice2D
	{
		private GraphicsDevice _device;
		private readonly List<SpriteBatchRenderItem> _sprites = new List<SpriteBatchRenderItem>();
		private SpriteBlendMode _actualBlendMode;
		private SpriteSortMode _actualSortMode = SpriteSortMode.Deferred;
    
		public GraphicsDevice2D (GraphicsDevice Device)
		{
			_device = Device;
		}

        public void SizeChanged()
        {
            // Set up OpenGL projection matrix
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, _device.DisplayMode.Width, _device.DisplayMode.Height, 0, -1, 1);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.Viewport(0, 0, _device.DisplayMode.Width, _device.DisplayMode.Height);
        }

        public void ApplyScale()
        {
            GL.Scale(ScaleW, ScaleH, 1);
        }

        public float ScaleH
        {
            get
            {
                //GL.Scale(scaleW, scaleH, 1);
                return _device.DisplayMode.Height / _device.mngr.PreferredBackBufferHeight;
            }
        }

        public float ScaleW
        {
            get
            {
                return _device.DisplayMode.Width/_device.mngr.PreferredBackBufferWidth;
            }
        }

		public void Initialise2DOpenGL()
		{
			// Initialize OpenGL states			
			GL.Disable(EnableCap.DepthTest);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,(int) All.BlendSrc);
			GL.EnableClientState(ArrayCap.VertexArray);
			
			_device.ActiveTexture = -1;
			GL.Disable(EnableCap.Blend);
		}
			
		public void StartSpriteBatch(SpriteBlendMode blendMode, SpriteSortMode sortMode)
		{
			if (_sprites.Count > 0)
			{
				throw new InvalidOperationException("SpriteBatch is in incorrect state");
			}
			
			_actualBlendMode = blendMode;
			_actualSortMode = sortMode;
			
			Initialise2DOpenGL();			
			
			// Set the current blend mode
			switch (_actualBlendMode)
			{
				case SpriteBlendMode.Additive :					
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha,BlendingFactorDest.One);
					break;
				case SpriteBlendMode.AlphaBlend :
					GL.Enable(EnableCap.Blend);
					GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
					break;
				case SpriteBlendMode.PreMultiplied :
					GL.Enable(EnableCap.Blend); 
					GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
					break;
				case SpriteBlendMode.None :
					break;
			}
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
		
		private void DrawSprites(List<SpriteBatchRenderItem> spritesToDraw)
		{
			// Draw the current spritebatch
			if (spritesToDraw.Count > 0)
			{									
				List<float> Vertices = new List<float>();
				List<float> TextureCoords = new List<float>();
				TextureCoords.Capacity = spritesToDraw.Count*4;
				Vertices.Capacity = spritesToDraw.Count*4;
				
				RenderMode actualMode = spritesToDraw[0].RenderData;
				
				// Draw sprite vertices
				for (int sbi = 0; sbi<spritesToDraw.Count; sbi++)
				{
					SpriteBatchRenderItem sb = spritesToDraw[sbi];
					
					if ((!actualMode.IsCompatible(sb.RenderData)) || (actualMode.Texture.Image.Name != sb.RenderData.Texture.Image.Name))
					{
						// Render sprites
						_device.RenderSprites(Vector2.Zero,TextureCoords.ToArray(),Vertices.ToArray(),actualMode);
						Vertices.Clear();
						TextureCoords.Clear();
						actualMode = sb.RenderData;
					}
							
					// put the vertices in vertex array
					AddToFloatList(sb.Vertices,Vertices);
					AddToFloatList(sb.TextureCoordinates,TextureCoords);
				}
				
				// Render the sprites
				_device.RenderSprites(Vector2.Zero,TextureCoords.ToArray(),Vertices.ToArray(),spritesToDraw[spritesToDraw.Count-1].RenderData);
			}						
		}
		
		public void AddToFloatList(Vector2[] vectors, List<float> list)
		{
			for (int i=0;i<vectors.Length;i++)
			{
				list.Add(vectors[i].X);
				list.Add(vectors[i].Y);
			}
		}
		
		public void AddToSpriteBuffer(SpriteBatchRenderItem sbItem)
		{
			_sprites.Add(sbItem);
		}
		
		private void SortSprites()
        {
			if ( _actualSortMode != SpriteSortMode.Immediate )
			{
				IComparer<SpriteBatchRenderItem> comparer;
				
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
					default :
						throw new NotImplementedException();
	            }
				_sprites.Sort(comparer);
			}
        }
	}
}
