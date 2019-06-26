// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;


namespace Microsoft.Xna.Framework
{
    abstract partial class GamePlatform : IDisposable
    {
        #region Fields

        protected TimeSpan _inactiveSleepTime = TimeSpan.FromMilliseconds(20.0);
        protected bool _needsToResetElapsedTime = false;
        bool disposed;
        protected bool InFullScreenMode = false;
        protected bool IsDisposed { get { return disposed; } }

        #endregion

        #region Construction/Destruction

		protected GamePlatform(Game game)
        {
            if (game == null)
                throw new ArgumentNullException("game");
            Game = game;
        }

        ~GamePlatform()
        {
            Dispose(false);
        }

        #endregion Construction/Destruction

        #region Public Properties

        /// <summary>
        /// When implemented in a derived class, reports the default
        /// GameRunBehavior for this platform.
        /// </summary>
        public abstract GameRunBehavior DefaultRunBehavior { get; }

        /// <summary>
        /// Gets the Game instance that owns this GamePlatform instance.
        /// </summary>
        public Game Game
        {
            get; private set;
        }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            internal set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    EventHelpers.Raise(this, _isActive ? Activated : Deactivated, EventArgs.Empty);
                }
            }
        }

        private bool _isMouseVisible;
        public bool IsMouseVisible
        {
            get { return _isMouseVisible; }
            set
            {
                if (_isMouseVisible != value)
                {
                    _isMouseVisible = value;
                    OnIsMouseVisibleChanged();
                }
            }
        }

        private GameWindow _window;
        public GameWindow Window
        {
            get { return _window; }


            protected set
            {
                if (_window == null)
                {
                    Mouse.PrimaryWindow = value;
                    TouchPanel.PrimaryWindow = value;
                }

                _window = value;
            }
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs> AsyncRunLoopEnded;
        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;

        /// <summary>
        /// Raises the AsyncRunLoopEnded event.  This method must be called by
        /// derived classes when the asynchronous run loop they start has
        /// stopped running.
        /// </summary>
        protected void RaiseAsyncRunLoopEnded()
        {
            EventHelpers.Raise(this, AsyncRunLoopEnded, EventArgs.Empty);
        }

        #endregion Events

        #region Methods

        /// <summary>
        /// Gives derived classes an opportunity to do work before any
        /// components are initialized.  Note that the base implementation sets
        /// IsActive to true, so derived classes should either call the base
        /// implementation or set IsActive to true by their own means.
        /// </summary>
        public virtual void BeforeInitialize()
        {
            IsActive = true;
        }

        /// <summary>
        /// Gives derived classes an opportunity to do work just before the
        /// run loop is begun.  Implementations may also return false to prevent
        /// the run loop from starting.
        /// </summary>
        /// <returns></returns>
        public virtual bool BeforeRun()
        {
            return true;
        }

        /// <summary>
        /// When implemented in a derived, ends the active run loop.
        /// </summary>
        public abstract void Exit();

        /// <summary>
        /// When implemented in a derived, starts the run loop and blocks
        /// until it has ended.
        /// </summary>
        public abstract void RunLoop();

        /// <summary>
        /// When implemented in a derived, starts the run loop and returns
        /// immediately.
        /// </summary>
        public abstract void StartRunLoop();

        /// <summary>
        /// Gives derived classes an opportunity to do work just before Update
        /// is called for all IUpdatable components.  Returning false from this
        /// method will result in this round of Update calls being skipped.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public abstract bool BeforeUpdate(GameTime gameTime);

        /// <summary>
        /// Gives derived classes an opportunity to do work just before Draw
        /// is called for all IDrawable components.  Returning false from this
        /// method will result in this round of Draw calls being skipped.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public abstract bool BeforeDraw(GameTime gameTime);

        /// <summary>
        /// When implemented in a derived class, causes the game to enter
        /// full-screen mode.
        /// </summary>
        public abstract void EnterFullScreen();

        /// <summary>
        /// When implemented in a derived class, causes the game to exit
        /// full-screen mode.
        /// </summary>
        public abstract void ExitFullScreen();

        /// <summary>
        /// Gives derived classes an opportunity to modify
        /// Game.TargetElapsedTime before it is set.
        /// </summary>
        /// <param name="value">The proposed new value of TargetElapsedTime.</param>
        /// <returns>The new value of TargetElapsedTime that will be set.</returns>
        public virtual TimeSpan TargetElapsedTimeChanging(TimeSpan value)
        {
            return value;
        }
        /// <summary>
        /// Starts a device transition (windowed to full screen or vice versa).
        /// </summary>
        /// <param name='willBeFullScreen'>
        /// Specifies whether the device will be in full-screen mode upon completion of the change.
        /// </param>
        public abstract void BeginScreenDeviceChange (
                 bool willBeFullScreen
        );

        /// <summary>
        /// Completes a device transition.
        /// </summary>
        /// <param name='screenDeviceName'>
        /// Screen device name.
        /// </param>
        /// <param name='clientWidth'>
        /// The new width of the game's client window.
        /// </param>
        /// <param name='clientHeight'>
        /// The new height of the game's client window.
        /// </param>
        public abstract void EndScreenDeviceChange (
                 string screenDeviceName,
                 int clientWidth,
                 int clientHeight
        );

        /// <summary>
        /// Gives derived classes an opportunity to take action after
        /// Game.TargetElapsedTime has been set.
        /// </summary>
        public virtual void TargetElapsedTimeChanged() {}

        /// <summary>
        /// MSDN: Use this method if your game is recovering from a slow-running state, and ElapsedGameTime is too large to be useful.
        /// Frame timing is generally handled by the Game class, but some platforms still handle it elsewhere. Once all platforms
        /// rely on the Game class's functionality, this method and any overrides should be removed.
        /// </summary>
        public virtual void ResetElapsedTime() {}

        public virtual void Present() { }

        protected virtual void OnIsMouseVisibleChanged() {}

        /// <summary>
        /// Called by the GraphicsDeviceManager to notify the platform
        /// that the presentation parameters have changed.
        /// </summary>
        /// <param name="pp">The new presentation parameters.</param>
        internal virtual void OnPresentationChanged(PresentationParameters pp)
        {
        }

        #endregion Methods

        #region IDisposable implementation

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                Mouse.PrimaryWindow = null;
                TouchPanel.PrimaryWindow = null;

                disposed = true;
            }
        }
		
		/// <summary>
		/// Log the specified Message.
		/// </summary>
		/// <param name='Message'>
		/// 
		/// </param>
		[System.Diagnostics.Conditional("DEBUG")]
		public virtual void Log(string Message) {}		
			

        #endregion
    }
}

