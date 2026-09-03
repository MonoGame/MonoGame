// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Runtime.Browser.WebGL2;

namespace BrowserHostValidation;

[SupportedOSPlatform("browser")]
internal static class Program
{
    private const string ValidationAssetPackName = "validation-texture";

    private static BrowserHostValidationGame? _game;
    private static readonly TaskCompletionSource<bool> s_runLoopCompletion =
        new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    private static bool s_mainCompleted;
    private static bool s_runLoopStarted;

    private static async Task Main()
    {
        try
        {
            BrowserHostValidationReporter.ReportPhase(
                "entryPoint",
                "Managed entry point reached. Constructing the validation game.");

            BrowserHostValidationReporter.ReportPhase(
                "assetPackStaging",
                "Staging the validation asset pack through the browser host.");
            await WebInterop.StageAssetPackAsync(ValidationAssetPackName);
            BrowserHostValidationReporter.ReportPhase(
                "assetPackStaged",
                "The validation asset pack was staged before its Content.Load calls.");

            _game = new BrowserHostValidationGame();
            _game.Exiting += OnGameExiting;
            _game.Run();
            s_runLoopStarted = true;
            await s_runLoopCompletion.Task;
        }
        catch (Exception exception)
        {
            BrowserHostValidationReporter.ReportPhase(
                "entryPointError",
                exception.ToString());
            throw;
        }
        finally
        {
            s_mainCompleted = true;
        }
    }

    internal static bool IsMainCompleted => s_mainCompleted;

    internal static bool IsRunLoopStarted => s_runLoopStarted;

    internal static bool Tick()
    {
        if (_game == null)
            return false;

        _game.Tick();
        return true;
    }

    private static void OnGameExiting(object? sender, ExitingEventArgs eventArgs)
    {
        s_runLoopCompletion.TrySetResult(true);
    }
}
