// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework;

internal static class WebHostRuntime
{
    private const int CurrentHostVersion = 1;

    private static readonly object s_syncRoot = new object();

    private static BrowserHostReadyInfo s_readyInfo;
    private static bool s_runLoopActive;

    internal static bool IsReady
    {
        get
        {
            lock (s_syncRoot)
            {
                return s_readyInfo != null;
            }
        }
    }

    internal static bool IsRunLoopActive
    {
        get
        {
            lock (s_syncRoot)
            {
                return s_runLoopActive;
            }
        }
    }    

    internal static void Initialize(BrowserHostReadyInfo readyInfo)
    {
        if (readyInfo == null)
            throw new ArgumentNullException(nameof(readyInfo));

        if (readyInfo.HostVersion != CurrentHostVersion)
            throw new ArgumentException("The browser host version is not supported.", nameof(readyInfo));

        if (!string.Equals(readyInfo.GraphicsApi, "WebGL2", StringComparison.Ordinal))
            throw new ArgumentException("The browser host must provide a WebGL2 graphics API.", nameof(readyInfo));

        lock (s_syncRoot)
        {
            s_readyInfo = readyInfo;
            s_runLoopActive = false;
        }

        Debug.Assert(IsReady);
    }

    internal static BrowserHostReadyInfo GetReadyInfo()
    {
        lock (s_syncRoot)
        {
            if (s_readyInfo == null)
                throw new InvalidOperationException("The browser host runtime has not been initialized.");

            return s_readyInfo;
        }
    }

    internal static void StartRunLoop()
    {
        lock (s_syncRoot)
        {
            if (s_readyInfo == null)
                throw new InvalidOperationException("The browser host runtime has not been initialized.");

            s_runLoopActive = true;
        }
    }

    internal static void StopRunLoop()
    {
        lock (s_syncRoot)
        {
            s_runLoopActive = false;
        }
    }
}
