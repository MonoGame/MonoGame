// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace MonoGame.Runtime.Browser.WebGL2;

/// <summary>
/// Runs a MonoGame <see cref="Game"/> with the browser's Emscripten frame scheduler.
/// </summary>
[SupportedOSPlatform("browser")]
public static partial class BrowserGame
{
    private static bool _firstFrameReported;
    private static Game? _game;
    private static TaskCompletionSource<bool>? _runCompletion;
    private static bool _mainLoopStarted;

    /// <summary>
    /// Runs <paramref name="game"/> until it exits, then disposes it.
    /// </summary>
    /// <param name="game">The game to run. This method takes ownership of the game instance.</param>
    /// <returns>A task that completes when the game exits.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Another browser game is already running.</exception>
    public static async Task RunAsync(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);

        if (_game != null)
        {
            throw new InvalidOperationException("A browser game is already running.");
        }

        TaskCompletionSource<bool> runCompletion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _firstFrameReported = false;
        _game = game;
        _runCompletion = runCompletion;

        try
        {
            game.Exiting += OnGameExiting;
            game.Run();
            StartMainLoop();
            await runCompletion.Task;
        }
        finally
        {
            game.Exiting -= OnGameExiting;
            StopMainLoop();
            _runCompletion = null;
            _game = null;
            game.Dispose();
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void TickFromEmscripten()
    {
        try
        {
            _game?.Tick();
            if (!_firstFrameReported)
            {
                _firstFrameReported = true;
                NotifyManagedFirstFrame();
            }
        }
        catch (Exception exception)
        {
            StopMainLoop();
            NotifyManagedFailure(exception.Message, exception.ToString());
            _runCompletion?.TrySetException(exception);
        }
    }

    [JSExport]
    internal static void NotifyContextLost()
    {
        StopMainLoop();
        _runCompletion?.TrySetException(new InvalidOperationException(
            "The WebGL2 context was lost. MonoGame.Web cannot recover this application; reload the page."));
    }

    private static void OnGameExiting(object? sender, ExitingEventArgs eventArgs)
    {
        StopMainLoop();
        _runCompletion?.TrySetResult(true);
    }

    private static unsafe void StartMainLoop()
    {
        _mainLoopStarted = true;
        EmscriptenSetMainLoop(&TickFromEmscripten, 0, 0);
    }

    private static void StopMainLoop()
    {
        if (!_mainLoopStarted)
        {
            return;
        }

        _mainLoopStarted = false;
        EmscriptenCancelMainLoop();
    }

    [JSImport("globalThis.MonoGameWebHost.notifyManagedFirstFrame")]
    private static partial void NotifyManagedFirstFrame();

    [JSImport("globalThis.MonoGameWebHost.notifyManagedFailure")]
    private static partial void NotifyManagedFailure(string message, string details);

    [DllImport("*", CallingConvention = CallingConvention.Cdecl, EntryPoint = "emscripten_cancel_main_loop")]
    private static extern void EmscriptenCancelMainLoop();

    [DllImport("*", CallingConvention = CallingConvention.Cdecl, EntryPoint = "emscripten_set_main_loop")]
    private static extern unsafe void EmscriptenSetMainLoop(
        delegate* unmanaged[Cdecl]<void> callback,
        int framesPerSecond,
        int simulateInfiniteLoop);
}
