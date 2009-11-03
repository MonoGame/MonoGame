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
    
using XnaTouch.Framework.Content;
using System;
using XnaTouch.Framework.Graphics;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreFoundation;
using MonoTouch.UIKit;
using XnaTouch.Framework.Input;
using System.Threading;
using OpenTK.Graphics;

namespace XnaTouch.Framework
{
    public class Game : IDisposable
    {
        private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;
		private TimeSpan _timeSinceLast;
        private bool _initialized = false;
        private GameComponentCollection _gameComponentCollection;
        public GameServiceContainer _services;
        private ContentManager _content;
        private IphoneWindow _view;
		private bool _isFixedTimeStep = true;
        private TimeSpan _targetElapsedTime = TimeSpan.FromSeconds(1 / 60.0); // ~60 frames per second
        
		private NSTimer _animationTimer;
		private IGraphicsDeviceManager graphicsDeviceManager;
		private IGraphicsDeviceService graphicsDeviceService;
		private UIWindow _mainWindow;
		private IGraphicsContext _context;

		internal static bool _playingVideo = false;
		
		public Game()
        {           
			// Initialize collections
			_services = new GameServiceContainer();
			_gameComponentCollection = new GameComponentCollection();

			// Initialize OpenGL funcionts
			_context = OpenTK.Platform.Utilities.CreateGraphicsContext(MonoTouch.OpenGLES.EAGLRenderingAPI.OpenGLES1);
			//Create a full-screen window
			_mainWindow = new UIWindow (UIScreen.MainScreen.Bounds);
			_mainWindow.ExclusiveTouch = true;
			
			_view = new IphoneWindow ();
			_view.BackgroundColor = UIColor.Black;
			_view.Opaque = true;
			((IphoneWindow) _view).game = this;			
			_mainWindow.AddSubview (_view);	
					
			// Initialize GameTime
            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime();  	
		}
		
		public void Dispose ()
		{
			_animationTimer = null;
			_context.Dispose();
		}

		internal CAEAGLLayer Layer 
		{
			get 
			{
				return (CAEAGLLayer) _view.Layer;
			}
		}

        public bool IsActive
        {
            get
            {
                return true;
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
				throw new NotSupportedException();
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
					CreateTimer ();
				}
            }
        }

		private void CreateTimer ()
		{
			_animationTimer = null;
			_animationTimer = NSTimer.CreateRepeatingScheduledTimer (_targetElapsedTime, () => DoStep ());
		}
		
        public void Run()
    		{			
			this.Initialize();
			
			_lastUpdate = DateTime.Now;
			if (IsFixedTimeStep) 
			{				
            		this.Update(_updateGameTime); 												
				CreateTimer();				
			}
			else {
				//Thread workerThread = new Thread(this.RunGame);
				//workerThread.Start();
				
				//RunGame();
				//CreateTimer();
			}
			
			//Show the window
			_mainWindow.MakeKeyAndVisible ();						
        }
		
		private void RunGame()
		{
			_animationTimer = null;
			// This is the heart of the game loop and will keep on looping until it is told otherwise
			while(true) 
			{	
				// I found this trick on iDevGames.com.  The command below pumps events which take place
				// such as screen touches etc so they are handled and then runs our code.  This means
				// that we are always in sync with VBL rather than an NSTimer and VBL being out of sync					
				//while(CFRunLoopRunInMode(kCFRunLoopDefaultMode, 0, TRUE) == kCFRunLoopRunHandledSource);		
				while(CFRunLoop.Current.RunInMode(CFRunLoop.ModeDefault,0.002f,true) == CFRunLoopExitReason.HandledSource);
				// Go and update the game logic and then render the scene;
				DoStep();	
			}
		}
		
		internal void DoStep()
		{
			// Update the game			
            DateTime now = DateTime.Now;
            if (IsFixedTimeStep)
            {
                _timeSinceLast += now - _lastUpdate;
                
				while (_timeSinceLast >= _targetElapsedTime)
                {
                    _updateGameTime.Update(_targetElapsedTime);
                    _timeSinceLast -= _targetElapsedTime;
                    Update(_updateGameTime);
                }
            }
            else
            {
                _updateGameTime.Update(now - _lastUpdate);
                Update(_updateGameTime);
            }

            // Draw the screen
			GraphicsDevice.StartPresentation();			
            _drawGameTime.Update(now - _lastUpdate);
            _lastUpdate = now;
            Draw(_drawGameTime);       			
			GraphicsDevice.Present();
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
		
		protected virtual void LoadContent()
		{
			// do nothing
		}
		
		protected virtual void UnLoadContent()
		{
			// do nothing
		}
		
        protected virtual void Initialize()
        {
			Accelerometer.SetupAccelerometer();
			
			this.graphicsDeviceManager = this.Services.GetService(typeof(IGraphicsDeviceManager)) as IGraphicsDeviceManager;			
			this.graphicsDeviceService = this.Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;			

			if ((this.graphicsDeviceService != null) && (this.graphicsDeviceService.GraphicsDevice != null))
            {
                LoadContent();
            }

            foreach (GameComponent gc in _gameComponentCollection)
            {
                gc.Initialize();
            }			
        }

        protected virtual void Update(GameTime gameTime)
        {			
			foreach (GameComponent gc in _gameComponentCollection)			
			{
				if (gc.Enabled)
                {
                    gc.Update(gameTime);
                }
            }
        }

        protected virtual void Draw(GameTime gameTime)
        {
			if (!_playingVideo) 
			{
	            foreach (GameComponent gc in _gameComponentCollection)
	            {
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

        public void Exit()
        {
			_animationTimer.Dispose();
			_animationTimer = null;
			
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
    }
}

