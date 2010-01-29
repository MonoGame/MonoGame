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
	
using MonoTouch.CoreAnimation;
using MonoTouch.OpenGLES;
using OpenTK.Graphics.ES11;
using System;
using XnaTouch.Framework;

namespace XnaTouch.Framework.Graphics
{	
    public class GraphicsDevice : IDisposable
    {
		private All _preferedFilter;
		private int _activeTexture = -1;
		private Viewport _viewport;
		private GraphicsDevice2D _spriteDevice;
		private bool _isDisposed = false;
		private readonly DisplayMode _displayMode = new DisplayMode();
		
		internal All PreferedFilter 
		{
			get 
			{
				return _preferedFilter;
			}
			set 
			{
				_preferedFilter = value;
			}
		
		}
		
		internal int ActiveTexture
		{
			get 
			{
				return _activeTexture;
			}
			set 
			{
				_activeTexture = value;
			}
		}
		
		public bool IsDisposed 
		{ 
			get
			{
				return _isDisposed;
			}
		}
				
		public GraphicsDevice()
        {	
			// Initialize the main viewport
			_viewport = new Viewport();
			_viewport.X = 0;
			_viewport.Y = 0;						
			_viewport.Width = DisplayMode.Width;
			_viewport.Height = DisplayMode.Height;	
			_viewport.TitleSafeArea = new Rectangle(0,0,DisplayMode.Width,DisplayMode.Height);	
			
			// Create the Sprite Rendering engine
			_spriteDevice = new GraphicsDevice2D(this);
        }

        public void Clear(Color color)
        {
			Vector4 vector = color.ToEAGLColor();			
			GL.ClearColor (vector.X,vector.Y,vector.Z,1.0f);
			GL.Clear ((uint) All.ColorBufferBit);
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
			throw new NotImplementedException();
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
			throw new NotImplementedException();
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil, Rectangle[] regions)
        {
			throw new NotImplementedException();
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil, Rectangle[] regions)
        {
			throw new NotImplementedException();
        }

		public void Dispose()
		{
			_isDisposed = true;
		}
		
		protected virtual void Dispose(bool aReleaseEverything)
		{
			if (aReleaseEverything)
			{
				
			}
			
			_isDisposed = true;
		}
		
        public void Present()
        {
        }
		
        public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
  			throw new NotImplementedException();
		}
				
        public void Reset()
        {
			throw new NotImplementedException();
        }

        public void Reset(XnaTouch.Framework.Graphics.PresentationParameters presentationParameters)
        {
			throw new NotImplementedException();
        }

        public void Reset(XnaTouch.Framework.Graphics.PresentationParameters presentationParameters, GraphicsAdapter graphicsAdapter)
        {
			throw new NotImplementedException();
        }

        public XnaTouch.Framework.Graphics.DisplayMode DisplayMode
        {
            get
            {
                return _displayMode;
            }
        }

        public XnaTouch.Framework.Graphics.GraphicsDeviceCapabilities GraphicsDeviceCapabilities
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public XnaTouch.Framework.Graphics.GraphicsDeviceStatus GraphicsDeviceStatus
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public XnaTouch.Framework.Graphics.PresentationParameters PresentationParameters
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public XnaTouch.Framework.Graphics.Viewport Viewport
        {
            get
            {
				return _viewport;
			}
			set
			{
				_viewport = value;
			}
		}		
				
		internal void StartSpriteBatch(SpriteBlendMode blendMode, SpriteSortMode sortMode)
		{
			_spriteDevice.StartSpriteBatch(blendMode,sortMode);
		}
		
		internal void EndSpriteBatch()
		{
			_spriteDevice.EndSpriteBatch();
		}
		
		internal void AddToSpriteBuffer(SpriteBatchRenderItem sbItem)
		{
			_spriteDevice.AddToSpriteBuffer(sbItem);				
		}
		
		internal void RenderSprites(Vector2 point, float[] texCoords, float[] quadVertices, RenderMode renderMode)
		{
			if (texCoords.Length == 0) return;
			
			int itemCount = texCoords.Length / 8;
		
			// Enable Texture_2D
			GL.Enable(All.Texture2D);
			
			// Set the glColor to apply alpha to the image
			Vector4 color = renderMode.FilterColor.ToEAGLColor();			
			GL.Color4(color.X, color.Y, color.Z, color.W);
	
			// Set client states so that the Texture Coordinate Array will be used during rendering
			GL.EnableClientState(All.TextureCoordArray);
							
			// Bind to the texture that is associated with this image
			if (ActiveTexture != renderMode.Texture.Image.Name) 
			{
				GL.BindTexture(All.Texture2D, renderMode.Texture.Image.Name);
				ActiveTexture = (int) renderMode.Texture.Image.Name;
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
		
		public void SetRenderTarget (
         int renderTargetIndex,
         RenderTarget2D renderTarget
		                             )
		{
			throw new NotImplementedException();
		}
	}
}

