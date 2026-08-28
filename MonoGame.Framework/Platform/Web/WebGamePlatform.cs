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
    private static unsafe MGG_GraphicsSystem* s_graphicsSystem;

    internal unsafe MGP_Platform* Handle;

    private readonly BrowserHostReadyInfo _readyInfo;
    private readonly WebGameWindow _window;

    public unsafe WebGamePlatform(Game game) : base(game)
    {
        GameRunBehavior behavior;
        Handle = MGP.Platform_Create(out behavior);
        if (Handle == null)
            throw new NoSuitableGraphicsDeviceException("Failed to initialize browser platform!");

        _readyInfo = WebHostRuntime.GetReadyInfo();
        _window = new WebGameWindow(this, _readyInfo, true);
        Window = _window;
        OnIsMouseVisibleChanged();
    }

    internal static unsafe MGG_GraphicsSystem* GraphicsSystem
    {
        get
        {
            if (s_graphicsSystem == null)
            {
                s_graphicsSystem = MGG.GraphicsSystem_Create();
                if (s_graphicsSystem == null)
                    throw new NoSuitableGraphicsDeviceException("Failed to initialize graphics system!");
            }

            return s_graphicsSystem;
        }
    }

    public override GameRunBehavior DefaultRunBehavior => GameRunBehavior.Asynchronous;

    internal override void OnPresentationChanged(PresentationParameters pp)
    {
        _window.OnPresentationChanged(pp);
    }

    public override void BeforeInitialize()
    {
        GraphicsDeviceManager graphicsDeviceManager = Game.graphicsDeviceManager;
        if (graphicsDeviceManager != null)
        {
            PresentationParameters pp = Game.GraphicsDevice.PresentationParameters;
            _window.OnPresentationChanged(pp);
        }

        base.BeforeInitialize();
        IsActive = _readyInfo.HasFocus && _readyInfo.IsPageVisible;
    }

    public override unsafe bool BeforeRun()
    {
        return WebHostRuntime.IsReady && MGP.Platform_BeforeRun(Handle) != 0;
    }

    public override void Exit()
    {
        WebHostRuntime.StopRunLoop();
        RaiseAsyncRunLoopEnded();
    }

    public override void RunLoop()
    {
        throw new InvalidOperationException("The browser platform requires a browser managed asynchronous run loop.");
    }

    public override unsafe void StartRunLoop()
    {
        _window.Show(true);
        WebHostRuntime.StartRunLoop();
        MGP.Platform_StartRunLoop(Handle);
    }

    public override unsafe bool BeforeUpdate(GameTime gameTime)
    {
        return MGP.Platform_BeforeUpdate(Handle) != 0;
    }

    public override unsafe bool BeforeDraw(GameTime gameTime)
    {
        return MGP.Platform_BeforeDraw(Handle) != 0;
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

    protected override unsafe void OnIsMouseVisibleChanged()
    {
        if (Handle != null)
            MGP.Mouse_SetVisible(Handle, (byte)(IsMouseVisible ? 1 : 0));
    }

    protected override unsafe void Dispose(bool disposing)
    {
        if (_window != null)
            _window.Destroy();

        if (s_graphicsSystem != null)
        {
            MGG.GraphicsSystem_Destroy(s_graphicsSystem);
            s_graphicsSystem = null;
        }

        if (Handle != null)
        {
            MGP.Platform_Destroy(Handle);
            Handle = null;
        }

        base.Dispose(disposing);
    }
}
