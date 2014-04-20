using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;

namespace Microsoft.Xna.Framework
{
    class WebGamePlatform : GamePlatform
    {
        private WebGameWindow _window;

        public WebGamePlatform(Game game)
            : base(game)
        {
            _window = new WebGameWindow(this);
            
            Window = _window;
        }
        
        public override void Exit()
        {
        }

        public override void RunLoop()
        {
        }

        public override void StartRunLoop()
        {
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
                return GameRunBehavior.Synchronous;
            }
        }
    }
}

