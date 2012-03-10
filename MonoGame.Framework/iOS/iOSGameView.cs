#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;
using System.Drawing;

using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

using OpenTK.Graphics;
using OpenTK.Platform.iPhoneOS;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

using All = OpenTK.Graphics.ES20.All;

namespace Microsoft.Xna.Framework {
	partial class iOSGameView : UIView {
		private readonly iOSGamePlatform _platform;
		private int _renderbuffer;
		private int _framebuffer;

		#region Construction/Destruction
		public iOSGameView (iOSGamePlatform platform, RectangleF frame)
			: base(frame)
		{
			if (platform == null)
				throw new ArgumentNullException ("platform");
			_platform = platform;
			Initialize ();
			SyncTouchRecognizers ();
		}

		private void Initialize ()
		{
			MultipleTouchEnabled = true;
			Opaque = true;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (_graphicsContext != null)
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
		private GraphicsContext _graphicsContext;
		private IOpenGLApi _glapi;
		private void CreateContext ()
		{
			AssertNotDisposed ();

			Layer.DrawableProperties = NSDictionary.FromObjectsAndKeys (
				new NSObject [] {
					NSNumber.FromBoolean (true),
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

			EAGLRenderingAPI eaglRenderingAPI;
			try {
				_graphicsContext = new GraphicsContext (null, null, 2, 0, GraphicsContextFlags.Embedded);
				eaglRenderingAPI = EAGLRenderingAPI.OpenGLES2;
				_glapi = new Gles20Api ();
			} catch {
				_graphicsContext = new GraphicsContext (null, null, 1, 1, GraphicsContextFlags.Embedded);
				eaglRenderingAPI = EAGLRenderingAPI.OpenGLES1;
				_glapi = new Gles11Api ();
			}

			_graphicsContext.MakeCurrent (null);
			_graphicsContext.LoadAll ();

			// FIXME: These static methods on GraphicsDevice need
			// to go away someday.
			GraphicsDevice.OpenGLESVersion = eaglRenderingAPI;
		}

		private void DestroyContext ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

			_graphicsContext.Dispose ();
			_graphicsContext = null;
			_glapi = null;
		}

		private void CreateFramebuffer ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

			_graphicsContext.MakeCurrent (null);

			int previousRenderbuffer = 0;
			_glapi.GetInteger (All.RenderbufferBinding, ref previousRenderbuffer);

			_glapi.GenRenderbuffers (1, ref _renderbuffer);
			_glapi.BindRenderbuffer (All.Renderbuffer, _renderbuffer);

			var ctx = ((IGraphicsContextInternal) _graphicsContext).Implementation as iPhoneOSGraphicsContext;

			// TODO: EAGLContext.RenderBufferStorage returns false
			//       on all but the first call.  Nevertheless, it
			//       works.  Still, it would be nice to know why it
			//       claims to have failed.
			ctx.EAGLContext.RenderBufferStorage ((uint) All.Renderbuffer, Layer);

			_glapi.GenFramebuffers (1, ref _framebuffer);
			_glapi.BindFramebuffer (All.Framebuffer, _framebuffer);
			_glapi.FramebufferRenderbuffer (
				All.Framebuffer, All.ColorAttachment0, All.Renderbuffer, _renderbuffer);

			// HACK:  GraphicsDevice itself should be calling
			//        glViewport, so we shouldn't need to do it
			//        here and then force the state into
			//        GraphicsDevice.  However, that change is a
			//        ways off, yet.
			int unscaledViewportWidth = (int) Math.Round (Layer.Bounds.Size.Width);
			int unscaledViewportHeight = (int) Math.Round (Layer.Bounds.Size.Height);

			_glapi.Viewport (0, 0, unscaledViewportWidth, unscaledViewportHeight);
			_glapi.Scissor (0, 0, unscaledViewportWidth, unscaledViewportHeight);

			var gds = (IGraphicsDeviceService) _platform.Game.Services.GetService (
				typeof (IGraphicsDeviceService));

			if (gds != null && gds.GraphicsDevice != null)
			{
				gds.GraphicsDevice.Viewport = new Viewport (
					0, 0,
					(int) (Layer.Bounds.Width * Layer.ContentsScale),
					(int) (Layer.Bounds.Height * Layer.ContentsScale));
			}

			var status = _glapi.CheckFramebufferStatus (All.Framebuffer);
			if (status != All.FramebufferComplete)
				throw new InvalidOperationException (
					"Framebuffer was not created correctly: " + status);

			// FIXME: These static methods on GraphicsDevice need
			//        to go away someday.
			GraphicsDevice.FrameBufferScreen = _framebuffer;
		}

		private void DestroyFramebuffer ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

			_graphicsContext.MakeCurrent (null);

			var ctx = ((IGraphicsContextInternal)_graphicsContext).Implementation as iPhoneOSGraphicsContext;
			// FIXME: MonoTouch needs to allow null arguments to
			//        RenderBufferStorage, but it doesn't right now.
			//        So we call it manually.
			//ctx.EAGLContext.RenderBufferStorage((uint)All.Renderbuffer, null);
			var selector = new Selector("renderbufferStorage:fromDrawable:");
			Messaging.bool_objc_msgSend_UInt32_IntPtr(
				ctx.EAGLContext.Handle, selector.Handle, (uint)All.Renderbuffer, IntPtr.Zero);

			_glapi.DeleteFramebuffers (1, ref _framebuffer);
			_framebuffer = 0;

			_glapi.DeleteRenderbuffers (1, ref _renderbuffer);
			_renderbuffer = 0;
		}

		// FIXME: This logic belongs in GraphicsDevice.Present, not
		//        here.  If it can someday be moved there, then the
		//        normal call to Present in Game.Tick should cover
		//        this.  For now, iOSGamePlatform will call Present
		//        in the Draw/Update loop handler.
		[Obsolete("Remove iOSGameView.Present once GraphicsDevice.Present fully expresses this")]
		public void Present ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

			_graphicsContext.MakeCurrent (null);

			var ctx = ((IGraphicsContextInternal) _graphicsContext).Implementation as iPhoneOSGraphicsContext;
			ctx.EAGLContext.PresentRenderBuffer ((uint) All.Renderbuffer);
		}

		// FIXME: This functionality belongs in GraphicsDevice.
		[Obsolete("Move the functionality of iOSGameView.MakeCurrent into GraphicsDevice")]
		public void MakeCurrent ()
		{
			AssertNotDisposed ();
			AssertValidContext ();

			_graphicsContext.MakeCurrent (null);
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();

			if (_framebuffer != 0 || _renderbuffer != 0)
				DestroyFramebuffer ();
			if (_graphicsContext == null)
				CreateContext();
			CreateFramebuffer ();
		}

		#region UIWindow Notifications

		public override void WillMoveToWindow (UIWindow window)
		{
			base.WillMoveToWindow (window);

			if (Window != null)
				TouchPanel.EnabledGesturesChanged -= TouchPanel_EnabledGesturesChanged;

			if (_framebuffer != 0 || _renderbuffer != 0)
				DestroyFramebuffer();
		}

		[Export ("didMoveToWindow")]
		public virtual void DidMoveToWindow ()
		{
			if (Window != null) {
				TouchPanel.EnabledGesturesChanged += TouchPanel_EnabledGesturesChanged;

				if (_graphicsContext == null)
					CreateContext ();
				if (_framebuffer == 0 || _renderbuffer == 0)
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
			if (_graphicsContext == null)
				throw new InvalidOperationException (
					"GraphicsContext must be created for this operation to succeed.");
		}
	}
}
