// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework;

internal sealed class WebGamePlatform : GamePlatform
{
    internal unsafe MGP_Platform* Handle;

    private readonly WebGameWindow _window;

    public unsafe WebGamePlatform(Game game) : base(game)
    {
        _window = new WebGameWindow(this, true);
        Window = _window;
        GamePad.Handle = Handle;
    }

    internal static unsafe MGG_GraphicsSystem* GraphicsSystem => throw new NotImplementedException();

    public override GameRunBehavior DefaultRunBehavior => GameRunBehavior.Asynchronous;

    public override void Exit()
    {
    }

    public override void RunLoop()
    {
        throw new NotImplementedException();
    }

    public override void StartRunLoop()
    {
        throw new NotImplementedException();
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

    public override void Present()
    {
        if (Game.GraphicsDevice != null)
            Game.GraphicsDevice.Present();
    }
}
