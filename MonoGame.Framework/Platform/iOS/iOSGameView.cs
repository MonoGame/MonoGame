// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Drawing;

using CoreAnimation;
using Foundation;
using ObjCRuntime;
using OpenGLES;
using UIKit;
using CoreGraphics;

using MonoGame.OpenGL;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework {

    [Register("iOSGameView")]
	partial class iOSGameView : UIView {
		private readonly iOSGamePlatform _platform;
		private int _colorbuffer;
		private int _depthbuffer;
		private int _framebuffer;

		#region Construction/Destruction
		public iOSGameView (iOSGamePlatform platform, CGRect frame)
			: base(frame)
		{
			if (platform == null)
				throw new ArgumentNullException ("platform");
			_platform = platform;
			Initialize ();
		}

		private void Initialize ()
		{
            #if !TVOS
			MultipleTouchEnabled = true;
            #endif
			Opaque = true;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (__renderbuffergraphicsContext != null)
					DestroyContext();
			}

			base.Dispose (disposing);
			_isDisposed = true;
		}

		#endregion Construction/Destruction

		#region Properties

		private bool _isDisposed;

		public bool IsDisposed {
			get { return _isDisposed; }
		}

		#endregion Properties

		[Export ("layerClass")]
		public static Class GetLayerClass ()
		{
			return new Class (typeof (CAEAGLLayer));
		}

		public override bool CanBecomeFirstResponder {
			get { return true; }
		}

		private new CAEAGLLayer Layer {
			get { return base.Layer as CAEAGLLayer; }
		}

		// FIXME: Someday, hopefully it will be possible to move
		//        GraphicsContext into an iOS-specific GraphicsDevice.
		//        Some level of cooperation with the UIView/Layer will
		//        probably always be necessary, unfortunately.
		private IGraphicsContext __renderbuffergraphicsContext;
		private IOpenGLApi _glapi;
		private void CreateContext ()
		{
			AssertNotDisposed ();

            // RetainedBacking controls if the content of the colorbuffer should be preserved after being displayed
            // This is the XNA equivalent to set PreserveContent when initializing the GraphicsDevice
            // (should be false by default for better performance)
			Layer.DrawableProperties = NSDictionary.FromObjectsAndKeys (
				new NSObject [] {
					NSNumber.FromBoolean (false), 
					EAGLColorFormat.RGBA8
				},
				new NSObject [] {
					EAGLDrawableProperty.RetainedBacking,
					EAGLDrawableProperty.ColorFormat
				});

			Layer.ContentsScale = Window.Screen.Scale;

			//var strVersion = OpenTK.Graphics.ES11.GL.GetString (OpenTK.Graphics.ES11.All.Version);
			//strVersion = OpenTK.Graphics.ES20.GL.GetString (OpenTK.Graphics.ES20.All.Version);
			//var version = Version.Parse (strVersion);

			try {
                __renderbuffergraphicsContext = GL.CreateContext (null);
                //new GraphicsContext (null, null, 2, 0, GraphicsContextFlags.Embedded)
            } catch (Exception ex) {
                throw new Exception ("Device not Supported. GLES 2.0 or above is required!");
			}

			this.MakeCurrent();
            _glapi = new Gles20Api();
		}

		private void DestroyContext ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

			__renderbuffergraphicsContext.Dispose ();
			__renderbuffergraphicsContext = null;
			_glapi = null;
		}

        [Export("doTick")]
        void DoTick()
        {
            _platform.Tick();
        }

		private void CreateFramebuffer ()
		{
			this.MakeCurrent();
			
			// HACK:  GraphicsDevice itself should be calling
			//        glViewport, so we shouldn't need to do it
			//        here and then force the state into
			//        GraphicsDevice.  However, that change is a
			//        ways off, yet.
            int viewportHeight = (int)Math.Round(Layer.Bounds.Size.Height * Layer.ContentsScale);
            int viewportWidth = (int)Math.Round(Layer.Bounds.Size.Width * Layer.ContentsScale);

			_glapi.GenFramebuffers (1, ref _framebuffer);
            _glapi.BindFramebuffer (FramebufferTarget.Framebuffer, _framebuffer);

			// Create our Depth buffer. Color buffer must be the last one bound
            var gdm = _platform.Game.Services.GetService(
                typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager;
            if (gdm != null)
            {
                var preferredDepthFormat = gdm.PreferredDepthStencilFormat;
                if (preferredDepthFormat != DepthFormat.None)
                {
                    GL.GenRenderbuffers(1, out _depthbuffer);
                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _depthbuffer);
                    var internalFormat = RenderbufferStorage.DepthComponent16;
                    if (preferredDepthFormat == DepthFormat.Depth24)
                        internalFormat = RenderbufferStorage.DepthComponent24Oes;
                    else if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                        internalFormat = RenderbufferStorage.Depth24Stencil8Oes;
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalFormat, viewportWidth, viewportHeight);
                    GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _depthbuffer);
                    if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, _depthbuffer);
                }
            }

			_glapi.GenRenderbuffers(1, ref _colorbuffer);
            _glapi.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _colorbuffer);

			var ctx = __renderbuffergraphicsContext as GraphicsContext;

			// TODO: EAGLContext.RenderBufferStorage returns false
			//       on all but the first call.  Nevertheless, it
			//       works.  Still, it would be nice to know why it
			//       claims to have failed.
            ctx.Context.RenderBufferStorage ((uint) RenderbufferTarget.Renderbuffer, Layer);
			
            _glapi.FramebufferRenderbuffer (FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, RenderbufferTarget.Renderbuffer, _colorbuffer);
			
			var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
			if (status != FramebufferErrorCode.FramebufferComplete)
				throw new InvalidOperationException (
					"Framebuffer was not created correctly: " + status);

			_glapi.Viewport(0, 0, viewportWidth, viewportHeight);
            _glapi.Scissor(0, 0, viewportWidth, viewportHeight);

			var gds = _platform.Game.Services.GetService(
                typeof (IGraphicsDeviceService)) as IGraphicsDeviceService;

			if (gds != null && gds.GraphicsDevice != null)
			{
                var pp = gds.GraphicsDevice.PresentationParameters;
                int height = viewportHeight;
                int width = viewportWidth;

                if (this.NextResponder is iOSGameViewController)
                {
                    var displayOrientation = _platform.Game.Window.CurrentOrientation;
                    if (displayOrientation == DisplayOrientation.LandscapeLeft || displayOrientation == DisplayOrientation.LandscapeRight)
                    {
                        height = Math.Min(viewportHeight, viewportWidth);
                        width = Math.Max(viewportHeight, viewportWidth);
                    }
                    else
                    {
                        height = Math.Max(viewportHeight, viewportWidth);
                        width = Math.Min(viewportHeight, viewportWidth);
                    }
                }

                pp.BackBufferHeight = height;
                pp.BackBufferWidth = width;

				gds.GraphicsDevice.Viewport = new Viewport(
					0, 0,
					pp.BackBufferWidth,
					pp.BackBufferHeight);
				
				// FIXME: These static methods on GraphicsDevice need
				//        to go away someday.
				gds.GraphicsDevice.glFramebuffer = _framebuffer;
			}

            if (Threading.BackgroundContext == null)
                Threading.BackgroundContext = new OpenGLES.EAGLContext(ctx.Context.API, ctx.Context.ShareGroup);
		}

		private void DestroyFramebuffer ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

			__renderbuffergraphicsContext.MakeCurrent (null);

			_glapi.DeleteFramebuffers (1, ref _framebuffer);
			_framebuffer = 0;

			_glapi.DeleteRenderbuffers (1, ref _colorbuffer);
			_colorbuffer = 0;
			
            if (_depthbuffer != 0)
            {
			    _glapi.DeleteRenderbuffers (1, ref _depthbuffer);
			    _depthbuffer = 0;
            }
		}

        private static readonly FramebufferAttachment[] attachements = new FramebufferAttachment[] { FramebufferAttachment.DepthAttachment, FramebufferAttachment.StencilAttachment };

		// FIXME: This logic belongs in GraphicsDevice.Present, not
		//        here.  If it can someday be moved there, then the
		//        normal call to Present in Game.Tick should cover
		//        this.  For now, iOSGamePlatform will call Present
		//        in the Draw/Update loop handler.
		public void Present ()
		{
            AssertNotDisposed ();
            AssertValidContext ();

            this.MakeCurrent();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, this._colorbuffer);
            GL.InvalidateFramebuffer(FramebufferTarget.Framebuffer, 2, attachements);
            __renderbuffergraphicsContext.SwapBuffers();
		}

		// FIXME: This functionality belongs in GraphicsDevice.
		public void MakeCurrent ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

            if (!__renderbuffergraphicsContext.IsCurrent)
            {
			    __renderbuffergraphicsContext.MakeCurrent (null);
            }
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

            var gds = _platform.Game.Services.GetService (
                typeof (IGraphicsDeviceService)) as IGraphicsDeviceService;

            if (gds == null || gds.GraphicsDevice == null)
                return;

			if (_framebuffer != 0)
				DestroyFramebuffer ();
			if (__renderbuffergraphicsContext == null)
				CreateContext();
			CreateFramebuffer ();
		}

		#region UIWindow Notifications

		[Export ("didMoveToWindow")]
		public virtual void DidMoveToWindow ()
		{

            if (Window != null) {
                
                if (__renderbuffergraphicsContext == null)
                    CreateContext ();
                if (_framebuffer == 0)
                    CreateFramebuffer ();
            }
		}

		#endregion UIWindow Notifications

		private void AssertNotDisposed ()
		{
			if (_isDisposed)
				throw new ObjectDisposedException (GetType ().Name);
		}

		private void AssertValidContext ()
		{
			if (__renderbuffergraphicsContext == null)
				throw new InvalidOperationException (
					"GraphicsContext must be created for this operation to succeed.");
		}
	}
}
