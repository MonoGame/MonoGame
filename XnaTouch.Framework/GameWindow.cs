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

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Platform.iPhoneOS;
using OpenTK.Graphics.ES11;
using OpenTK.Graphics.ES20;

using XnaTouch.Framework.Input;
using XnaTouch.Framework.Input.Touch;
#endregion Using Statements

namespace XnaTouch.Framework
{
    public class GameWindow : iPhoneOSGameView
    {
		private readonly Rectangle clientBounds;
		internal Game game;
		private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;
		private DateTime _now;
		
		public EAGLContext MainContext;
	    public EAGLContext BackgroundContext;
	    public EAGLSharegroup ShareGroup; 
				
		#region UIVIew Methods
		
		public GameWindow() : base (UIScreen.MainScreen.Bounds)
		{
			LayerRetainsBacking = false; 
			LayerColorFormat	= EAGLColorFormat.RGBA8;
			
			RectangleF rect = UIScreen.MainScreen.Bounds;
			clientBounds = new Rectangle(0,0,(int) rect.Width,(int) rect.Height);
			
			// Enable multi-touch
			MultipleTouchEnabled = true;
						
			// Initialize GameTime
            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime();  	
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
		
		protected override void ConfigureLayer(CAEAGLLayer eaglLayer) 
		{
			eaglLayer.Opaque = true;
		}
		
		protected override void CreateFrameBuffer()
		{	    
			try
			{
		        // TODO ContextRenderingApi = EAGLRenderingAPI.OpenGLES2;
				ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
				base.CreateFrameBuffer();
		    } 
			catch (Exception) 
			{
		        // device doesn't support OpenGLES 2.0; retry with 1.1:
		        ContextRenderingApi = EAGLRenderingAPI.OpenGLES1;
				base.CreateFrameBuffer();
		    }
			
			
		}
		
		#endregion
		
		#region iPhoneOSGameView Methods
		
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);
		}
		
		protected override void OnDisposed(EventArgs e)
		{
			base.OnDisposed(e);
		}
		
		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad(e);
		}
		
		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);
			
			MakeCurrent();
						
			// This code was commented to make the code base more iPhone like.
			// More speed testing is required, to see if this is worse or better
			// game.DoStep();	
			
			if (game != null )
			{
				_drawGameTime.Update(_now - _lastUpdate);
            	_lastUpdate = _now;
            	game.DoDraw(_drawGameTime);
			}
						
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
			
			if (game != null )
			{
				_now = DateTime.Now;
				_updateGameTime.Update(_now - _lastUpdate);
            	game.DoUpdate(_updateGameTime);
			}
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
						
		private readonly Dictionary<IntPtr, TouchLocation> previousTouches = new Dictionary<IntPtr, TouchLocation>();
		
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
					tlocation = new TouchLocation(touch.Handle.ToInt32(), state, new Vector2 (touch.LocationInView (touch.View)), 1.0f, previousTouch.State, previousTouch.Position, previousTouch.Pressure);
				else
					tlocation = new TouchLocation(touch.Handle.ToInt32(), state, new Vector2 (touch.LocationInView (touch.View)), 1.0f);
				
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
		
		public DisplayOrientation CurrentOrientation 
		{ 
			get;
			set;
		}

		
		public event EventHandler ClientSizeChanged;
		public event EventHandler ScreenDeviceNameChanged;
    }
}

