#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2011 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement.
*/
#endregion License

using System;

#if WINRT
using Windows.UI.ViewManagement;
#endif

namespace Microsoft.Xna.Framework
{
    abstract class GamePlatform : IDisposable
    {
        #region
        protected TimeSpan _inactiveSleepTime = TimeSpan.FromMilliseconds(20.0);
        protected bool _needsToResetElapsedTime = false;
        #endregion

        #region Construction/Destruction
        public static GamePlatform Create(Game game)
        {
#if IPHONE
            return new iOSGamePlatform(game);
#elif MONOMAC
            return new MacGamePlatform(game);
#elif WINDOWS || LINUX
            return new OpenTKGamePlatform(game);
#elif ANDROID
            return new AndroidGamePlatform(game);
#elif PSS
			return new PSSGamePlatform(game);
#elif WINRT
            return new MetroGamePlatform(game);
#endif
        }

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
                    Raise(_isActive ? Activated : Deactivated, EventArgs.Empty);
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

#if WINRT
        private ApplicationViewState _viewState;
        public ApplicationViewState ViewState
        {
            get { return _viewState; }
            set
            {
                if (_viewState == value)
                    return;

                Raise(ViewStateChanged, new ViewStateChangedEventArgs(value));

                _viewState = value;
            }
        }
#endif

#if ANDROID
        public AndroidGameWindow Window
        {
            get; protected set;
        }
#elif PSS
		public PSSGameWindow Window
		{
			get; protected set;
		}
#else
        public GameWindow Window
        {
            get; protected set;
        }
#endif

        #endregion

        #region Events

        public event EventHandler<EventArgs> AsyncRunLoopEnded;
        public event EventHandler<EventArgs> Activated;
        public event EventHandler<EventArgs> Deactivated;

#if WINRT
        public event EventHandler<ViewStateChangedEventArgs> ViewStateChanged;
#endif

        private void Raise<TEventArgs>(EventHandler<TEventArgs> handler, TEventArgs e)
            where TEventArgs : EventArgs
        {
            if (handler != null)
                handler(this, e);
        }

        /// <summary>
        /// Raises the AsyncRunLoopEnded event.  This method must be called by
        /// derived classes when the asynchronous run loop they start has
        /// stopped running.
        /// </summary>
        protected void RaiseAsyncRunLoopEnded()
        {
            Raise(AsyncRunLoopEnded, EventArgs.Empty);
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
            if (this.Game.GraphicsDevice == null) 
            {
               var graphicsDeviceManager = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));			   
               graphicsDeviceManager.CreateDevice();
            }
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

        protected virtual void OnIsMouseVisibleChanged() {}

        #endregion Methods

        #region IDisposable implementation

        /// <summary>
        /// Performs application-defined tasks associated with freeing,
        /// releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {}
		
		/// <summary>
		/// Log the specified Message.
		/// </summary>
		/// <param name='Message'>
		/// 
		/// </param>
		[System.Diagnostics.Conditional("DEBUG")]
		public virtual void Log(string Message) {}		
			

        #endregion

        public virtual void Present() {}
    }
}

