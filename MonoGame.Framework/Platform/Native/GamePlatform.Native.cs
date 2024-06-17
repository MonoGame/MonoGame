// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework;

partial class GamePlatform
{
    internal static GamePlatform PlatformCreate(Game game) => new NativeGamePlatform(game);
}

class NativeGamePlatform : GamePlatform
{
    public NativeGamePlatform(Game game) : base(game)
    {
    }

    public override GameRunBehavior DefaultRunBehavior { get; }

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
        return false;
    }

    public override bool BeforeDraw(GameTime gameTime)
    {
        return false;
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
}
