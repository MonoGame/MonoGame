using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework
{
    class WindowsGamePlatform : GamePlatform
    {
        private WindowsGameWindow _view;

        public WindowsGamePlatform(Game game)
            : base(game)
        {
            _view = new WindowsGameWindow();
            _view.Game = game;
            this.Window = _view;
            IsActive = true;
        }

        public override void RunLoop()
        {
            //Need to execute this on the rendering thread
            // FIXME: Is this needed?
            /*_view.OpenTkGameWindow.RenderFrame += delegate
            {
                if (!_devicesLoaded)
                {
                    Initialize();
                    _devicesLoaded = true;
                }
            };*/

            _view.OpenTkGameWindow.Run(1 / Game.TargetElapsedTime.TotalSeconds);
        }

        public override void StartRunLoop()
        {
            throw new NotImplementedException();
        }

        public override void Exit()
        {
            if (!_view.OpenTkGameWindow.IsExiting)
            {
                Net.NetworkSession.Exit();
                _view.OpenTkGameWindow.Exit();
            }
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
    }
}
