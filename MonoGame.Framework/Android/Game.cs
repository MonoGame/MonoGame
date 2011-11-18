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
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Util;
using Android.Views;
using OpenTK.Graphics;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework
{
    public class Game : IDisposable
    {
		private const float FramesPerSecond = 60.0f; // ~60 frames per second
		
        private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;
		private bool _isActive = false;
        private GameComponentCollection _gameComponentCollection;
        public GameServiceContainer _services;
        private ContentManager _content;
        internal AndroidGameWindow view;
		private bool _isFixedTimeStep = true;
        private TimeSpan _targetElapsedTime = TimeSpan.FromSeconds(1 / FramesPerSecond);
        bool disposed;

		internal IGraphicsDeviceManager graphicsDeviceManager;
        internal IGraphicsDeviceService graphicsDeviceService;
        private bool _devicesLoaded;

		internal static bool _playingVideo = false;
		
		delegate void InitialiseGameComponentsDelegate();

		internal static AndroidGameActivity contextInstance;

		public static AndroidGameActivity Activity
		{
			get
			{
				return contextInstance;
			}
			set
			{
				contextInstance = value;
			}
		}

		public Game()
		{
			System.Diagnostics.Debug.Assert(contextInstance != null, "Must set Game.Activity before creating the Game instance");
			contextInstance.Game = this;

			// Initialize collections
			_services = new GameServiceContainer();
			_gameComponentCollection = new GameComponentCollection();

			_content = new ContentManager(_services);

            view = new AndroidGameWindow(contextInstance);
		    view.game = this;						
			// Initialize GameTime
            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime();
		}
		
		~Game()
		{
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            // Tell the garbage collector not to call the finalizer
            // since all the cleanup will already be done.
            GC.SuppressFinalize(this);
        }

        // If disposing is true, it was called explicitly.
        // If disposing is false, it was called by the finalizer.
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                if (_content != null)
                {
                    _content.Dispose();
                    _content = null;
                }
                disposed = true;
            }
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
            }
        }
		
        public void Run()
    	{			
			_lastUpdate = DateTime.Now;
			
			// Get the Accelerometer going
			//TODO umcomment when the following bug is fixed 
			// http://bugzilla.xamarin.com/show_bug.cgi?id=1084
			// Accelerometer currently seems to have a memory leak
			//Accelerometer.SetupAccelerometer();
            view.Run(FramesPerSecond / (FramesPerSecond * TargetElapsedTime.TotalSeconds));	
        }
		
		internal void DoUpdate(GameTime aGameTime)
		{
            if (!_devicesLoaded)
            {
                Initialize();
                _devicesLoaded = true;
            }

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
                if (BeginDraw())
                {
                    Draw(aGameTime);
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

        public AndroidGameWindow Window
        {
            get
            {
                return view;
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
				return _content;
			}
			set
			{
				_content = value;
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
            if (_isActive)
            {
                _isActive = false;
                view.Pause();
                Accelerometer.Pause();
                if (Deactivated != null)
                    Deactivated.Invoke(this, null);
            }
		}
		
		public void EnterForeground()
    	{
            if (!_isActive)
            {
                _isActive = true;
                view.Resume();
                Accelerometer.Resume();
                if (Activated != null)
                    Activated.Invoke(this, null);
            }
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
            
		}
		
		protected virtual void UnloadContent()
		{
			// do nothing
		}
        
        protected virtual void Initialize()
        {
			this.graphicsDeviceManager = this.Services.GetService(typeof(IGraphicsDeviceManager)) as IGraphicsDeviceManager;			
			this.graphicsDeviceService = this.Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;			
			
			switch (Window.Context.Resources.Configuration.Orientation) {
				case Android.Content.Res.Orientation.Portrait :
					Window.SetOrientation(DisplayOrientation.Portrait);
					break;				
				case Android.Content.Res.Orientation.Landscape :
				    Window.SetOrientation(DisplayOrientation.LandscapeLeft);
					break;
				default:
				    Window.SetOrientation(DisplayOrientation.LandscapeLeft);
					break;
			} 

			foreach (GameComponent gc in _gameComponentCollection)
			{
				gc.Initialize();
			}
			
			if ((this.graphicsDeviceService != null) && (this.graphicsDeviceService.GraphicsDevice != null))
			{
				LoadContent();
			}
		}
		
#if xDEBUG
        private int garbageCounter = 0;
#endif

        protected virtual void Update(GameTime gameTime)
		{		
			for (int x = 0; x < _gameComponentCollection.Count; x++)			
			{
				var gc = (GameComponent)_gameComponentCollection[x]; 
				if (gc.Enabled)
				{
					gc.Update(gameTime);
				}
			}
#if xDEBUG
			garbageCounter++;
			if (garbageCounter > 200)
			{
				// force a Garbage Collection
				try
				{
					Android.Util.Log.Info("MonoGameInfo", String.Format("Game.Update Pre Collect {0}", GC.GetTotalMemory(true)));
					// GC.Collect(0);
					// Android.Util.Log.Info("MonoGameInfo", String.Format("Game.Update Pos Collect {0}", GC.GetTotalMemory(false)));
				}
				catch(Exception ex)
				{
					Android.Util.Log.Error("MonoGameInfo",String.Format("GC.Collect(0) {0}", ex.ToString()));
				}
				garbageCounter = 0;
			}
#endif
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
			//TODO: Fix this
			try
			{
				if (Exiting != null) Exiting(this, null);
				Net.NetworkSession.Exit();
                view.Close();
			}
			catch
			{
			}
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

