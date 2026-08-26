// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Microsoft.Xna.Framework;

namespace BrowserHostValidation;

[SupportedOSPlatform("browser")]
internal static partial class BrowserHostValidationBootstrap
{
    [JSExport]
    public static void InitializeHost(string startupInfoJson)
    {
        BrowserHostReadyInfo readyInfo = BrowserHostValidationStartupParser.ParseReadyInfo(startupInfoJson);
        WebHostRuntime.Initialize(readyInfo);
    }

    [JSExport]
    public static bool Tick()
    {
        if (!WebHostRuntime.IsRunLoopActive)
            return false;

        Game game = Game.Instance;
        if (game == null)
            return false;

        game.Tick();
        return WebHostRuntime.IsRunLoopActive;
    }
}
