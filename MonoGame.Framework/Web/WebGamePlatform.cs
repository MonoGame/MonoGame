using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;

namespace Microsoft.Xna.Framework
{
    using MonoGame.Web;

    public interface IHasCallback
    {
        void Callback();
    }

    class WebGamePlatform : GamePlatform, IHasCallback
    {
        private WebGameWindow _window;

        public WebGamePlatform(Game game)
            : base(game)
        {
            Window = new WebGameWindow(this);
        }

        public virtual void Callback()
        {
            this.Game.Tick();
        }
        
        public override void Exit()
        {
        }

        public override void RunLoop()
        {
            throw new InvalidOperationException("You can not run a synchronous loop on the web platform.");
        }

        public override void StartRunLoop()
        {
            JSAPIAccess.Instance.GamePlatformStartRunLoop(this);
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            return true;
        }

        public override bool BeforeDraw(GameTime gameTime)
        {
            return true;
        }

        public override void EnterFullScreen()
        {
        }

        public override void ExitFullScreen()
        {
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        public override GameRunBehavior DefaultRunBehavior
        {
            get
            {
                return GameRunBehavior.Asynchronous;
            }
        }
    }
}

