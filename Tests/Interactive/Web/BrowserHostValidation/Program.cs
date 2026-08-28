// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.Versioning;

namespace BrowserHostValidation;

[SupportedOSPlatform("browser")]
internal static class Program
{
    private static BrowserHostValidationGame? _game;

    private static void Main()
    {
        BrowserHostValidationReporter.ReportPhase(
            "entryPoint",
            "Managed entry point reached. Constructing the validation game.");

        _game = new BrowserHostValidationGame();
        _game.Run();
    }

    internal static bool Tick()
    {
        if (_game == null)
            return false;

        _game.Tick();
        return true;
    }
}
