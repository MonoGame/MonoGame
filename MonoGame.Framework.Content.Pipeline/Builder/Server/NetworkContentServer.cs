// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Net;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder.Server;

/// <summary>
/// A <see cref="ContentServer"/> that is hosting a network.
/// </summary>
public class NetworkContentServer : ContentServer
{
    private HttpListener _httpListener = new();
    private Thread? _listeningThread;
    private bool isShutingDown;
    private object _waitLock = new();

    /// <summary>
    /// Server port to use for the listening for content requests in <see cref="ContentBuilderMode.Server"/> mode.
    /// </summary>
    /// <value>7771 by default.</value>
    [ContentServerParameter("port", "The port to be used for the content server mode.")]
    public ushort Port { get; init; } = 7771;

    /// <inheritdoc/>
    public override void NotifyContentRequestCompiled()
    {
        lock (_waitLock)
        {
            Monitor.Pulse(_waitLock);
        }
    }

    /// <inheritdoc/>
    public override void StartListening()
    {
        StopListening();

        Logger.Log($"Starting server on: http://localhost:{Port}/");
        if (!HttpListener.IsSupported)
        {
            Logger.Log("HttpListener is not supported on this system.");
            return;
        }

        _httpListener.Prefixes.Clear();
        _httpListener.Prefixes.Add($"http://*:{Port}/");
        _httpListener.Start();
        Logger.Log("Listening...");

        _listeningThread = new Thread(new ThreadStart(ServerThread));
        _listeningThread.Start();
    }

    /// <inheritdoc/>
    public override void StopListening()
    {
        if (_listeningThread == null || _httpListener == null)
        {
            return;
        }

        isShutingDown = true;

        while (_listeningThread.IsAlive)
        {
            _httpListener.Stop();
        }

        isShutingDown = false;
        _listeningThread = null;
    }

    private void ServerThread()
    {
        while (!isShutingDown)
        {
            try
            {
                RunServerCycle(_httpListener);
            }
            catch (Exception ex)
            {
                if (!isShutingDown)
                {
                    Logger.Log(LogLevel.Error, $"Error during a cycle: {ex}");
                }
            }
        }
    }

    private void RunServerCycle(HttpListener listener)
    {
        var context = listener.GetContext();
        var request = context.Request;
        var relativePath = request.Headers["path"] ?? "";

        ContentRequestedArgs args = new(relativePath);
        OnContentRequested(args);

        if (args.CompilationStarted)
        {
            lock (_waitLock)
            {
                Monitor.Wait(_waitLock);
            }
        }

        var response = context.Response;
        var fileExists = File.Exists(args.FilePath);
        var currentLastModifiedTime = fileExists ? File.GetLastWriteTimeUtc(args.FilePath).Ticks : 0;
        var sentLastModifiedTimeStr = request.Headers["LastModifiedTime"];

        try
        {
            if (fileExists && sentLastModifiedTimeStr != null)
            {
                var sentLastModifiedTime = long.Parse(sentLastModifiedTimeStr);
                if (sentLastModifiedTime == currentLastModifiedTime)
                {
                    fileExists = false;
                }
            }
        }
        catch
        {
            Logger.Log(LogLevel.Error, $"Sent last modified time is invalid so ignoring it: {sentLastModifiedTimeStr}");
        }

        if (!fileExists)
        {
            response.ContentLength64 = 1;
            response.OutputStream.Write([0], 0, 1);
            response.OutputStream.Close();
            return;
        }

        var lastModifiedTime = BitConverter.GetBytes(currentLastModifiedTime);
        var buffer = File.ReadAllBytes(args.FilePath);

        response.ContentLength64 = lastModifiedTime.Length + buffer.Length;
        response.OutputStream.Write(lastModifiedTime, 0, lastModifiedTime.Length);
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}

