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

		 
		// I do believe we can take out the next three variables.
		//  After the release this should be looked at as the time is
		//  passed from the GameWindow which controlls when updating is to
		//  be done.
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
		private IGraphicsDeviceManager _graphicsDeviceManager;
		private IGraphicsDeviceService graphicsDeviceService;
		private NSWindow _mainWindow;
		internal static bool _playingVideo = false;
		private SpriteBatch spriteBatch;
		private Texture2D splashScreen;
		private bool _mouseVisible = false;
		private List<IGameComponent> _gameComponentsToInitialize = new List<IGameComponent>();
		private bool _wasResizeable;

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

			//Create a window
			_mainWindow = new NSWindow (frame, NSWindowStyle.Titled | NSWindowStyle.Closable, NSBackingStore.Buffered, true);

			// Perform any other window configuration you desire
			_mainWindow.IsOpaque = true;

			_view = new GameWindow (frame);
			_view.game = this;
			
			_mainWindow.ContentView.AddSubview (_view);
			_mainWindow.AcceptsMouseMovedEvents = false;

			// Initialize GameTime
			_updateGameTime = new GameTime ();
			_drawGameTime = new GameTime ();  
			
			//Set the current directory.
			// We set the current directory to the ResourcePath on Mac
			Directory.SetCurrentDirectory(NSBundle.MainBundle.ResourcePath);
		}

		void Handle_gameComponentCollectionComponentAdded (object sender, GameComponentCollectionEventArgs e)
		{

			if (!_initialized && !_initializing) {
				//Console.WriteLine("here");
				//e.GameComponent.Initialize();
			}
			else {
				e.GameComponent.Initialize();
				//_gameComponentsToInitialize.Add(e.GameComponent);
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
		
		// This method calls the game Initialize and BeginRun methods before it begins the game loop and starts 
		// processing events for the game.
		public void Run ()
		{			

			_lastUpdate = DateTime.Now;

			// In an original XNA game the GraphicsDevice property is null during initialization
			// but before the Game's Initialize method is called the property is available so we can
			// only assume that it should be created somewhere in here.  We can not set the viewport 
			// values correctly based on the Preferred settings which is causing some problems on some
			// Microsoft samples which we are not handling correctly.
			graphicsDeviceManager.CreateDevice();
			
			var manager = Services.GetService (typeof(IGraphicsDeviceManager)) as GraphicsDeviceManager;
			
			Microsoft.Xna.Framework.Graphics.Viewport _vp =
			new Microsoft.Xna.Framework.Graphics.Viewport();
				
			_vp.X = 0;
			_vp.Y = 0;
			_vp.Width = manager.PreferredBackBufferWidth;
			_vp.Height = manager.PreferredBackBufferHeight;
			
			GraphicsDevice.Viewport = _vp;

			_initializing = true;

			// Moving the GraphicsDevice creation to here also modifies when GameComponents are being
			// initialized.
			// Use OpenGL context locking in delegate function
//			InitialiseGameComponentsDelegate initD = new InitialiseGameComponentsDelegate (InitializeGameComponents);
//
//			// Invoke on thread from the pool
//			initD.BeginInvoke (
//				delegate (IAsyncResult iar) 
//				{
//					// We must have finished initialising, so set our flag appropriately
//					// So that we enter the Update loop
//					_initialized = true;
//					_initializing = false;
//				}, 
//			initD);
			
			InitializeGameComponents();
			_initialized = true;
			_initializing = false;
			
			
			Initialize ();

			_view.Run (FramesPerSecond / (FramesPerSecond * TargetElapsedTime.TotalSeconds));
			_mainWindow.MakeKeyAndOrderFront (_mainWindow);
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

		private GraphicsDeviceManager graphicsDeviceManager {
			get {
				if (this._graphicsDeviceManager == null) {
					this._graphicsDeviceManager = this.Services.GetService (typeof(IGraphicsDeviceManager)) as IGraphicsDeviceManager;
					if (this._graphicsDeviceManager == null) {
						throw new InvalidOperationException ("No Graphics Device Manager");
					}
				}
				return (GraphicsDeviceManager)this._graphicsDeviceManager;
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
			RectangleF frame;
			RectangleF content;
			
			if (graphicsDeviceManager.IsFullScreen) {
				frame = NSScreen.MainScreen.Frame;
				content = NSScreen.MainScreen.Frame;
			} else {
				content = _view.Bounds;
				content.Width = Math.Min(
				                    graphicsDeviceManager.PreferredBackBufferWidth,
				                    NSScreen.MainScreen.VisibleFrame.Width);
				content.Height = Math.Min(
				                    graphicsDeviceManager.PreferredBackBufferHeight,
				                    NSScreen.MainScreen.VisibleFrame.Height-TitleBarHeight());
				
				frame = _mainWindow.Frame;
				frame.X = Math.Max(frame.X, NSScreen.MainScreen.VisibleFrame.X);
				frame.Y = Math.Max(frame.Y, NSScreen.MainScreen.VisibleFrame.Y);
				frame.Width = content.Width;
				frame.Height = content.Height + TitleBarHeight();
			}
			_mainWindow.SetFrame (frame, true);
			
			_view.Bounds = content;
			_view.Size = content.Size.ToSize();
				
		}

		internal void GoWindowed ()
		{
			
			//Changing window style forces a redraw. Some games
			//have fail-logic and toggle fullscreen in their draw function,
			//so temporarily become inactive so it won't execute.
			bool wasActive = IsActive;
			IsActive = false;
			
			//Changing window style resets the title. Save it.
			string oldTitle = _view.Title;
			
			NSMenu.MenuBarVisible = true;
			_mainWindow.StyleMask = NSWindowStyle.Titled | NSWindowStyle.Closable;
			if (_wasResizeable) _mainWindow.StyleMask |= NSWindowStyle.Resizable;
			_mainWindow.HidesOnDeactivate = false;
			
			ResetWindowBounds();
			
			if (oldTitle != null)
				_view.Title = oldTitle;
			
			IsActive = wasActive;
		}
		
		internal void GoFullScreen ()
		{
			bool wasActive = IsActive;
			IsActive = false;
			
			//Some games set fullscreen in their initialize function,
			//before we have sized the window and set it active.
			//Do that now, or else mouse tracking breaks.
			_mainWindow.MakeKeyAndOrderFront(_mainWindow);
			ResetWindowBounds();
			
			_wasResizeable = IsAllowUserResizing;
			
			string oldTitle = _view.Title;
			
			NSMenu.MenuBarVisible = false;
			_mainWindow.StyleMask = NSWindowStyle.Borderless;
			_mainWindow.HidesOnDeactivate = true;
			
			ResetWindowBounds();
			
			if (oldTitle != null)
				_view.Title = oldTitle;
			
			if (!IsMouseVisible) {
				NSCursor.Hide();
			}
			
			IsActive = wasActive;
		}
		
		protected virtual void Initialize ()
		{

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
				//foreach (IGameComponent gc in _gameComponentsToInitialize) {
//				foreach (IGameComponent gc in _gameComponentCollection) {
//					// We may be drawing on a secondary thread through the display link or timer thread.
//					// Add a mutex around to avoid the threads accessing the context simultaneously
//					_view.OpenGLContext.CGLContext.Lock ();
//
//					// set our current context
//					_view.MakeCurrent ();
//
//					gc.Initialize ();
//
//					// now unlock it
//					_view.OpenGLContext.CGLContext.Unlock ();
//					_gameComponentsToInitialize.Remove(gc);
//				}
				
				// Changed from foreach to for loop in case the GameComponents's Update method
				//   modifies the component collection.  With a foreach it causes an error:
				//  "Collection was modified; enumeration operation may not execute."
				//  .Net 4.0 I thought got around this but in Mono 2.10.2 we still get this error.
				for (int x = 0; x < _gameComponentCollection.Count; x++) {
					var gc = (GameComponent)_gameComponentCollection[x];
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
			if (_initialized) { // && !Guide.IsVisible ) {
				
				// Changed from foreach to for loop in case the GameComponents's Update method
				//   modifies the component collection.  With a foreach it causes an error:
				//  "Collection was modified; enumeration operation may not execute."
				//  .Net 4.0 I thought got around this but in Mono 2.10.2 we still get this error.
				for (int x = 0; x < _gameComponentCollection.Count; x++) {
					var gc = (GameComponent)_gameComponentCollection[x];
					if (gc.Enabled) {
						gc.Update (gameTime);
					}
				}

			} else {
				
				// TODO: We can probably take this out but will wait until the next round
				//  of code checking.
				//  This should have all been moved to the Run method for initialization
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

