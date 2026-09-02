// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace BrowserHostValidation;

[SupportedOSPlatform("browser")]
internal static partial class BrowserHostValidationBootstrap
{
    [JSExport]
    public static bool Tick()
    {
        if (!Program.IsRunLoopStarted)
            return !Program.IsMainCompleted;

        if (!Program.Tick())
            return false;

        return !Program.IsMainCompleted;
    }
}
