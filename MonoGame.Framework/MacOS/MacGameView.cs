//
// http://www.cocoa-mono.org
//
// Copyright (c) 2011 Kenneth J. Pouncey
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
using System;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;

using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.CoreVideo;
using MonoMac.CoreGraphics;
using MonoMac.OpenGL;

namespace Microsoft.Xna.Framework
{
	public class MacGameView : MonoMac.AppKit.NSView, IGameWindow
	{
		bool disposed;
		NSOpenGLContext openGLContext;
		NSOpenGLPixelFormat pixelFormat;
		CVDisplayLink displayLink;
		NSObject notificationProxy;
		NSTimer animationTimer;
		bool animating;
		bool displayLinkSupported = false;
		WindowState windowState = WindowState.Normal;
		DateTime prevUpdateTime;
		DateTime prevRenderTime;
		FrameEventArgs updateEventArgs = new FrameEventArgs ();
		FrameEventArgs renderEventArgs = new FrameEventArgs ();

		[Export("initWithFrame:")]
		public MacGameView (RectangleF frame) : this(frame, null)
		{
		}

		public MacGameView (RectangleF frame, NSOpenGLContext context) : base(frame)
		{
			var attribs = new object [] {
				NSOpenGLPixelFormatAttribute.Accelerated,
				NSOpenGLPixelFormatAttribute.NoRecovery,
				NSOpenGLPixelFormatAttribute.DoubleBuffer,
				NSOpenGLPixelFormatAttribute.ColorSize, 24,
				NSOpenGLPixelFormatAttribute.DepthSize, 24 };

			pixelFormat = new NSOpenGLPixelFormat (attribs);

			if (pixelFormat == null)
				Console.WriteLine ("No OpenGL pixel format");

			// NSOpenGLView does not handle context sharing, so we draw to a custom NSView instead
			openGLContext = new NSOpenGLContext (pixelFormat, context);

			openGLContext.MakeCurrentContext ();

			// default the swap interval and displaylink
			SwapInterval = true;
			DisplaylinkSupported = true;

			// Look for changes in view size
			// Note, -reshape will not be called automatically on size changes because NSView does not export it to override 
			notificationProxy = NSNotificationCenter.DefaultCenter.AddObserver (NSView.GlobalFrameChangedNotification, HandleReshape);
		}

		[Preserve (Conditional=true)]
		public override void DrawRect (RectangleF dirtyRect)
		{
			if (animating) {
				if (displayLinkSupported) {
					// Render Scene only if the display link is running
					if (displayLink.IsRunning)
						RenderScene ();
				} else {
					RenderScene ();
				}
			}
		}

		[Preserve (Conditional=true)]
		public override void LockFocus ()
		{
			base.LockFocus ();
			if (openGLContext.View != this)
				openGLContext.View = this;
		}

		public NSOpenGLContext OpenGLContext {
			get { return openGLContext; }
		}

		public NSOpenGLPixelFormat PixelFormat {
			get { return pixelFormat; }
		}

		NSViewController GetViewController ()
		{
			NSResponder r = this;
			while (r != null) {
				var c = r as NSViewController;
				if (c != null)
					return c;
				r = r.NextResponder;
			}
			return null;
		}

		protected void UpdateView ()
		{
			Size = new Size ((int)Bounds.Width, (int)Bounds.Height);
		}

		private void HandleReshape (NSNotification note)
		{
			UpdateView ();
		}

		private void StartAnimation (double updatesPerSecond)
		{
			if (!animating) {
				if (displayLinkSupported) {
					if (displayLink != null && !displayLink.IsRunning)
						displayLink.Start ();
				} else {
					// Can't use TimeSpan.FromSeconds() as that only has 1ms
					// resolution, and we need better (e.g. 60fps doesn't fit nicely
					// in 1ms resolution, but does in ticks).
					var timeout = new TimeSpan ((long)(((1.0 * TimeSpan.TicksPerSecond) / updatesPerSecond) + 0.5));

					if (SwapInterval) {
						animationTimer = NSTimer.CreateRepeatingScheduledTimer (timeout, 
							delegate {
								NeedsDisplay = true;
							});
					} else {
						animationTimer = NSTimer.CreateRepeatingScheduledTimer (timeout, 
							delegate {
								RenderScene ();
							});
					}
					
					NSRunLoop.Current.AddTimer (animationTimer, NSRunLoopMode.Default);
					NSRunLoop.Current.AddTimer (animationTimer, NSRunLoopMode.EventTracking);

				}
			}

			animating = true;
		}

		public void Stop ()
		{
			if (animating) {
				if (displayLinkSupported) {
					if (displayLink != null && displayLink.IsRunning)
						displayLink.Stop ();

				} else {
					animationTimer.Invalidate ();
					animationTimer = null;
				}
			}
			animating = false;
		}

		// Clean up the notifications
		private void DeAllocate ()
		{
			Stop ();
			displayLink = null;
			NSNotificationCenter.DefaultCenter.RemoveObserver (notificationProxy); 
		}

		void AssertValid ()
		{
			if (disposed)
				throw new ObjectDisposedException ("");
		}

		void AssertContext ()
		{
			if (OpenGLContext == null)
				throw new InvalidOperationException ("Operation requires an OpenGLContext, which hasn't been created yet.");
		}

		public virtual string Title {
			get {
				AssertValid ();
				if (Window != null)
					return Window.Title;
				else
					throw new NotSupportedException();
			}
			set {
				AssertValid ();
				if (Window != null)
					Window.Title = value;
				else
					throw new NotSupportedException();
			}
		}		

		protected virtual void OnTitleChanged (EventArgs e)
		{
			var h = TitleChanged;
			if (h != null)
				h (this, EventArgs.Empty);
		}

		bool INativeWindow.Focused {
			get { throw new NotImplementedException ();}
		}

		public virtual bool Visible {
			get {
				AssertValid ();
				return !base.Hidden;
			}
			set {
				AssertValid ();
				if (base.Hidden != !value) {
					base.Hidden = !value;
					OnVisibleChanged (EventArgs.Empty);
				}
			}
		}

		protected virtual void OnVisibleChanged (EventArgs e)
		{
			var h = VisibleChanged;
			if (h != null)
				h (this, EventArgs.Empty);
		}

		bool INativeWindow.Exists {
			get { throw new NotImplementedException ();}
		}

		public virtual WindowState WindowState {
			get {
				AssertValid ();
				return windowState;
			}
			set {
				AssertValid ();
				if (windowState != value) {
					windowState = value;
					OnWindowStateChanged (EventArgs.Empty);
				}
			}
		}

		protected virtual void OnWindowStateChanged (EventArgs e)
		{
			var h = WindowStateChanged;
			if (h != null)
				h (this, EventArgs.Empty);
		}

		public virtual WindowBorder WindowBorder {
			get {
				AssertValid ();
				return WindowBorder.Hidden;
			}
			set {}
		}

		System.Drawing.Rectangle INativeWindow.Bounds {
			get { throw new NotSupportedException ();}
			set { throw new NotSupportedException ();}
		}

		System.Drawing.Point INativeWindow.Location {
			get { throw new NotSupportedException ();}
			set { throw new NotSupportedException ();}
		}

		System.Drawing.Icon INativeWindow.Icon {  
			get { throw new NotSupportedException ();}
			set { throw new NotSupportedException ();}		
		}

		Size size;

		public Size Size {
			get {
				AssertValid ();
				return size;
			}
			set {
				AssertValid ();
				if (size != value) {
					size = value;
					// This method will be called on the main thread when resizing, but we may be drawing on a secondary thread through the display link
					// Add a mutex around to avoid the threads accessing the context simultaneously
					openGLContext.CGLContext.Lock ();

					// Delegate to the scene object to update for a change in the view size
					OnResize (EventArgs.Empty);
					openGLContext.Update ();

					openGLContext.CGLContext.Unlock ();					
				}
			}
		}

		protected virtual void OnResize (EventArgs e)
		{
			var h = Resize;
			if (h != null)
				h (this, e);
		}

		int INativeWindow.X {
			get { throw new NotSupportedException ();}
			set { throw new NotSupportedException ();}
		}

		int INativeWindow.Y {
			get { throw new NotSupportedException ();}
			set { throw new NotSupportedException ();}
		}

		int INativeWindow.Width {
			get { throw new NotSupportedException ();}
			set { throw new NotSupportedException ();}
		}

		int INativeWindow.Height {
			get { throw new NotSupportedException ();}
			set { throw new NotSupportedException ();}
		}

		System.Drawing.Rectangle INativeWindow.ClientRectangle {
			get { throw new NotSupportedException ();}
			set { throw new NotSupportedException ();}
		}

		Size INativeWindow.ClientSize {
			get { throw new NotSupportedException ();}
			set { throw new NotSupportedException ();}
		}

		public virtual void Close ()
		{
			AssertValid ();
			OnClosed (EventArgs.Empty);
		}

		protected virtual void OnClosed (EventArgs e)
		{
			var h = Closed;
			if (h != null)
				h (this, e);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposed)
				return;
			if (disposing) {
				DeAllocate ();
				//DestroyFrameBuffer ();
			}
			base.Dispose (disposing);
			disposed = true;
			if (disposing)
				OnDisposed (EventArgs.Empty);
		}

		protected virtual void OnDisposed (EventArgs e)
		{
			var h = Disposed;
			if (h != null)
				h (this, e);
		}

		void INativeWindow.ProcessEvents ()
		{
			throw new NotSupportedException ();
		}

		System.Drawing.Point INativeWindow.PointToClient (System.Drawing.Point point)
		{
			return point;
		}

		System.Drawing.Point INativeWindow.PointToScreen (System.Drawing.Point point)
		{
			return point;
		}

		public virtual void MakeCurrent ()
		{
			AssertValid ();
			AssertContext ();
			OpenGLContext.MakeCurrentContext ();
		}

		public virtual void SwapBuffers ()
		{
			AssertValid ();
			AssertContext ();
			// basically SwapBuffers is the same as FlushBuffer on OSX
			OpenGLContext.FlushBuffer ();
		}

		private bool SwapInterval { get; set; }

		private bool DisplaylinkSupported {
			get { return displayLinkSupported; }	
			set { 
				displayLinkSupported = value;	
			}
		}

		public void Run ()
		{
			AssertValid ();
			OnLoad (EventArgs.Empty);

			// Synchronize buffer swaps with vertical refresh rate
			openGLContext.SwapInterval = SwapInterval;

			if (displayLinkSupported) 
				SetupDisplayLink ();

			StartAnimation (0.0);
		}

		public void Run (double updatesPerSecond)
		{
			AssertValid ();
			if (updatesPerSecond == 0.0) {
				Run ();
				return;
			}

			OnLoad (EventArgs.Empty);

			// Here we set these to false for now and let the main logic continue
			// in the future we may open up these properties to the public
			SwapInterval = false;
			DisplaylinkSupported = false;

			// Synchronize buffer swaps with vertical refresh rate
			openGLContext.SwapInterval = SwapInterval;

			if (displayLinkSupported)
				SetupDisplayLink ();

			StartAnimation (updatesPerSecond);
		}

		private void RenderScene ()
		{

			// This method will be called on both the main thread (through DrawRect:) and a secondary thread (through the display link rendering loop)
			// Also, when resizing the view, Reshape is called on the main thread, but we may be drawing on a secondary thread
			// Add a mutex around to avoid the threads accessing the context simultaneously 
			openGLContext.CGLContext.Lock ();

			// Make sure we draw to the right context
			openGLContext.MakeCurrentContext ();

			var curUpdateTime = DateTime.Now;
			if (prevUpdateTime.Ticks == 0) {
				prevUpdateTime = curUpdateTime;
			}
			var t = (curUpdateTime - prevUpdateTime).TotalSeconds;

			// This fixes a potential error
			if (t <= 0) t = Double.Epsilon;

			updateEventArgs.GetType().InvokeMember("Time",
			                                       BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
			                                       Type.DefaultBinder, updateEventArgs, new object[]{ t });

			OnUpdateFrame (updateEventArgs);
			prevUpdateTime = curUpdateTime;

			var curRenderTime = DateTime.Now;
			if (prevRenderTime.Ticks == 0) {
				prevRenderTime = curRenderTime;
			}
			t = (curRenderTime - prevRenderTime).TotalSeconds;

			// This fixes a potential error
			if (t <= 0) t = Double.Epsilon;
			
			updateEventArgs.GetType().InvokeMember("Time",
			                                       BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
			                                       Type.DefaultBinder, updateEventArgs, new object[]{ t });

			OnRenderFrame (renderEventArgs);
			prevRenderTime = curRenderTime;

			openGLContext.FlushBuffer ();

			openGLContext.CGLContext.Unlock ();
		}

		private void SetupDisplayLink ()
		{
			if (displayLink != null)
				return;

			// Create a display link capable of being used with all active displays
			displayLink = new CVDisplayLink ();

			// Set the renderer output callback function
			displayLink.SetOutputCallback (MyDisplayLinkOutputCallback);

			// Set the display link for the current renderer
			CGLContext cglContext = openGLContext.CGLContext;
			CGLPixelFormat cglPixelFormat = PixelFormat.CGLPixelFormat;
			displayLink.SetCurrentDisplay (cglContext, cglPixelFormat);

		}

		// Private Callback function for CVDisplayLink
		private CVReturn MyDisplayLinkOutputCallback (CVDisplayLink displayLink, ref CVTimeStamp inNow, ref CVTimeStamp inOutputTime, CVOptionFlags flagsIn, ref CVOptionFlags flagsOut)
		{
			//CVReturn result = GetFrameForTime (inOutputTime);
			CVReturn result = CVReturn.Error;

			// There is no autorelease pool when this method is called because it will be called from a background thread
			// It's important to create one or you will leak objects
			using (NSAutoreleasePool pool = new NSAutoreleasePool ()) {
				// Update the animation
				BeginInvokeOnMainThread (RenderScene);
				result = CVReturn.Success;
			}

			return result;
		}

		protected virtual void OnLoad (EventArgs e)
		{
			var h = Load;
			if (h != null)
				h (this, e);
		}

		protected virtual void OnUnload (EventArgs e)
		{
			var h = Unload;
			Stop ();
			if (h != null)
				h (this, e);
		}

		protected virtual void OnUpdateFrame (FrameEventArgs e)
		{
			var h = UpdateFrame;
			if (h != null)
				h (this, e);
		}

		protected virtual void OnRenderFrame (FrameEventArgs e)
		{
			var h = RenderFrame;
			if (h != null)
				h (this, e);
		}

		event EventHandler<EventArgs> INativeWindow.Move {
			add { throw new NotSupportedException ();}
			remove { throw new NotSupportedException ();}
		}

		public event EventHandler<EventArgs> Resize;

		event EventHandler<CancelEventArgs> INativeWindow.Closing {
			add { throw new NotSupportedException ();}
			remove { throw new NotSupportedException ();}
		}

		public event EventHandler<EventArgs> Closed;
		public event EventHandler<EventArgs> Disposed;
		public event EventHandler<EventArgs> TitleChanged;
		public event EventHandler<EventArgs> VisibleChanged;

		event EventHandler<EventArgs> INativeWindow.FocusedChanged {
			add { throw new NotSupportedException ();}
			remove { throw new NotSupportedException ();}
		}

		event EventHandler<EventArgs> INativeWindow.WindowBorderChanged {
			add { throw new NotSupportedException ();}
			remove { throw new NotSupportedException ();}
		}

		public event EventHandler<EventArgs> WindowStateChanged;

		//		event EventHandler<KeyPressEventArgs> INativeWindow.KeyPress {
		//			add { throw new NotSupportedException ();}
		//			remove { throw new NotSupportedException ();}
		//		}
		event EventHandler<EventArgs> INativeWindow.IconChanged {
			add { throw new NotSupportedException ();}
			remove { throw new NotSupportedException ();}
		}

		public event EventHandler<EventArgs> Load;
		public event EventHandler<EventArgs> Unload;
		public event EventHandler<FrameEventArgs> UpdateFrame;
		public event EventHandler<FrameEventArgs> RenderFrame;
	}
}

