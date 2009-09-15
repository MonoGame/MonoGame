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
	
using XnaTouch.Framework;
using System;
using MonoTouch.OpenGLES;
using OpenTK.Graphics.ES11;
using MonoTouch.CoreAnimation;

namespace XnaTouch.Framework.Graphics
{
    public class GraphicsDevice : IDisposable
    {
		private EAGLContext _context;
		private uint ViewRenderBuffer, ViewFrameBuffer;
		private int BackingWidth;
		private int BackingHeight;
		private CAEAGLLayer _eaglLayer;
		private All _preferedFilter;
		private static int _activeTexture = -1;
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
		
		internal static int ActiveTexture
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
		
		public GraphicsDevice(CAEAGLLayer layer)
        {
			_eaglLayer = layer;
			_context = new EAGLContext (EAGLRenderingAPI.OpenGLES1);
			if (!EAGLContext.SetCurrentContext (_context)) 
			{
				throw new Exception ("Unable to set EAGLContext!");
			}
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
        }

        public void Present()
        {
			_context.PresentRenderBuffer ((uint) All.RenderbufferOes);
        }


        public void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
  			throw new NotImplementedException();
		}
		
		internal void InitializeOpenGL(int backBufferWidth, int backBufferHeight)
		{
			//Set the frame buffer size
			BackingWidth = backBufferWidth;
			BackingHeight = backBufferHeight;
			
			// Set up OpenGL projection matrix
			GL.MatrixMode(All.Projection);
			GL.LoadIdentity();
			GL.Ortho(0, BackingWidth, 0, BackingHeight, -1, 1);
			GL.MatrixMode(All.Modelview);
			GL.Viewport(0,0,BackingWidth,BackingHeight);
					
			// Initialize OpenGL states			
			GL.Disable(All.DepthTest);
			GL.TexEnv(All.TextureEnv, All.TextureEnvMode,(int) All.BlendSrc);
			GL.EnableClientState(All.VertexArray);
		}
		
		internal void StartPresentation()
		{						
			EAGLContext.SetCurrentContext (_context);
			GL.Oes.BindFramebuffer (All.FramebufferOes, ViewFrameBuffer);
			GL.Viewport (0, 0, BackingWidth, BackingHeight);						
		}

        public void Reset()
        {
			DestroyFrameBuffer ();
			CreateFrameBuffer ();	
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
                return new DisplayMode();
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
                throw new NotImplementedException();
            }
            set
            {
            }
        }
		
		private bool CreateFrameBuffer ()
		{
			GL.Oes.GenFramebuffers (1, ref ViewFrameBuffer);
			GL.Oes.GenRenderbuffers (1, ref ViewRenderBuffer);
	
			GL.Oes.BindFramebuffer (All.FramebufferOes, ViewFrameBuffer);
			GL.Oes.BindRenderbuffer (All.RenderbufferOes, ViewRenderBuffer);
			_context.RenderBufferStorage ((uint) All.RenderbufferOes, _eaglLayer);
			GL.Oes.FramebufferRenderbuffer (All.FramebufferOes,
				All.ColorAttachment0Oes,
				All.RenderbufferOes,
				ViewRenderBuffer);
	
			GL.Oes.GetRenderbufferParameter (All.RenderbufferOes, All.RenderbufferWidthOes, ref BackingWidth);
			GL.Oes.GetRenderbufferParameter (All.RenderbufferOes, All.RenderbufferHeightOes, ref BackingHeight);
			
			if (GL.Oes.CheckFramebufferStatus (All.FramebufferOes) != All.FramebufferCompleteOes) {
				Console.Error.WriteLine("failed to make complete framebuffer object {0}",
					GL.Oes.CheckFramebufferStatus (All.FramebufferOes));
			}
			
			return true;
		}
		
		private void DestroyFrameBuffer ()
		{
			if (ViewFrameBuffer != 0) 
			{
				GL.Oes.DeleteFramebuffers (1, ref ViewFrameBuffer);
				ViewFrameBuffer = 0;
				GL.Oes.DeleteRenderbuffers (1, ref ViewRenderBuffer);
				ViewRenderBuffer = 0;
			}
		}
		
		internal static void RenderAtPoint(ESImage image, Vector2 point)
		{
			// Use the textureOffset defined for X and Y along with the texture width and height to render the texture
			Vector2 texOffsetPoint = new Vector2(image.TextureOffsetX, image.TextureOffsetY);
			RenderSubImageAtPoint(image, point,texOffsetPoint,image.ImageWidth,image.ImageHeight);
		}
		
		internal static void RenderAt(ESImage image, Vector2 point, float[] texCoords, float[] quadVertices)
		{
			// Save the current matrix to the stack
			GL.PushMatrix();
	
			// Move to where we want to draw the image
			GL.Translate(point.X, point.Y+image.Origin.Y*2, 0.0f);
	
			// Rotate around the Z axis by the angle define for this image
			// we cannot use radians
			GL.Rotate(MathHelper.ToDegrees(-image.Rotation), 0.0f, 0.0f, 1.0f);
	
			// Set the glColor to apply alpha to the image
			GL.Color4(image.FilterColor.X, image.FilterColor.Y, image.FilterColor.Z, image.FilterColor.W);
	
			// Set client states so that the Texture Coordinate Array will be used during rendering
			GL.EnableClientState(All.TextureCoordArray);
	
			// Enable Texture_2D
			GL.Enable(All.Texture2D);
			
			// Bind to the texture that is associated with this image
			if (_activeTexture != image.Name) 
			{
				GL.BindTexture(All.Texture2D, image.Name);
				_activeTexture = (int) image.Name;
			}
			
			// Set up the VertexPointer to point to the vertices we have defined
			GL.VertexPointer(2, All.Float, 0, quadVertices);
			
			// Set up the TexCoordPointer to point to the texture coordinates we want to use
			GL.TexCoordPointer(2, All.Float, 0, texCoords);
			
			// Draw the vertices to the screen
			GL.DrawArrays(All.TriangleStrip, 0, 4);
			
			// Disable as necessary
			GL.Disable(All.Texture2D);
			GL.DisableClientState(All.TextureCoordArray);
			
			// Restore the saved matrix from the stack
			GL.PopMatrix();
		}
		
		internal static void RenderSubImageAtPoint(ESImage image, Vector2 point, Vector2 offsetPoint, float subImageWidth, float subImageHeight)
		{
	
			// Calculate the texture coordinates using the offset point from which to start the image and then using the width and height
			// passed in
			float[]	textureCoordinates = new float[8];
			image.GetTextureCoordinates(textureCoordinates,0,new Rectangle((int)offsetPoint.X,(int)offsetPoint.Y,(int)subImageWidth,(int)subImageHeight));		
	
			// Calculate the width and the height of the quad using the current image scale and the width and height
			// of the image we are going to render
			float quadWidth = subImageWidth * image.HorizontalScale;
			float quadHeight = subImageHeight * image.VerticalScale;
	
			// Define the vertices for each corner of the quad which is going to contain our image.
			// We calculate the size of the quad to match the size of the subimage which has been defined.
			// If center is true, then make sure the point provided is in the center of the image else it will be
			// the bottom left hand corner of the image
			float[] quadVertices = new float[8];
			image.GetTextureVertices(quadVertices,0,subImageWidth,subImageHeight);
	
			// Now that we have defined the texture coordinates and the quad vertices we can render to the screen 
			// using them
			RenderAt(image, point, textureCoordinates,quadVertices);
		}		
	}
}

