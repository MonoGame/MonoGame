// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;

using JSIL;
using JSIL.Meta;

using MonoGame.Web;

namespace Microsoft.Xna.Framework
{
    using MonoGame.Web;

    public interface IHasCallback
    {
        void Callback();
    }

    class WebGamePlatform : GamePlatform, IHasCallback
    {
        private WebGameWindow _view;

        public WebGamePlatform(Game game)
            : base(game)
        {
            Window = new WebGameWindow(this);

            _view = (WebGameWindow)Window;
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
            ResetWindowBounds();
            _view.window.setInterval((Action)(() => {
                _view.ProcessEvents();
                Game.Tick();
            }), 25);
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
            ResetWindowBounds();
        }

        public override void ExitFullScreen()
        {
            ResetWindowBounds();
        }

        internal void ResetWindowBounds()
        {
            var graphicsDeviceManager = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));

            if (graphicsDeviceManager.IsFullScreen)
            {
                
            }
            else
            {
                _view.glcanvas.style.width = graphicsDeviceManager.PreferredBackBufferWidth + "px";
                _view.glcanvas.style.height = graphicsDeviceManager.PreferredBackBufferHeight + "px";
            }
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

