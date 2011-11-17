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

using MonoTouch.CoreAnimation;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using MonoTouch.OpenGLES;
using MonoTouch.UIKit;

using OpenTK.Graphics;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using System.Linq;
using MonoTouch.CoreGraphics;

namespace Microsoft.Xna.Framework
{
    public class Game : IDisposable
    {
		private const float FramesPerSecond = 60.0f; // ~60 frames per second
		
        private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;
        private bool _initialized = false;
		private bool _initializing = false;
		private bool _isActive = true;
        private GameComponentCollection _gameComponentCollection;
        public GameServiceContainer _services;
        private ContentManager _content;
        internal GameWindow _view;
		private bool _isFixedTimeStep = true;
        private TimeSpan _targetElapsedTime = TimeSpan.FromSeconds(1 / FramesPerSecond); 
        
		private IGraphicsDeviceManager _graphicsDeviceManager;
		private IGraphicsDeviceService graphicsDeviceService;
		private UIWindow _mainWindow;

		internal static bool _playingVideo = false;
		private SpriteBatch spriteBatch;
		private Texture2D splashScreen;
		
		delegate void InitialiseGameComponentsDelegate();
		
		public Game()
        {           
			// Initialize collections
			_services = new GameServiceContainer();
			_gameComponentCollection = new GameComponentCollection();
			_gameComponentCollection.ComponentAdded += Handle_gameComponentCollectionComponentAdded;

			//Create a full-screen window
			_mainWindow = new UIWindow (UIScreen.MainScreen.Bounds);			
			_view = new GameWindow();
			GameWindow.game = this;			
			_mainWindow.Add(_view);							
					
			// Initialize GameTime
            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime();  	
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
		
		~Game()
		{
			UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications(); 
		}
		
		private void ObserveDeviceRotation ()
		{
			NSNotificationCenter.DefaultCenter.AddObserver( new NSString("UIDeviceOrientationDidChangeNotification"), (notification) => { 
			UIDeviceOrientation orientation = UIDevice.CurrentDevice.Orientation;
				
			// Calculate supported orientations if it has been left as "default"
			var gdm = (graphicsDeviceManager as GraphicsDeviceManager);
            DisplayOrientation supportedOrientations = gdm.SupportedOrientations;
			
			if (GraphicsDevice.PresentationParameters.BackBufferWidth != gdm.PreferredBackBufferWidth)
			{
				GraphicsDevice.PresentationParameters.BackBufferWidth = gdm.PreferredBackBufferWidth;
			}
			
			if (GraphicsDevice.PresentationParameters.BackBufferHeight != gdm.PreferredBackBufferHeight)
			{
				GraphicsDevice.PresentationParameters.BackBufferHeight = gdm.PreferredBackBufferHeight;

			}
				
            if ((supportedOrientations & DisplayOrientation.Default) != 0)
            {
                if (GraphicsDevice.PresentationParameters.BackBufferWidth > GraphicsDevice.PresentationParameters.BackBufferHeight)
                {
                    supportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
                }
                else
                {
                    supportedOrientations = DisplayOrientation.Portrait | DisplayOrientation.PortraitUpsideDown;
                }
            }
				
				switch (orientation)
				{
					case UIDeviceOrientation.Portrait :
						if ((supportedOrientations & DisplayOrientation.Portrait) != 0)
						{
							_view.CurrentOrientation = DisplayOrientation.Portrait;
							GraphicsDevice.PresentationParameters.DisplayOrientation = DisplayOrientation.Portrait;
							TouchPanel.DisplayOrientation = DisplayOrientation.Portrait;
						}
						break;
					case UIDeviceOrientation.LandscapeLeft :
						if ((supportedOrientations & DisplayOrientation.LandscapeLeft) != 0)
			            {
			            	_view.CurrentOrientation = DisplayOrientation.LandscapeLeft;
			             	GraphicsDevice.PresentationParameters.DisplayOrientation = DisplayOrientation.LandscapeLeft;
			              	TouchPanel.DisplayOrientation = DisplayOrientation.LandscapeLeft;							
			            }
						break;
					case UIDeviceOrientation.LandscapeRight :						
						if ((supportedOrientations & DisplayOrientation.LandscapeRight) != 0)
			            {
			            	_view.CurrentOrientation = DisplayOrientation.LandscapeRight;
			            	GraphicsDevice.PresentationParameters.DisplayOrientation = DisplayOrientation.LandscapeRight;
			            	TouchPanel.DisplayOrientation = DisplayOrientation.LandscapeRight;							
			            }
						break;
					case UIDeviceOrientation.FaceDown :
						if ((supportedOrientations & DisplayOrientation.FaceDown) != 0)
						{
							_view.CurrentOrientation = DisplayOrientation.FaceDown;
							GraphicsDevice.PresentationParameters.DisplayOrientation = DisplayOrientation.FaceDown;
							TouchPanel.DisplayOrientation = DisplayOrientation.FaceDown;							
						}
						break;
					case UIDeviceOrientation.FaceUp :
						if ((supportedOrientations & DisplayOrientation.FaceUp) != 0)						
						{
							_view.CurrentOrientation = DisplayOrientation.FaceUp;
							GraphicsDevice.PresentationParameters.DisplayOrientation = DisplayOrientation.FaceUp;
							TouchPanel.DisplayOrientation = DisplayOrientation.FaceUp;
						}
						break;
					case UIDeviceOrientation.PortraitUpsideDown :
						if ((supportedOrientations & DisplayOrientation.PortraitUpsideDown) != 0)						
						{
							_view.CurrentOrientation = DisplayOrientation.PortraitUpsideDown;
							GraphicsDevice.PresentationParameters.DisplayOrientation = DisplayOrientation.PortraitUpsideDown;
							TouchPanel.DisplayOrientation = DisplayOrientation.PortraitUpsideDown;
						}
						break;
					case UIDeviceOrientation.Unknown :
						if ((supportedOrientations & DisplayOrientation.Unknown) != 0)						
						{
							_view.CurrentOrientation = DisplayOrientation.Unknown;
							TouchPanel.DisplayOrientation = DisplayOrientation.Unknown;
						}
						break;						
					default:						
						break;
				}					  
			});
			
			UIDevice.CurrentDevice.BeginGeneratingDeviceOrientationNotifications();
		}
		
		public void Dispose ()
		{
			// do nothing
		}
		
        public bool IsActive
        {
            get
			{
				return _isActive;
			}
			protected set
			{
				if (_isActive != value )
				{
					_isActive = value;
				}
			}
        }

        public bool IsMouseVisible
        {
            get
			{
				return false;
			}
            set
			{
				// do nothing; ignore
			}
        }

        public TimeSpan TargetElapsedTime
        {
            get
            {
                return _targetElapsedTime;
            }
            set
            {
                _targetElapsedTime = value;			
				if(_initialized) {
					throw new NotSupportedException();
				}
            }
        }
		
        public void Run()
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
			
			_view.Run( FramesPerSecond / ( FramesPerSecond * TargetElapsedTime.TotalSeconds ) );	
			
			_view.MainContext = _view.EAGLContext;
			_view.ShareGroup = _view.MainContext.ShareGroup;
			_view.BackgroundContext = new MonoTouch.OpenGLES.EAGLContext(_view.ContextRenderingApi, _view.ShareGroup);
			
			//Show the window			
			_mainWindow.MakeKeyAndVisible();	
			
			Accelerometer.SetupAccelerometer();			
			Initialize();
			
			// Listen out for rotation changes
			ObserveDeviceRotation();
        }
		
		internal void DoUpdate(GameTime aGameTime)
		{
			if (_isActive)
			{
				Update(aGameTime);
			}
		}
		
		internal void DoDraw(GameTime aGameTime)
		{
			if (_isActive)
			{
				// Ok Based on these two messages the Draw and EndDraw should not be called
				// if BeginDraw returns false.
				// http://stackoverflow.com/questions/4054936/manual-control-over-when-to-redraw-the-screen/4057180#4057180
				// http://stackoverflow.com/questions/4235439/xna-3-1-to-4-0-requires-constant-redraw-or-will-display-a-purple-screen
				if (BeginDraw()) {
					Draw (aGameTime);
					EndDraw();
				}
			}
		}
		
		internal void DoStep()
		{
			var timeNow = DateTime.Now;
			
			// Update the game			
            _updateGameTime.Update(timeNow - _lastUpdate);
            Update(_updateGameTime);

            // Draw the screen
            _drawGameTime.Update(timeNow - _lastUpdate);
            _lastUpdate = timeNow;
            Draw(_drawGameTime);       			
		}

        public bool IsFixedTimeStep
        {
            get
			{
				return _isFixedTimeStep;
			}
            set
			{
				_isFixedTimeStep = value;
			}
        }

        public GameWindow Window
        {
            get
            {
                return _view;
            }
        }
		
		public void ResetElapsedTime()
        {
            _lastUpdate = DateTime.Now;
        }


        public GameServiceContainer Services
        {
            get
            {
                return _services;
            }
		}

        public ContentManager Content
        {
            get
            {
                if (_content == null)
                {
                    _content = new ContentManager(_services);
                }
                return _content;
            }
        }
		
		internal GraphicsDeviceManager graphicsDeviceManager {
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

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                if (this.graphicsDeviceService == null)
                {
                    this.graphicsDeviceService = this.Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;
                    if (this.graphicsDeviceService == null)
                    {
                        throw new InvalidOperationException("No Graphics Device Service");
                    }
                }
                return this.graphicsDeviceService.GraphicsDevice;
            }
        }
		
		public void EnterBackground()
    	{
			_isActive = false;
			 if (Deactivated != null)
                Deactivated.Invoke(this, null);
			
			_view.Pause();
		}
		
		public void EnterForeground()
    	{
			_isActive = true;			
			if (Activated != null)
                Activated.Invoke(this, null);
			
			_view.Resume();			
		}
		
		protected virtual bool BeginDraw()
		{
			return true;
		}
		
		protected virtual void EndDraw()
		{
			
		}
		
		protected virtual void LoadContent()
		{		
			var DefaultPath = DefaultPngFile;
			//string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Default*~iphone.png",SearchOption.TopDirectoryOnly);
			var model = UIDevice.CurrentDevice.Model;
				string suffix = "";
			bool isIpad = !model.ToLower().Contains("iphone");
			Console.WriteLine(DefaultPath);
			if (File.Exists(DefaultPath))
			{
				// Store the RootDir for later 
				string backup = Content.RootDirectory;

				try 
				{
					// Clear the RootDirectory for this operation
					Content.RootDirectory = string.Empty;

					spriteBatch = new SpriteBatch(GraphicsDevice);
					//splashScreen = Texture2D.FromFile(GraphicsDevice,DefaultPath);
					if(!isIpad && DefaultPath.Contains("Landscape") )
						splashScreen = Texture2D.FromImage(GraphicsDevice,Rotate(new UIImage(DefaultPath),90));
					else
						splashScreen = Texture2D.FromFile(GraphicsDevice,DefaultPath);
				}
				finally 
				{
					// Reset RootDir
					Content.RootDirectory = backup;
				}

			}
			else
			{
				spriteBatch = null;
				splashScreen = null;
			}
		}
		private string DefaultPngFile{
			get{
				
				var model = UIDevice.CurrentDevice.Model;
				string suffix = "";
				if ( !model.ToLower().Contains("iphone") )
				{
					string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Default*~ipad.png",SearchOption.TopDirectoryOnly);
					if(files.Length == 0)
						return "Default.png";
					if(files.Length == 1)
						return files[0];
					else
					//todo: switch it out by orientation
						return "Default~ipad.png";
				}
				else
				{
					string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Default*~iphone.png",SearchOption.TopDirectoryOnly);
					if(files.Length == 0)
						return "Default.png";
					//if(files.Length == 1)
					//	return files[0];
					//else
						return files.Where(x=> !x.Contains("@2x")).FirstOrDefault();
					return "Default~iphone.png";
				}
			}
		}
		
		UIImage Rotate(UIImage self,float degrees)
		{
			return self;
			// calculate the size of the rotated view's containing box for our drawing space
			UIView rotatedViewBox = new UIView(new System.Drawing.RectangleF(0,0,self.Size.Width * self.CurrentScale,self.Size.Height*self.CurrentScale));
			CGAffineTransform t = CGAffineTransform.MakeRotation(MathHelper.ToRadians(degrees));
			rotatedViewBox.Transform = t;
			var size = rotatedViewBox.Frame.Size;
			Console.WriteLine(size);
			rotatedViewBox.Dispose();
			// Create the bitmap context
			UIGraphics.BeginImageContext(size);
			CGContext bitmap = UIGraphics.GetCurrentContext();

			bitmap.TranslateCTM(size.Width/2,size.Height/2);

			bitmap.RotateCTM(MathHelper.ToRadians(degrees));

   			// Now, draw the rotated/scaled image into the context
			bitmap.ScaleCTM(1,-1);
			bitmap.DrawImage(new System.Drawing.RectangleF(-self.Size.Width*self.CurrentScale /2,-self.Size.Height*self.CurrentScale/2,self.Size.Width*self.CurrentScale,self.Size.Height*self.CurrentScale),self.CGImage);
   			UIImage newImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
   			return newImage;	
		}
		
		protected virtual void UnloadContent()
		{
			// do nothing
		}
		
        protected virtual void Initialize()
        {
			this.graphicsDeviceService = this.Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;			

			if ((this.graphicsDeviceService != null) && (this.graphicsDeviceService.GraphicsDevice != null))
            {
                LoadContent();
            }
        }
		
		private void InitializeGameComponents()
		{
			EAGLContext.SetCurrentContext(_view.BackgroundContext);
			
			for (int x = 0; x < _gameComponentCollection.Count; x++) 
			{
				var gc = (GameComponent)_gameComponentCollection[x];
				gc.Initialize();
			}
			
			EAGLContext.SetCurrentContext(_view.MainContext);
		}

        protected virtual void Update(GameTime gameTime)
        {			
			if ( _initialized  && !Guide.IsVisible )
			{				
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
			}
			else
			{
				if (!_initializing) 
				{
					_initializing = true;
					
					// Use OpenGLES context switching as described here
					// http://developer.apple.com/iphone/library/qa/qa2010/qa1612.html
					InitialiseGameComponentsDelegate initD = new InitialiseGameComponentsDelegate(InitializeGameComponents);

					// Invoke on thread from the pool
        			initD.BeginInvoke( 
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
		
        protected virtual void Draw(GameTime gameTime)
        {
			if ( _initializing )
			{
				if ( spriteBatch != null )
				{
					spriteBatch.Begin();
					
					// We need to turn this into a progress bar or animation to give better user feedback
					spriteBatch.Draw(splashScreen, new Vector2(0, 0), Color.White );
					spriteBatch.End();
				}
			}
			else
			{
				if (!_playingVideo) 
				{
					// Changed from foreach to for loop in case the GameComponents's Update method
					//   modifies the component collection.  With a foreach it causes an error:
					//  "Collection was modified; enumeration operation may not execute."
					//  .Net 4.0 I thought got around this but in Mono 2.10.2 we still get this error.
					for (int x = 0; x < _gameComponentCollection.Count; x++) 
					{
						var gc = (GameComponent)_gameComponentCollection[x];
						if (gc.Enabled && gc is DrawableGameComponent) 
						{
							DrawableGameComponent dc = gc as DrawableGameComponent;
							if (dc.Visible)
							{
								dc.Draw(gameTime);
							}
						}
					}
				}
			}
        }

        public void Exit()
        {
			//TODO: Fix this
			UIAlertView alert = new UIAlertView("Game Exit", "Hit Home Button to Exit",null,null,null);
			alert.Show();		
        }

        public GameComponentCollection Components
        {
            get
            {
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

