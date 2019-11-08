// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework
{
    class GtkGamePlatform : GamePlatform
    {
        private readonly OpenALSoundController _soundControllerInstance;
        
        public GtkGamePlatform(Game game) : base(game)
        {
            Window = new GtkGameWindow(game);
        }

        public override GameRunBehavior DefaultRunBehavior => GameRunBehavior.Asynchronous;

        public override bool BeforeDraw(GameTime gameTime)
        {
            return true;
        }

        public override bool BeforeUpdate(GameTime gameTime)
        {
            return true;
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {

        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {

        }

        public override void EnterFullScreen()
        {

        }

        public override void Exit()
        {
            (Window as GtkGameWindow)?.Exit();
        }

        public override void ExitFullScreen()
        {

        }

        public override void RunLoop()
        {

        }

        public override void StartRunLoop()
        {
            (Window as GtkGameWindow)?.StartRunLoop();
        }
    }
}
