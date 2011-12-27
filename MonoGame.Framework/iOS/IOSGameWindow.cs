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

#region Using Statements
using System;
using System.Drawing;
using System.Collections.Generic;

using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

using OpenTK.Platform.iPhoneOS;

using OpenTK;
using OpenTK.Platform;
using OpenTK.Graphics;
using OpenTK.Graphics.ES11;
using OpenTK.Graphics.ES20;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Graphics;
#endregion Using Statements

namespace Microsoft.Xna.Framework
{
    public class GameWindow : iPhoneOSGameView
    {
		private readonly Rectangle clientBounds;
		private iOSGamePlatform _platform;
		private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;
		private DateTime _lastDraw;
		private DateTime _nowUpdate;
		private DateTime _nowDraw;
		
		// HACK HACK HACK!
		/// <summary>
		/// This is an amazing hack that is based on the knowledge that Run will call CreateFrameBuffer
		/// and that Stop will call DestroyFrameBuffer. We don't want to do either (we don't want to touch
		/// the OpenGL state while the application is being backgrounded/foregrounded). So when the application
		/// is pausing (from DidEnterBackground) simply don't allow CreateFrameBuffer or DestroyFrameBuffer to run.
		/// Also OnLoad, the Load event, OnUnload and the Unload event are called - assume they pose no problem
		/// (although the same technique could be used if they do).
		/// 
		/// The reason that Run and Stop need to be called at all is to stop the timer that is dispatching
		/// update and draw events (which will touch the OpenGL context and get us killed when backgrounded).
		/// </summary>
		bool isPausing = false;
		
		UITapGestureRecognizer recognizerTap;
		UITapGestureRecognizer recognizerDoubleTap;
		UIPinchGestureRecognizer recognizerPinch; 
		
		// As per http://stackoverflow.com/questions/3319209/setting-direction-for-uiswipegesturerecognizer
		UISwipeGestureRecognizer recognizerLeftRightSwipe;
		UISwipeGestureRecognizer recognizerUpDownSwipe;
		
		UILongPressGestureRecognizer recognizerLongPress;
		UIPanGestureRecognizer recognizerPan;
		UIRotationGestureRecognizer recognizerRotation;
		
		Vector2 translatedTouchPosition;
		
		public EAGLContext MainContext;
	    public EAGLContext BackgroundContext;
	    public EAGLSharegroup ShareGroup; 
				
		#region UIVIew Methods

		internal GameWindow(iOSGamePlatform platform) : base (UIScreen.MainScreen.Bounds)
		{
            if (platform == null)
                throw new ArgumentNullException("platform");
            _platform = platform;

			LayerRetainsBacking = false; 
			LayerColorFormat	= EAGLColorFormat.RGBA8;
			ContentScaleFactor  = UIScreen.MainScreen.Scale;
			
			RectangleF rect = UIScreen.MainScreen.Bounds;
			clientBounds = new Rectangle(0,0,(int) (rect.Width * UIScreen.MainScreen.Scale),(int) (rect.Height * UIScreen.MainScreen.Scale));
			
			// Enable multi-touch
			MultipleTouchEnabled = true;
			
			// Don't Autoresize, we'll do that
			AutoResize = false;
						
			// Initialize GameTime
            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime(); 
			
			// Initialize _lastUpdate and _lastDraw
			_lastUpdate = DateTime.Now;
			_lastDraw = DateTime.Now;
		}	
		
		~GameWindow()
		{
			//
		}
		
		[Export ("layerClass")]
		static Class LayerClass() 
		{
			return iPhoneOSGameView.GetLayerClass ();
		}

        public override bool CanBecomeFirstResponder
        {
            get { return true; }
        }
		
		protected override void ConfigureLayer(CAEAGLLayer eaglLayer) 
		{
			eaglLayer.Opaque = true;

			// Scale OpenGL layer to the scale of the main layer
			// On iPhone 4 this makes the renderbuffer size the same as actual device resolution
			// On iPad with user-selected scale of 2x at startup, this will trigger but has no effect on the renderbuffer
			if(UIScreen.MainScreen.Scale != 1)
				eaglLayer.ContentsScale = UIScreen.MainScreen.Scale;
		}
		
		int renderbufferWidth;
		int renderbufferHeight;
		
		protected override void CreateFrameBuffer()
		{	    
			if(isPausing)
				return; // See note on isPausing
			
			try
			{
				ContextRenderingApi = EAGLRenderingAPI.OpenGLES2;
				base.CreateFrameBuffer();
				
				unsafe
				{
					int width = 0, height = 0;
					OpenTK.Graphics.ES20.GL.GetRenderbufferParameter(OpenTK.Graphics.ES20.All.Renderbuffer, OpenTK.Graphics.ES20.All.RenderbufferWidth, &width);
					OpenTK.Graphics.ES20.GL.GetRenderbufferParameter(OpenTK.Graphics.ES20.All.Renderbuffer, OpenTK.Graphics.ES20.All.RenderbufferHeight, &height);
	
					renderbufferWidth = width;
					renderbufferHeight = height;
				}
		    } 
			catch (Exception) 
			{
		        // device doesn't support OpenGLES 2.0; retry with 1.1:
		        ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
				base.CreateFrameBuffer();
				
				// Determine actual render buffer size (due to possible Retina Display scaling)
				// http://developer.apple.com/library/ios/#documentation/iphone/conceptual/iphoneosprogrammingguide/SupportingResolutionIndependence/SupportingResolutionIndependence.html#//apple_ref/doc/uid/TP40007072-CH10-SW11
				unsafe
				{
					int width = 0, height = 0;
					OpenTK.Graphics.ES11.GL.Oes.GetRenderbufferParameter(OpenTK.Graphics.ES11.All.RenderbufferOes, OpenTK.Graphics.ES11.All.RenderbufferWidthOes, &width);
					OpenTK.Graphics.ES11.GL.Oes.GetRenderbufferParameter(OpenTK.Graphics.ES11.All.RenderbufferOes, OpenTK.Graphics.ES11.All.RenderbufferHeightOes, &height);
	
					renderbufferWidth = width;
					renderbufferHeight = height;
				}
		    }
			
			// OpenGL Version for GraphicsDevice (Used to generate OpenGL1.1 or Opengl2.0 textures, buffers, and draw with Fixed or Fragment Pixel Shader)
			GraphicsDevice.OpenGLESVersion = ContextRenderingApi;
			GraphicsDevice.FrameBufferScreen = this.Framebuffer;
		}
		
		protected override void DestroyFrameBuffer()
		{
			if(isPausing)
				return; // see note on isPausing

			base.DestroyFrameBuffer();
		}
		
		public void Pause()
		{
			isPausing = true;
			Stop();
		}

		public void Resume()
		{
			Run();
			isPausing = false;
		}
		
		#endregion
		
		#region iPhoneOSGameView Methods
		
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
			TouchPanel.EnabledGesturesChanged -= updateGestures;
		}
		
		protected override void OnDisposed(EventArgs e)
		{
			base.OnDisposed(e);
		}
		
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad(e);
			updateGestures (null, null);
			TouchPanel.EnabledGesturesChanged += updateGestures;
		}

		void updateGestures (object sender, EventArgs e)
		{
			var enabledGestures = TouchPanel.EnabledGestures;
			
			if ((enabledGestures & GestureType.Hold) != 0)
			{
				if (recognizerLongPress == null)
				{
					recognizerLongPress = new UILongPressGestureRecognizer(this, new Selector("LongPressGestureRecognizer"));
					recognizerLongPress.MinimumPressDuration = 1.0;
					AddGestureRecognizer(recognizerLongPress);
				}
			}
			else if (recognizerLongPress != null)
			{
				RemoveGestureRecognizer(recognizerLongPress);
				recognizerLongPress = null;
			}
			
			if ((enabledGestures & GestureType.Tap) != 0)
			{
				if (recognizerTap == null)
				{
					recognizerTap = new UITapGestureRecognizer(this, new Selector("TapGestureRecognizer"));
					recognizerTap.NumberOfTapsRequired = 1;
					AddGestureRecognizer(recognizerTap);
				}
			}
			else if (recognizerTap != null)
			{
				RemoveGestureRecognizer(recognizerTap);
				recognizerTap = null;
			}
			
			if ((enabledGestures & GestureType.DoubleTap) != 0)
			{
				if (recognizerDoubleTap == null)
				{
					recognizerDoubleTap = new UITapGestureRecognizer(this, new Selector("TapGestureRecognizer"));
					recognizerDoubleTap.NumberOfTapsRequired = 2;
					AddGestureRecognizer(recognizerDoubleTap);
				}
			}
			else if (recognizerDoubleTap != null)
			{
				RemoveGestureRecognizer(recognizerDoubleTap);
				recognizerDoubleTap = null;
			}
			
			if ((enabledGestures & GestureType.FreeDrag) != 0)
			{
				if (recognizerPan == null)
				{
					recognizerPan = new UIPanGestureRecognizer(this, new Selector("PanGestureRecognizer"));
					recognizerPan.CancelsTouchesInView = false;
					AddGestureRecognizer(recognizerPan);
				}
			}
			else if (recognizerPan != null)
			{
				RemoveGestureRecognizer(recognizerPan);
				recognizerPan = null;
			}
			
			if ((enabledGestures & GestureType.Flick) != 0)
			{
				if (recognizerLeftRightSwipe == null)
				{
					recognizerLeftRightSwipe = new UISwipeGestureRecognizer(this, new Selector("SwipeGestureRecognizer"));
					recognizerLeftRightSwipe.Direction = UISwipeGestureRecognizerDirection.Down | UISwipeGestureRecognizerDirection.Up | UISwipeGestureRecognizerDirection.Left | UISwipeGestureRecognizerDirection.Right;
					AddGestureRecognizer(recognizerLeftRightSwipe);
				}
				
				if (recognizerUpDownSwipe == null)
				{
					recognizerUpDownSwipe = new UISwipeGestureRecognizer(this, new Selector("SwipeGestureRecognizer"));
					recognizerUpDownSwipe.Direction = UISwipeGestureRecognizerDirection.Left | UISwipeGestureRecognizerDirection.Right;
					AddGestureRecognizer(recognizerUpDownSwipe);
				}
			}
			else if (recognizerLeftRightSwipe != null)
			{
				RemoveGestureRecognizer(recognizerLeftRightSwipe);
				recognizerLeftRightSwipe = null;
			}
			
			if ((enabledGestures & GestureType.Flick) != 0)
			{
				if (recognizerUpDownSwipe == null)
				{
					recognizerUpDownSwipe = new UISwipeGestureRecognizer(this, new Selector("SwipeGestureRecognizer"));
					recognizerUpDownSwipe.Direction = UISwipeGestureRecognizerDirection.Up | UISwipeGestureRecognizerDirection.Down;
					AddGestureRecognizer(recognizerUpDownSwipe);
				}
			}
			else if (recognizerUpDownSwipe != null)
			{
				RemoveGestureRecognizer(recognizerUpDownSwipe);
				recognizerUpDownSwipe = null;
			}
			
			if ((enabledGestures & GestureType.Pinch) != 0)
			{
				if (recognizerPinch == null)
				{
					recognizerPinch = new UIPinchGestureRecognizer(this, new Selector("PinchGestureRecognizer"));
					AddGestureRecognizer(recognizerPinch);
				}
			}
			else if (recognizerPinch != null)
			{
				RemoveGestureRecognizer(recognizerPinch);
				recognizerPinch = null;
			}
			
			if ((enabledGestures & GestureType.Rotation) != 0)
			{
				if (recognizerRotation == null)
				{
					recognizerRotation = new UIRotationGestureRecognizer(this, new Selector("RotationGestureRecognizer"));
					AddGestureRecognizer(recognizerRotation);
				}
			}
			else if (recognizerRotation != null)
			{
				RemoveGestureRecognizer(recognizerRotation);
				recognizerRotation = null;
			}
			
		}
		
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);
			
			MakeCurrent();
						
			// This code was commented to make the code base more iPhone like.
			// More speed testing is required, to see if this is worse or better
			// game.DoStep();	
			
			_nowDraw = DateTime.Now;
			_drawGameTime.Update(_nowDraw - _lastDraw);
			_lastDraw = _nowDraw;
			_platform.Game.DoDraw(_drawGameTime);
						
			SwapBuffers();
		}
		
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
		}
		
		protected override void OnTitleChanged(EventArgs e)
		{
			base.OnTitleChanged(e);
		}
		
		protected override void OnUnload(EventArgs e)
		{
			base.OnUnload(e);
		}
		
		protected override void OnUpdateFrame(FrameEventArgs e)
		{			
			base.OnUpdateFrame(e);	
			
			_nowUpdate = DateTime.Now;
			_updateGameTime.Update(_nowUpdate - _lastUpdate);
			_lastUpdate = _nowUpdate;
			_platform.Game.DoUpdate(_updateGameTime);
		}
		
		protected override void OnVisibleChanged(EventArgs e)
		{			
			base.OnVisibleChanged(e);	
		}
		
		protected override void OnWindowStateChanged(EventArgs e)
		{		
			base.OnWindowStateChanged(e);	
		}
		
		#endregion
				
		#region UIVIew Methods	

		[Export("LongPressGestureRecognizer")]
		public void LongPressGestureRecognizer (UILongPressGestureRecognizer sender)
		{
            // FIXME: Determine the appropriate action to take here.  The XNA
            //        docs say, "This is a single event, and not continuously
            //        generated while the user is holding the touchpoint."
            //        However, iOS generates Began for that condition, then zero
            //        or more Changed notifications, and then one of the final-
            //        state notifications (Recognized, Failed, etc)
            if (sender.State == UIGestureRecognizerState.Began)
			    TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.Hold, new TimeSpan(_nowUpdate.Ticks), translatedTouchPosition, new Vector2 (sender.LocationInView (sender.View)), new Vector2(0,0), new Vector2(0,0)));
		}
		
		
		[Export("PanGestureRecognizer")]
		public void PanGestureRecognizer (UIPanGestureRecognizer sender)
		{
			if (sender.State==UIGestureRecognizerState.Ended || sender.State==UIGestureRecognizerState.Cancelled || sender.State==UIGestureRecognizerState.Failed)
				TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.DragComplete, new TimeSpan(_nowUpdate.Ticks), translatedTouchPosition, new Vector2(0,0), new Vector2 (sender.TranslationInView(sender.View)), new Vector2(0,0)));
			else
				TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.FreeDrag, new TimeSpan(_nowUpdate.Ticks), translatedTouchPosition, new Vector2(0,0), new Vector2 (sender.TranslationInView(sender.View)), new Vector2(0,0)));
		}
			
		[Export("PinchGestureRecognizer")]
		public void PinchGestureRecognizer (UIPinchGestureRecognizer sender)
		{
			TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.Pinch, new TimeSpan(_nowUpdate.Ticks), new Vector2 (sender.LocationOfTouch(0,sender.View)), new Vector2 (sender.LocationOfTouch(1,sender.View)), new Vector2(0,0), new Vector2(0,0)));
		}
		
		
		[Export("RotationGestureRecognizer")]
		public void RotationGestureRecognizer (UIRotationGestureRecognizer sender)
		{
			TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.Rotation, new TimeSpan(_nowUpdate.Ticks), new Vector2 (sender.LocationInView (sender.View)), new Vector2 (sender.LocationInView (sender.View)), new Vector2(0,0), new Vector2(0,0)));
		}
		
		[Export("SwipeGestureRecognizer")]
		public void SwipeGestureRecognizer (UISwipeGestureRecognizer sender)
		{
			TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.Flick, new TimeSpan(_nowUpdate.Ticks), new Vector2 (sender.LocationInView (sender.View)), new Vector2 (sender.LocationInView (sender.View)), new Vector2(0,0), new Vector2(0,0)));		
		}
		
		[Export("TapGestureRecognizer")]
		public void TapGestureRecognizer (UITapGestureRecognizer sender)
		{
			TouchPanel.GestureList.Enqueue(new GestureSample(GestureType.Tap, new TimeSpan(_nowUpdate.Ticks), translatedTouchPosition, new Vector2 (sender.LocationInView (sender.View)), new Vector2(0,0), new Vector2(0,0)));
		}
		
		private void FillTouchCollection(NSSet touches)
		{
			UITouch []touchesArray = touches.ToArray<UITouch>();
			
			for (int i=0; i < touchesArray.Length;i++)
			{
				
				//Get IOS touch
				UITouch touch = touchesArray[i];
				
				//Get position touch
				Vector2 position = new Vector2 (touch.LocationInView (touch.View));
				translatedTouchPosition = GetOffsetPosition(position, true);
				
				TouchLocation tlocation;
				TouchCollection collection = TouchPanel.Collection;
				int index;
				switch (touch.Phase)
				{
					case UITouchPhase.Stationary:
					case UITouchPhase.Moved:
						index = collection.FindById(touch.Handle.ToInt32(), out tlocation);
						if (index >= 0)
					    {
							tlocation.State = TouchLocationState.Moved;
							tlocation.Position = translatedTouchPosition;
							collection[index] = tlocation;
						}
						
						if (i == 0)
						{
						    Mouse.State.X = (int)translatedTouchPosition.X;
							Mouse.State.Y = (int)translatedTouchPosition.Y;
						}
						break;
					case UITouchPhase.Began	:	
						tlocation = new TouchLocation(touch.Handle.ToInt32(), TouchLocationState.Pressed, translatedTouchPosition);
						collection.Add(tlocation);
					    if (i == 0)
						{
						    Mouse.State.X = (int)translatedTouchPosition.X;
							Mouse.State.Y = (int)translatedTouchPosition.Y;
							Mouse.State.LeftButton = ButtonState.Pressed;
						}
						break;
					case UITouchPhase.Ended	:
						index = collection.FindById(touch.Handle.ToInt32(), out tlocation);
						if (index >= 0)
						{
							tlocation.State = TouchLocationState.Released;							
							collection[index] = tlocation;
						}
					
						if (i == 0)
						{
						    Mouse.State.X = (int)translatedTouchPosition.X;
							Mouse.State.Y = (int)translatedTouchPosition.Y;
							Mouse.State.LeftButton = ButtonState.Released;
						}
						break;
					case UITouchPhase.Cancelled:
						index = collection.FindById(touch.Handle.ToInt32(), out tlocation);
						if (index >= 0)
						{
							tlocation.State = TouchLocationState.Invalid;
							collection[index] = tlocation;
						}
						break;
					default :
						break;					
				}
			}			
		}
		
		internal Vector2 GetOffsetPosition(Vector2 position, bool useScale)
		{
			Vector2 translatedPosition = position * UIScreen.MainScreen.Scale;

			switch (CurrentOrientation)
			{
				case DisplayOrientation.Portrait :
				{																		
					break;
				}

				case DisplayOrientation.LandscapeRight :
				{				
					translatedPosition = new Vector2( ClientBounds.Height - translatedPosition.Y, translatedPosition.X );							
					break;
				}

				case DisplayOrientation.LandscapeLeft :
				{							
					translatedPosition = new Vector2( translatedPosition.Y, ClientBounds.Width - translatedPosition.X );							
					break;
				}

				case DisplayOrientation.PortraitUpsideDown :
				{				
					translatedPosition = new Vector2( ClientBounds.Width - translatedPosition.X, ClientBounds.Height - translatedPosition.Y );							
					break;
				}
			}
			if(!useScale)
				translatedPosition = translatedPosition / UIScreen.MainScreen.Scale;
			return translatedPosition;
		}
		
		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);
			
			FillTouchCollection(touches);
			
			GamePad.Instance.TouchesBegan(touches,evt,this);	
		}
		
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			
			FillTouchCollection(touches);	
			
			GamePad.Instance.TouchesEnded(touches,evt,this);								
		}
		
		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);
			
			FillTouchCollection(touches);
			
			GamePad.Instance.TouchesMoved(touches,evt,this);
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled (touches, evt);
			
			FillTouchCollection(touches);
			
			GamePad.Instance.TouchesCancelled(touches,evt);
		}

		#endregion
						
		public string ScreenDeviceName 
		{
			get 
			{
				throw new System.NotImplementedException ();
			}
		}

		public Rectangle ClientBounds 
		{
			get 
			{
				return clientBounds;
			}
		}
		
		public bool AllowUserResizing 
		{
			get 
			{
				return false;
			}
			set 
			{
				// Do nothing; Ignore rather than raising and exception
			}
		}	
		
		private DisplayOrientation _currentOrientation;
		public DisplayOrientation CurrentOrientation 
		{ 
			get
            {
                return _currentOrientation;
            }
            internal set
            {
                if (value != _currentOrientation)
                {
                    _currentOrientation = value;
                    if (OrientationChanged != null)
                    {
                        OrientationChanged(this, EventArgs.Empty);
                    }
				
                }
            }
		}

		public event EventHandler<EventArgs> OrientationChanged;
		public event EventHandler ClientSizeChanged;
		public event EventHandler ScreenDeviceNameChanged;
    }
}

