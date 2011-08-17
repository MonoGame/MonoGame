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
		private TimeSpan _targetElapsedTime; 
		private IGraphicsDeviceManager _graphicsDeviceManager;
		private IGraphicsDeviceService graphicsDeviceService;
		internal static bool _playingVideo = false;
		private SpriteBatch spriteBatch;
		private Texture2D splashScreen;
		private bool _mouseVisible = false;

		delegate void InitialiseGameComponentsDelegate ();

		#region Ctor
		
		public Game ()
		{
			// Initialize collections
			_services = new GameServiceContainer ();
			_gameComponentCollection = new GameComponentCollection ();
			
			_gameComponentCollection.ComponentAdded += Handle_gameComponentCollectionComponentAdded;

			_view = new GameWindow ();
			_view.Game = this;
			
			// default update rate
			_targetElapsedTime = TimeSpan.FromSeconds (1.0D / FramesPerSecond);
			
			// Initialize GameTime
			_updateGameTime = new GameTime ();
			_drawGameTime = new GameTime ();  

		}		

		~Game ()
		{
		}
		
		#endregion

		#region Private/Internal Properties
		
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
		
		#endregion
		
		#region Public Properties
		
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

				// TODO: implement this when available on opentk
				
			}
		}

		public TimeSpan TargetElapsedTime {
			get {
				return _targetElapsedTime;
			}
			set {
				_targetElapsedTime = value;			
				if (_initialized) {
					_view.Window.TargetUpdateFrequency = value.TotalSeconds;
				}
			}
		}
		
		public GameComponentCollection Components {
			get {
				return _gameComponentCollection;
			}
		}
		
		#endregion		
				
		#region Public Methods
		
		public void Dispose ()
		{
			_view.Dispose();
		}
		
		public void Exit ()
		{	 
			if (!_view.Window.IsExiting)
            {
                // raise the Exiting event
            	if (Exiting != null) Exiting(this, null);
                Net.NetworkSession.Exit();
				Audio.Sound.DisposeSoundServices();
                _view.Window.Exit();
            }			
		}

		public void ResetElapsedTime ()
		{
			_lastUpdate = DateTime.Now;
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

		public void Run ()
		{			
			_lastUpdate = DateTime.Now;

			Initialize ();			
			
            _view.Run(1.0f /  TargetElapsedTime.TotalSeconds);			
		}
		
		#endregion
		
		#region Private / Internal Method

		private void Handle_gameComponentCollectionComponentAdded (object sender, GameComponentCollectionEventArgs e)
		{
			if (!_initialized && !_initializing) {
				//_gameComponentsToInitialize.Add(e.GameComponent);
				e.GameComponent.Initialize();
			}
			else {
				e.GameComponent.Initialize();
				//_gameComponentsToInitialize.Add(e.GameComponent);
			}					
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

		private void ResetWindowBounds (bool toggleFullScreen)
		{
			Rectangle bounds;

			bounds = _view.ClientBounds;
			
			//Changing window style forces a redraw. Some games
			//have fail-logic and toggle fullscreen in their draw function,
			//so temporarily become inactive so it won't execute.
			
			bool wasActive = IsActive;
			IsActive = false;
			
			if (graphicsDeviceManager.IsFullScreen)
			{
				bounds = new Rectangle(0, 0,
				                       OpenTK.DisplayDevice.Default.Width,
						               OpenTK.DisplayDevice.Default.Height);
			}
			else
			{
				bounds.Width = graphicsDeviceManager.PreferredBackBufferWidth;
				bounds.Height = graphicsDeviceManager.PreferredBackBufferHeight;
			}
			
			if (toggleFullScreen)
				_view.ToggleFullScreen();
			
			// we only change window bounds if we are not fullscreen 
			if (!graphicsDeviceManager.IsFullScreen)
				_view.ChangeClientBounds(bounds);
			
			IsActive = wasActive;
		}
		
		internal void ResizeWindow(bool toggleFullScreen)
		{
			ResetWindowBounds(toggleFullScreen);	
		}
		
		protected virtual void Initialize ()
		{			
			this.graphicsDeviceService = this.Services.GetService (typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;			
			
			ResetWindowBounds(false);
			
			if ((this.graphicsDeviceService != null) && (this.graphicsDeviceService.GraphicsDevice != null)) {
				LoadContent ();
			}
		}

		private void InitializeGameComponents ()
		{
			foreach (GameComponent gc in _gameComponentCollection)
            {
                gc.Initialize();
            }			
		}

		protected virtual void Update (GameTime gameTime)
		{			
			if (_initialized  /* TODO && !Guide.IsVisible */) {
				
				
//				foreach (GameComponent gc in _gameComponentCollection) {
//					if (gc.Enabled) {
//						gc.Update (gameTime);
//					}
//				}
				
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
		#endregion

		#region Events
		public event EventHandler Activated;
		public event EventHandler Deactivated;
		public event EventHandler Disposed;
		public event EventHandler Exiting;
		#endregion
	}
}

