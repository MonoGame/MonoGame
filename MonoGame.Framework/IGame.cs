using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public interface IGame : IDisposable
    {
        LaunchParameters LaunchParameters { get; }
        GameComponentCollection Components { get; }
        TimeSpan InactiveSleepTime { get; set; }
        bool IsActive { get; }
        bool IsMouseVisible { get; set; }
        TimeSpan TargetElapsedTime { get; set; }
        bool IsFixedTimeStep { get; set; }
        GameServiceContainer Services { get; }
        ContentManager Content { get; set; }
        GraphicsDevice GraphicsDevice { get; }

        //[CLSCompliant(false)]
        //GameWindow Window { get; }

        event EventHandler<EventArgs> Activated;
        event EventHandler<EventArgs> Deactivated;
        event EventHandler<EventArgs> Disposed;
        event EventHandler<EventArgs> Exiting;
        void Exit();
        void ResetElapsedTime();
        void SuppressDraw();
        //void RunOneFrame();
        //void Run();
        //void Run(GameRunBehavior runBehavior);
        //void Tick();
    }
}
