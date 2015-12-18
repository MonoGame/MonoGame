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
using System.Globalization;
using System.Linq;

#if PLATFORM_MACOS_LEGACY
using MonoMac.CoreAnimation;
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
using MonoMac.OpenGL;
using MonoMac.AppKit;
using NSViewResizingMaskClass = MonoMac.AppKit.NSViewResizingMask;
using RectF = System.Drawing.RectangleF;
#else
using CoreAnimation;
using Foundation;
using ObjCRuntime;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform.MacOS;
using AppKit;
using NSViewResizingMaskClass = AppKit.NSViewResizingMask;
using RectF = CoreGraphics.CGRect;
using PointF = CoreGraphics.CGPoint;
using SizeF = CoreGraphics.CGSize;
#endif

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

#endregion Using Statements

namespace Microsoft.Xna.Framework
{
	[CLSCompliant(false)]
	public class GameWindow : MonoMacGameView
	{
		//private readonly Rectangle clientBounds;
		private Rectangle clientBounds;
		private Game _game;
		private MacGamePlatform _platform;
        internal MouseState MouseState;
        internal TouchPanelState TouchPanelState;

		private NSTrackingArea _trackingArea;
		private bool _needsToResetElapsedTime = false;

        public static Func<Game, RectF, GameWindow> CreateWindowDelegate 
		{
			get;
			set;
		}

		#region GameWindow Methods
        public GameWindow(Game game, RectF frame) : base (frame)
		{
            if (game == null)
                throw new ArgumentNullException("game");
            _game = game;
            _platform = (MacGamePlatform)_game.Services.GetService(typeof(MacGamePlatform));
            TouchPanelState = new TouchPanelState(this);

			//LayerRetainsBacking = false; 
			//LayerColorFormat	= EAGLColorFormat.RGBA8;
            this.AutoresizingMask = NSViewResizingMaskClass.HeightSizable
                | NSViewResizingMaskClass.MaxXMargin 
                | NSViewResizingMaskClass.MinYMargin
                | NSViewResizingMaskClass.WidthSizable;
			
            var rect = NSScreen.MainScreen.Frame;
			
			clientBounds = new Rectangle (0,0,(int)rect.Width,(int)rect.Height);

			// Enable multi-touch
			//MultipleTouchEnabled = true;
			
			Mouse.Window = this;
		}

        public GameWindow(Game game, RectF frame, NSOpenGLContext context) :
            this(game, frame)
		{
		}

		[Export("initWithFrame:")]
		public GameWindow () : base (NSScreen.MainScreen.Frame)
		{
            this.AutoresizingMask = NSViewResizingMaskClass.HeightSizable
                | NSViewResizingMaskClass.MaxXMargin 
                | NSViewResizingMaskClass.MinYMargin
                | NSViewResizingMaskClass.WidthSizable;

			var rect = NSScreen.MainScreen.Frame;
			clientBounds = new Rectangle (0,0,(int)rect.Width,(int)rect.Height);

			// Enable multi-touch
			//MultipleTouchEnabled = true;

		}

		~GameWindow ()
		{
			//
		}

		#endregion

		public void StartRunLoop(double updateRate)
		{
			Run(updateRate);
		}

		public void ResetElapsedTime ()
		{
			_needsToResetElapsedTime = true;
		}
		#region MonoMacGameView Methods

		protected override void OnClosed (EventArgs e)
		{
			base.OnClosed (e);
		}

		protected override void OnDisposed (EventArgs e)
		{
			base.OnDisposed (e);
		}

		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			Title = MonoGame.Utilities.AssemblyHelper.GetDefaultWindowTitle();
		}

		protected override void OnRenderFrame (FrameEventArgs e)
		{
			base.OnRenderFrame (e);

            // FIXME: Since Game.Exit may be called during an Update loop (and
            //        in fact that is quite likely to happen), this code is now
            //        littered with checks to _platform.IsRunning.  It would be
            //        nice if there weren't quite so many.  The move to a
            //        Game.Tick-centric architecture may eliminate this problem
            //        automatically.
			if (_game != null && _platform.IsRunning) {
                if (_needsToResetElapsedTime) 
                {
                    _game.ResetElapsedTime ();
					_needsToResetElapsedTime = false;
                }
				_game.Tick();
			}
		}
//		protected override void OnUpdateFrame (FrameEventArgs e)
//		{
//			base.OnUpdateFrame (e);
//
//		}
		protected override void OnResize (EventArgs e)
		{
            var manager = (GraphicsDeviceManager)_game.Services.GetService(typeof(IGraphicsDeviceManager));
            if (_game.Initialized)
            {
    			manager.OnDeviceResetting(EventArgs.Empty);
    			
    			Microsoft.Xna.Framework.Graphics.Viewport _vp =
    			new Microsoft.Xna.Framework.Graphics.Viewport();
    			
    			_game.GraphicsDevice.PresentationParameters.BackBufferWidth = (int)Bounds.Width;
    			_game.GraphicsDevice.PresentationParameters.BackBufferHeight = (int)Bounds.Height;

    			_vp.X = (int)Bounds.X;
    			_vp.Y = (int)Bounds.Y;
    			_vp.Width = (int)Bounds.Width;
    			_vp.Height = (int)Bounds.Height;

    			_game.GraphicsDevice.Viewport = _vp;
            }
			
			clientBounds = new Rectangle((int)Bounds.X,(int)Bounds.Y,(int)Bounds.Width,(int)Bounds.Height);
			
			base.OnResize(e);
			OnClientSizeChanged(e);

            if (_game.Initialized)
    			manager.OnDeviceReset(EventArgs.Empty);
		}
		
		protected virtual void OnClientSizeChanged (EventArgs e)
		{
			var h = ClientSizeChanged;
			if (h != null)
				h (this, e);
		}
		
		protected override void OnTitleChanged (EventArgs e)
		{
			base.OnTitleChanged (e);
		}

		protected override void OnUnload (EventArgs e)
		{
			base.OnUnload (e);
		}

		protected override void OnVisibleChanged (EventArgs e)
		{			
			base.OnVisibleChanged (e);	
		}

		protected override void OnWindowStateChanged (EventArgs e)
		{		
			base.OnWindowStateChanged (e);	
		}

		#endregion

		#region UIVIew Methods

		/* TODO private readonly Dictionary<IntPtr, TouchLocation> previousTouches = new Dictionary<IntPtr, TouchLocation>();

		private void FillTouchCollection(NSSet touches)
		{
			UITouch []touchesArray = touches.ToArray<UITouch>();

			TouchPanel.Collection.Clear();
			TouchPanel.Collection.Capacity = touchesArray.Length;

			for (int i=0; i<touchesArray.Length;i++)
			{
				TouchLocationState state;				
				UITouch touch = touchesArray[i];
				switch (touch.Phase)
				{
					case UITouchPhase.Began	:	
						state = TouchLocationState.Pressed;
						break;
					case UITouchPhase.Cancelled	:
					case UITouchPhase.Ended	:
						state = TouchLocationState.Released;
						break;
					default :
						state = TouchLocationState.Moved;
						break;					
				}

				TouchLocation tlocation;
				TouchLocation previousTouch;
				if (state != TouchLocationState.Pressed && previousTouches.TryGetValue (touch.Handle, out previousTouch))
				{
					Vector2 position = new Vector2 (touch.LocationInView (touch.View));
					Vector2 translatedPosition = position;

					switch (CurrentOrientation)
					{
						case DisplayOrientation.Portrait :
						{																		
							break;
						}

						case DisplayOrientation.LandscapeRight :
						{				
							translatedPosition = new Vector2( ClientBounds.Height - position.Y, position.X );							
							break;
						}

						case DisplayOrientation.LandscapeLeft :
						{							
							translatedPosition = new Vector2( position.Y, ClientBounds.Width - position.X );							
							break;
						}

						case DisplayOrientation.PortraitDown :
						{				
							translatedPosition = new Vector2( ClientBounds.Width - position.X, ClientBounds.Height - position.Y );							
							break;
						}
					}
					tlocation = new TouchLocation(touch.Handle.ToInt32(), state, translatedPosition, 1.0f, previousTouch.State, previousTouch.Position, previousTouch.Pressure);
				}
				else
				{
					Vector2 position = new Vector2 (touch.LocationInView (touch.View));
					Vector2 translatedPosition = position;

					switch (CurrentOrientation)
					{
						case DisplayOrientation.Portrait :
						{																		
							break;
						}

						case DisplayOrientation.LandscapeRight :
						{				
							translatedPosition = new Vector2( ClientBounds.Height - position.Y, position.X );							
							break;
						}

						case DisplayOrientation.LandscapeLeft :
						{							
							translatedPosition = new Vector2( position.Y, ClientBounds.Width - position.X );							
							break;
						}

						case DisplayOrientation.PortraitDown :
						{				
							translatedPosition = new Vector2( ClientBounds.Width - position.X, ClientBounds.Height - position.Y );							
							break;
						}
					}
					tlocation = new TouchLocation(touch.Handle.ToInt32(), state, translatedPosition, 1.0f);
				}

				TouchPanel.Collection.Add (tlocation);

				if (state != TouchLocationState.Released)
					previousTouches[touch.Handle] = tlocation;
				else
					previousTouches.Remove(touch.Handle);
			}
		}

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			base.TouchesBegan (touches, evt);

			FillTouchCollection(touches);

			GamePad.Instance.TouchesBegan(touches,evt);	
		}

		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);

			FillTouchCollection(touches);	

			GamePad.Instance.TouchesEnded(touches,evt);								
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			base.TouchesMoved (touches, evt);

			FillTouchCollection(touches);

			GamePad.Instance.TouchesMoved(touches,evt);
		}

		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			base.TouchesCancelled (touches, evt);

			FillTouchCollection(touches);

			GamePad.Instance.TouchesCancelled(touches,evt);
		} */

		#endregion

		public string ScreenDeviceName {
			get {
				throw new System.NotImplementedException ();
			}
		}

		public Rectangle ClientBounds {
			get {
				return clientBounds;
			}
		}

		public bool AllowUserResizing {
			get { return (Window.StyleMask & NSWindowStyle.Resizable) == NSWindowStyle.Resizable; }
			set
            {
                if (value)
					Window.StyleMask |= NSWindowStyle.Resizable;
                else
                    Window.StyleMask &= ~NSWindowStyle.Resizable;

				Window.StandardWindowButton(NSWindowButton.ZoomButton).Enabled = value;
			}
		}	

		private DisplayOrientation _currentOrientation;

		public DisplayOrientation CurrentOrientation { 
			get {
				return _currentOrientation;
			}
			internal set {
				if (value != _currentOrientation) {
					_currentOrientation = value;
					if (OrientationChanged != null) {
						OrientationChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		public event EventHandler<EventArgs> ClientSizeChanged;
		public event EventHandler<EventArgs> OrientationChanged;
		public event EventHandler<EventArgs> ScreenDeviceNameChanged;

		private bool SuppressEventHandlerWarningsUntilEventsAreProperlyImplemented()
		{
			return ScreenDeviceNameChanged != null;
		}
		
		// make sure we get mouse move events.
		public override bool AcceptsFirstResponder ()
		{
			return true;
		}

		public override bool BecomeFirstResponder ()
		{
			return true;
		}
		public override void CursorUpdate (NSEvent theEvent)
		{
			base.CursorUpdate (theEvent);
		}

		public override void ViewWillMoveToWindow (NSWindow newWindow)
		{
			//Console.WriteLine("View will move to window");
			if (_trackingArea != null) RemoveTrackingArea(_trackingArea);
			_trackingArea = new NSTrackingArea(Frame,
			                      	NSTrackingAreaOptions.MouseMoved | 
			                        NSTrackingAreaOptions.MouseEnteredAndExited |
			                        NSTrackingAreaOptions.EnabledDuringMouseDrag |
			                        NSTrackingAreaOptions.ActiveWhenFirstResponder |
			                        NSTrackingAreaOptions.InVisibleRect |
				NSTrackingAreaOptions.CursorUpdate,
			                      this, new NSDictionary());
			AddTrackingArea(_trackingArea);

		}

		// These variables are to handle our custom cursor for when IsMouseVisible is false.
		// Hiding and unhiding the cursor was such a pain that I decided to let the system
		// take care of this with Cursor Rectangles
		NSImage cursorImage = null;	// Will be set to our custom image
		NSCursor cursor = null;		// Our custom cursor
		public override void ResetCursorRects ()
		{

			// If we do not have a cursor then we create an image size 1 x 1
			// and then create our custom cursor with clear colors
			if (cursor == null) {
				cursorImage = new NSImage(new SizeF(1,1));
				cursor = new NSCursor(cursorImage, NSColor.Clear, NSColor.Clear, new PointF(0,0));
			}

			// if the cursor is not to be visible then we us our custom cursor.
			if (!_game.IsMouseVisible)
				AddCursorRect(Frame, cursor);
			else
				AddCursorRect(Frame, NSCursor.ArrowCursor);

		}

		public override void DiscardCursorRects ()
		{
			base.DiscardCursorRects ();
			//Console.WriteLine("DiscardCursorRects");
		}
		private void UpdateKeyboardState ()
		{
			_keyStates.Clear ();
			_keyStates.AddRange (_flags);
			_keyStates.AddRange (_keys);
			Keyboard.SetKeys(_keyStates);
		}
		
		// This method should only be called when necessary like when the Guide is displayed
		internal void ClearKeyCacheState() {
			_keys.Clear();	
		}
		
		List<Keys> _keys = new List<Keys> ();
		List<Keys> _keyStates = new List<Keys> ();

		public override void KeyDown (NSEvent theEvent)
		{
			if (!string.IsNullOrEmpty (theEvent.Characters) && theEvent.Characters.All (c => char.GetUnicodeCategory (c) != UnicodeCategory.PrivateUse)) 
			{
				foreach(char c in theEvent.Characters)
				{
					OnTextInput(new TextInputEventArgs(c));
				}
			}

			Keys kk = KeyUtil.GetKeys (theEvent); 

			if (!_keys.Contains (kk))
				_keys.Add (kk);

			UpdateKeyboardState ();
		}

		public override void KeyUp (NSEvent theEvent)
		{
			Keys kk = KeyUtil.GetKeys (theEvent); 

			_keys.Remove (kk);

			UpdateKeyboardState ();
		}

		protected void OnTextInput(TextInputEventArgs e)
		{
			if (e == null) 
			{
				throw new ArgumentNullException("e");
			}
			
			if (TextInput != null) 
			{
				TextInput.Invoke(this, e);
			}
		}
		
		/// <summary>
		/// Use this event to retrieve text for objects like textbox's.
		/// This event is not raised by noncharacter keys.
		/// This event also supports key repeat.
		/// </summary>
		public event EventHandler<TextInputEventArgs> TextInput;

		List<Keys> _flags = new List<Keys> ();

		public override void FlagsChanged (NSEvent theEvent)
		{

			_flags.Clear ();
			var modInt = (uint)theEvent.ModifierFlags & 0xFFFF0000;
			var modifier = ((NSEventModifierMask)Enum.ToObject (typeof(NSEventModifierMask), modInt));

			switch (modifier) {
			//case NSEventModifierMask.AlphaShiftKeyMask:
			// return Keys.None;
			case NSEventModifierMask.AlternateKeyMask:
				_flags.Add (Keys.LeftAlt);
				_flags.Add (Keys.RightAlt);
				break;

			case NSEventModifierMask.CommandKeyMask:
				_flags.Add (Keys.LeftWindows);
				_flags.Add (Keys.RightWindows);
				break;
			case NSEventModifierMask.ControlKeyMask:
				_flags.Add (Keys.LeftControl);
				_flags.Add (Keys.RightControl);
				break;
			case NSEventModifierMask.HelpKeyMask:
				_flags.Add (Keys.Help);
				break;
			case NSEventModifierMask.ShiftKeyMask:
				_flags.Add (Keys.RightShift);
				_flags.Add (Keys.LeftShift);
				break;
			}

			UpdateKeyboardState ();
		}

		public override void MouseDown (NSEvent theEvent)
		{
			PointF loc = theEvent.LocationInWindow;
			UpdateMousePosition (loc);
			switch (theEvent.Type) {
			case NSEventType.LeftMouseDown:
				MouseState.LeftButton = ButtonState.Pressed;
				break;
			}
		}

		public override void MouseUp (NSEvent theEvent)
		{
			PointF loc = theEvent.LocationInWindow;
			UpdateMousePosition (loc);
			switch (theEvent.Type) {

			case NSEventType.LeftMouseUp:
				MouseState.LeftButton = ButtonState.Released;
				break;
			}
		}
		
		public override void MouseDragged (NSEvent theEvent)
		{
			PointF loc = theEvent.LocationInWindow;
			UpdateMousePosition (loc);
		}
		
		public override void RightMouseDown (NSEvent theEvent)
		{
			PointF loc = theEvent.LocationInWindow;
			UpdateMousePosition (loc);
			switch (theEvent.Type) {
			case NSEventType.RightMouseDown:
				MouseState.RightButton = ButtonState.Pressed;
				break;
			}
		}
		
		public override void RightMouseUp (NSEvent theEvent)
		{
			PointF loc = theEvent.LocationInWindow;
			UpdateMousePosition (loc);
			switch (theEvent.Type) {
			case NSEventType.RightMouseUp:
				MouseState.RightButton = ButtonState.Released;
				break;
			}
		}
		
		public override void RightMouseDragged (NSEvent theEvent)
		{
			PointF loc = theEvent.LocationInWindow;
			UpdateMousePosition (loc);
		}
		
		public override void OtherMouseDown (NSEvent theEvent)
		{
			PointF loc = theEvent.LocationInWindow;
			UpdateMousePosition (loc);
			switch (theEvent.Type) {
			case NSEventType.OtherMouseDown:
				MouseState.MiddleButton = ButtonState.Pressed;
				break;
			}
		}
		
		public override void OtherMouseUp (NSEvent theEvent)
		{
			PointF loc = theEvent.LocationInWindow;
			UpdateMousePosition (loc);
			switch (theEvent.Type) {
			case NSEventType.OtherMouseUp:
				MouseState.MiddleButton = ButtonState.Released;
				break;
			}
		}
		
		public override void OtherMouseDragged (NSEvent theEvent)
		{
			PointF loc = theEvent.LocationInWindow;
			UpdateMousePosition (loc);
		}
		
		public override void ScrollWheel (NSEvent theEvent)
		{ 
			PointF loc = theEvent.LocationInWindow; 
			UpdateMousePosition (loc); 
			switch (theEvent.Type) 
			{ 
			case NSEventType.ScrollWheel: 
				if (theEvent.ScrollingDeltaY != 0) 
				{ 
					if (theEvent.ScrollingDeltaY > 0) 
					{ 
                        Mouse.ScrollWheelValue += (float)(theEvent.ScrollingDeltaY * 0.1f + 0.09f) * 1200; 
					} 
					else 
					{ 
                        Mouse.ScrollWheelValue += (float)(theEvent.ScrollingDeltaY * 0.1f - 0.09f) * 1200; 
					} 
				} 
				break; 
			} 
		}

		public override void MouseMoved (NSEvent theEvent)
		{
			PointF loc = theEvent.LocationInWindow;
			UpdateMousePosition (loc);

			switch (theEvent.Type) {
				case NSEventType.MouseMoved:
				//Mouse.Moved = true;
				break;
			}			
		}

		private void UpdateMousePosition (PointF location)
		{
			MouseState.X = (int)location.X;
			MouseState.Y = (int)(ClientBounds.Height - location.Y);			
		}

		internal void SetSupportedOrientations(DisplayOrientation orientations)
		{
		}
	}
}

