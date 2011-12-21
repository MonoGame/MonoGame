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

namespace Microsoft.Xna.Framework
{
    abstract class GamePlatform : IDisposable
    {
        public static GamePlatform Create(Game game)
        {
            // FIXME: How do we want this factory method to operate?  ifdef?  Reflection?
#if IPHONE
            return new iOSGamePlatform(game);
#elif MONOMAC
            return new MacGamePlatform(game);
#elif WINDOWS
            return new WindowsGamePlatform(game);
#elif ANDROID
            return new AndroidGamePlatform(game);
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

        #region Public Properties

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            protected set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    if (_isActive)
                        Raise(Activated, EventArgs.Empty);
                    else
                        Raise(Deactivated, EventArgs.Empty);
                }
            }
        }

        public bool IsMouseVisible
        {
            get; set;
        }

        public abstract GameRunBehavior DefaultRunBehavior { get; }

        public Game Game
        {
            get; private set;
        }

#if ANDROID
        public AndroidGameWindow Window
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

        private void Raise<TEventArgs>(EventHandler<TEventArgs> handler, TEventArgs e)
            where TEventArgs : EventArgs
        {
            if (handler != null)
                handler(this, e);
        }

        protected void RaiseAsyncRunLoopEnded()
        {
            Raise(AsyncRunLoopEnded, EventArgs.Empty);
        }

        #endregion Events

        public virtual void BeforeInitialize()
        {
            IsActive = true;
        }

        public virtual bool BeforeRun()
        {
            return true;
        }

        /// <summary>
        /// When implemented in a subclass, ends the active run loop.
        /// </summary>
        public abstract void Exit();

        /// <summary>
        /// When implemented in a subclass, starts the run loop and blocks
        /// until it has ended.
        /// </summary>
        public abstract void RunLoop();

        /// <summary>
        /// When implemented in a subclass, starts the run loop and returns
        /// immediately.
        /// </summary>
        public abstract void StartRunLoop();

        public abstract bool BeforeUpdate(GameTime gameTime);
        public abstract bool BeforeDraw(GameTime gameTime);

        public abstract void EnterFullScreen();
        public abstract void ExitFullScreen();

        public virtual bool IsActiveChanging(bool value)
        {
            return value;
        }

        public virtual void IsActiveChanged() {}

        public virtual bool IsMouseVisibleChanging(bool value)
        {
            return value;
        }

        public virtual void IsMouseVisibleChanged() {}

        public virtual TimeSpan TargetElapsedTimeChanging(TimeSpan value)
        {
            return value;
        }

        public virtual void TargetElapsedTimeChanged() {}

        public virtual void EnterBackground() {}
        public virtual void EnterForeground() {}

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {}

        #endregion
    }
}

