// FIXME: Add appropriate license
using System;

namespace Microsoft.Xna.Framework
{
    abstract class GamePlatform : IDisposable
    {
        public static GamePlatform Create(Game game)
        {
            // FIXME: How do we want this factory method to operate?  ifdef?  Reflection?
            return new iOSGamePlatform(game);
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

        public bool IsActive
        {
            get; protected set;
        }

        public Game Game
        {
            get; private set;
        }

        public GameWindow Window
        {
            get; protected set;
        }

        #endregion

        #region Events

        public event EventHandler AsyncRunLoopEnded;

        protected void RaiseAsyncRunLoopEnded()
        {
            var handler = AsyncRunLoopEnded;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion Events

        public virtual void BeforeInitialize() {}

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

        public abstract void EnterForeground();
        public abstract void EnterBackground();

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

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {}

        #endregion
    }
}

