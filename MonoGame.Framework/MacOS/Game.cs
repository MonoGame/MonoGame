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
using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

using MonoMac.CoreAnimation;
using MonoMac.CoreFoundation;
using MonoMac.Foundation;
using MonoMac.OpenGL;
using MonoMac.AppKit;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
	public class Game : IDisposable
	{
		private const float FramesPerSecond = 60.0f; // ~60 frames per second

		private GameTime _updateGameTime;
		private GameTime _drawGameTime;
		private DateTime _lastUpdate;
		internal bool _initialized = false;
		private bool _initializing = false;
		private bool _isActive = true;
		private GameComponentCollection _gameComponentCollection;
		public GameServiceContainer _services;
		private ContentManager _content;
		private GameWindow _view;
		private bool _isFixedTimeStep = true;
		private TimeSpan _targetElapsedTime = TimeSpan.FromSeconds (1 / FramesPerSecond); 
		private IGraphicsDeviceManager graphicsDeviceManager;
		private IGraphicsDeviceService graphicsDeviceService;
		private NSWindow _mainWindow;
		internal static bool _playingVideo = false;
		private SpriteBatch spriteBatch;
		private Texture2D splashScreen;
		private bool _mouseVisible = false;
		private List<IGameComponent> _gameComponentsToInitialize = new List<IGameComponent>();

		delegate void InitialiseGameComponentsDelegate ();

		public Game ()
		{
			// Initialize collections
			_services = new GameServiceContainer ();
			_gameComponentCollection = new GameComponentCollection ();
			
			_gameComponentCollection.ComponentAdded += Handle_gameComponentCollectionComponentAdded;
			
			// The default for Windows is 480 x 800
			//RectangleF frame = NSScreen.MainScreen.Frame;
			RectangleF frame = new RectangleF(0,0,Microsoft.Xna.Framework.Graphics.PresentationParameters._defaultBackBufferWidth,
				Microsoft.Xna.Framework.Graphics.PresentationParameters._defaultBackBufferHeight);

			//Create a full-screen window
			_mainWindow = new NSWindow (frame, NSWindowStyle.Titled | NSWindowStyle.Closable, NSBackingStore.Buffered, true);

			// Perform any other window configuration you desire
			_mainWindow.IsOpaque = true;
			_mainWindow.HidesOnDeactivate = true;

			_view = new GameWindow (frame);
			_view.game = this;

			_mainWindow.ContentView.AddSubview (_view);
			_mainWindow.AcceptsMouseMovedEvents = true;

			// Initialize GameTime
			_updateGameTime = new GameTime ();
			_drawGameTime = new GameTime ();  

		}

		void Handle_gameComponentCollectionComponentAdded (object sender, GameComponentCollectionEventArgs e)
		{
			if (!_initialized && !_initializing) {
				e.GameComponent.Initialize();
			}
			else {
				_gameComponentsToInitialize.Add(e.GameComponent);
			}					
		}

		~Game ()
		{
			// TODO NSDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications(); 
		}

		internal bool IsAllowUserResizing {
			get {
				return (_mainWindow.StyleMask & NSWindowStyle.Resizable) > 0;
			}

			set {
				if (IsAllowUserResizing != value)
					_mainWindow.StyleMask ^= NSWindowStyle.Resizable;
			}

		}
		/* private void ObserveDeviceRotation ()
		{
			NSNotificationCenter.DefaultCenter.AddObserver( new NSString("UIDeviceOrientationDidChangeNotification"), (notification) => { 
				UIDeviceOrientation orientation = UIDevice.CurrentDevice.Orientation;

				switch (orientation)
				{
					case UIDeviceOrientation.Portrait :
						if ( (graphicsDeviceManager as GraphicsDeviceManager).SupportedOrientations == DisplayOrientation.Portrait)
						{
							_view.CurrentOrientation = DisplayOrientation.Portrait;
							GraphicsDevice.PresentationParameters.DisplayOrientation = DisplayOrientation.Portrait;
							TouchPanel.DisplayOrientation = DisplayOrientation.Portrait;
						}
						break;
					case UIDeviceOrientation.LandscapeLeft :
						switch ((graphicsDeviceManager as GraphicsDeviceManager).SupportedOrientations)
						{
							case DisplayOrientation.LandscapeLeft:
							case DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight :
							{
								_view.CurrentOrientation = DisplayOrientation.LandscapeLeft;
								GraphicsDevice.PresentationParameters.DisplayOrientation = DisplayOrientation.LandscapeLeft;
								TouchPanel.DisplayOrientation = DisplayOrientation.LandscapeLeft;
								break;
							}
						}
						break;
					case UIDeviceOrientation.LandscapeRight :
						switch ((graphicsDeviceManager as GraphicsDeviceManager).SupportedOrientations)
						{
							case DisplayOrientation.LandscapeRight:
							case DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight :
							{
								_view.CurrentOrientation = DisplayOrientation.LandscapeRight;
								GraphicsDevice.PresentationParameters.DisplayOrientation = DisplayOrientation.LandscapeRight;
								TouchPanel.DisplayOrientation = DisplayOrientation.LandscapeRight;
								break;
							}
						}
						break;
					case UIDeviceOrientation.FaceDown :
						if ( (graphicsDeviceManager as GraphicsDeviceManager).SupportedOrientations == DisplayOrientation.FaceDown)
						{
							_view.CurrentOrientation = DisplayOrientation.FaceDown;
							TouchPanel.DisplayOrientation = DisplayOrientation.FaceDown;
						}
						break;
					case UIDeviceOrientation.FaceUp :
						if ( (graphicsDeviceManager as GraphicsDeviceManager).SupportedOrientations == DisplayOrientation.FaceDown)
						{
							_view.CurrentOrientation = DisplayOrientation.FaceUp;
							TouchPanel.DisplayOrientation = DisplayOrientation.FaceUp;
						}
						break;
					case UIDeviceOrientation.PortraitUpsideDown :
						if ( (graphicsDeviceManager as GraphicsDeviceManager).SupportedOrientations == DisplayOrientation.PortraitUpsideDown)
						{
							_view.CurrentOrientation = DisplayOrientation.PortraitUpsideDown;
							TouchPanel.DisplayOrientation = DisplayOrientation.PortraitUpsideDown;
						}
						break;
					case UIDeviceOrientation.Unknown :
						if ( (graphicsDeviceManager as GraphicsDeviceManager).SupportedOrientations == DisplayOrientation.Unknown)
						{
							_view.CurrentOrientation = DisplayOrientation.Unknown;
							TouchPanel.DisplayOrientation = DisplayOrientation.Unknown;
						}
						break;						
					default:
						if ( (graphicsDeviceManager as GraphicsDeviceManager).SupportedOrientations == DisplayOrientation.Default)
						{
							_view.CurrentOrientation = DisplayOrientation.Default;
							TouchPanel.DisplayOrientation = DisplayOrientation.Default;
						}
						break;
				}					  
			});

			UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
		}*/

		public void Dispose ()
		{
			// do nothing
		}

		public bool IsActive {
			get {
				return _isActive;
			}
			protected set {
				if (_isActive != value) {
					_isActive = value;
				}
			}
		}

		public bool IsMouseVisible {
			get {
				return _mouseVisible;
			}
			set {
				_mouseVisible = value;
				if (_mouseVisible) {
					NSCursor.Unhide();
				}
			}
		}

		public TimeSpan TargetElapsedTime {
			get {
				return _targetElapsedTime;
			}
			set {
				_targetElapsedTime = value;			
				if (_initialized) {
					throw new NotSupportedException ();
				}
			}
		}

		public void Run ()
		{			
			_lastUpdate = DateTime.Now;

			Initialize ();

			_mainWindow.MakeKeyAndOrderFront (_mainWindow);

			_view.Run (FramesPerSecond / (FramesPerSecond * TargetElapsedTime.TotalSeconds));
			//_view.Run();
			/*TODO _view.MainContext = _view.EAGLContext;
			_view.ShareGroup = _view.MainContext.ShareGroup;
			_view.BackgroundContext = new MonoTouch.OpenGLES.EAGLContext(_view.ContextRenderingApi, _view.ShareGroup); */

			//Show the window			
			//_mainWindow.MakeKeyWindow ();	

			// Get the Accelerometer going
			// TODO Accelerometer.SetupAccelerometer();			


			// Listen out for rotation changes
			// TODO ObserveDeviceRotation();
		}

		internal void DoUpdate (GameTime aGameTime)
		{
			if (_isActive) {
				Update (aGameTime);
			}
		}

		internal void DoDraw (GameTime aGameTime)
		{
			if (_isActive) {
				Draw (aGameTime);
			}
		}

		internal void DoStep ()
		{
			var timeNow = DateTime.Now;

			// Update the game			
			_updateGameTime.Update (timeNow - _lastUpdate);
			Update (_updateGameTime);

			// Draw the screen
			_drawGameTime.Update (timeNow - _lastUpdate);
			_lastUpdate = timeNow;
			Draw (_drawGameTime);       			
		}

		public bool IsFixedTimeStep {
			get {
				return _isFixedTimeStep;
			}
			set {
				_isFixedTimeStep = value;
			}
		}

		public GameWindow Window {
			get {
				return _view;
			}
		}

		public void ResetElapsedTime ()
		{
			_lastUpdate = DateTime.Now;
		}

		public GameServiceContainer Services {
			get {
				return _services;
			}
		}

		public ContentManager Content {
			get {
				if (_content == null) {
					_content = new ContentManager (_services);
				}
				return _content;
			}
		}

		public GraphicsDevice GraphicsDevice {
			get {
				if (this.graphicsDeviceService == null) {
					this.graphicsDeviceService = this.Services.GetService (typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
					if (this.graphicsDeviceService == null) {
						throw new InvalidOperationException ("No Graphics Device Service");
					}
				}
				return this.graphicsDeviceService.GraphicsDevice;
			}
		}

		public void EnterBackground ()
		{
			_isActive = false;
			if (Deactivated != null)
				Deactivated.Invoke (this, null);
		}

		public void EnterForeground ()
		{
			_isActive = true;
			if (Activated != null)
				Activated.Invoke (this, null);
		}

		protected virtual bool BeginDraw ()
		{
			return true;
		}

		protected virtual void EndDraw ()
		{

		}

		protected virtual void LoadContent ()
		{			
			string DefaultPath = "Default.png";
			if (File.Exists (DefaultPath)) {
				// Store the RootDir for later 
				string backup = Content.RootDirectory;

				try {
					// Clear the RootDirectory for this operation
					Content.RootDirectory = string.Empty;

					spriteBatch = new SpriteBatch (GraphicsDevice);
					splashScreen = Content.Load<Texture2D> (DefaultPath);			
				} finally {
					// Reset RootDir
					Content.RootDirectory = backup;
				}

			} else {
				spriteBatch = null;
				splashScreen = null;
			}
		}

		protected virtual void UnloadContent ()
		{
			// do nothing
		}

		private float TitleBarHeight ()
		{
			RectangleF contentRect = NSWindow.ContentRectFor (_mainWindow.Frame, _mainWindow.StyleMask);
			return _mainWindow.Frame.Height - contentRect.Height;
		}

		private void ResetWindowBounds ()
		{
			RectangleF frame = _mainWindow.Frame;
			RectangleF content = _view.Bounds;

			frame.Width = ((GraphicsDeviceManager)graphicsDeviceManager).PreferredBackBufferWidth;
			frame.Height = ((GraphicsDeviceManager)graphicsDeviceManager).PreferredBackBufferHeight + TitleBarHeight ();

			content.Width = ((GraphicsDeviceManager)graphicsDeviceManager).PreferredBackBufferWidth;
			content.Height = ((GraphicsDeviceManager)graphicsDeviceManager).PreferredBackBufferHeight;

			_mainWindow.SetFrame (frame, true);

			_view.Size = new Size ((int)content.Width,(int)content.Height);			
		}

		protected virtual void Initialize ()
		{

			this.graphicsDeviceManager = this.Services.GetService (typeof(IGraphicsDeviceManager)) as IGraphicsDeviceManager;			
			this.graphicsDeviceService = this.Services.GetService (typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;			
			
			ResetWindowBounds();
			
			if ((this.graphicsDeviceService != null) && (this.graphicsDeviceService.GraphicsDevice != null)) {
				LoadContent ();
			}
		}

		private void InitializeGameComponents ()
		{
			// There is no autorelease pool when this method is called because it will be called from a background thread
			// It's important to create one or you will leak objects
			using (NSAutoreleasePool pool = new NSAutoreleasePool ()) {
				
				// Leave the following code there just in case there are problems
				// with the intialization hack.
				//foreach (GameComponent gc in _gameComponentCollection) {
				foreach (IGameComponent gc in _gameComponentsToInitialize) {
					// We may be drawing on a secondary thread through the display link or timer thread.
					// Add a mutex around to avoid the threads accessing the context simultaneously
					_view.OpenGLContext.CGLContext.Lock ();

					// set our current context
					_view.MakeCurrent ();

					gc.Initialize ();

					// now unlock it
					_view.OpenGLContext.CGLContext.Unlock ();
					_gameComponentsToInitialize.Remove(gc);
				}
			}							
		}

		protected virtual void Update (GameTime gameTime)
		{			
			if (_initialized  /* TODO && !Guide.IsVisible */) {
				foreach (GameComponent gc in _gameComponentCollection) {
					if (gc.Enabled) {
						gc.Update (gameTime);
					}
				}
			} else {
				if (!_initializing) {
					_initializing = true;

					// Use OpenGL context locking in delegate function
					InitialiseGameComponentsDelegate initD = new InitialiseGameComponentsDelegate (InitializeGameComponents);

					// Invoke on thread from the pool
					initD.BeginInvoke (
						delegate (IAsyncResult iar) 
						{
							// We must have finished initialising, so set our flag appropriately
							// So that we enter the Update loop
							_initialized = true;
							_initializing = false;
						}, 
					initD);
				}
			}
		}

		protected virtual void Draw (GameTime gameTime)
		{
			if (_initializing) {
				if (spriteBatch != null) {
					spriteBatch.Begin ();

					// We need to turn this into a progress bar or animation to give better user feedback
					spriteBatch.Draw (splashScreen, new Vector2 (0, 0), Microsoft.Xna.Framework.Color.White);
					spriteBatch.End ();
				}
			} else {
				if (!_playingVideo) {
					foreach (GameComponent gc in _gameComponentCollection) {
						if (gc.Enabled && gc is DrawableGameComponent) {
							DrawableGameComponent dc = gc as DrawableGameComponent;
							if (dc.Visible) {
								dc.Draw (gameTime);
							}
						}
					}
				}
			}
		}

		public void Exit ()
		{
			using ( NSAlert alert = NSAlert.WithMessage("Game Exit", "Ok", "Cancel", null, "Are you sure you wish to exit?"))
			{
				var button = alert.RunModal();
				
				if ( button == 1 )
				{
					NSApplication.SharedApplication.Terminate(new NSObject());		
				}
			}
		}

		public GameComponentCollection Components {
			get {
				return _gameComponentCollection;
			}
		}

		#region Events
		public event EventHandler Activated;
		public event EventHandler Deactivated;
		public event EventHandler Disposed;
		public event EventHandler Exiting;
		#endregion
	}
}

