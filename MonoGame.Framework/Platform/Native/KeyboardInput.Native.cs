// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#nullable enable

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Input;

public static partial class KeyboardInput
{
    private static TaskCompletionSource<string>? _completion;

    private static Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
    {
        if (MGP.KeyboardInput_Show(title, description, defaultText, usePasswordMode ? (byte)1 : (byte)0) == 0)
        {
            throw new PlatformNotSupportedException();
        }

        Keyboard.Keys.Clear();
        TaskCompletionSource<string> completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        _completion = completion;
        return completion.Task;
    }

    private static void PlatformCancel(string result)
    {
        TaskCompletionSource<string>? completion = _completion;
        _completion = null;
        MGP.KeyboardInput_Cancel();
        completion?.TrySetResult(result);
    }

    internal static void PlatformUpdate()
    {
        if (_completion == null || MGP.KeyboardInput_GetResult(out nint result, out byte cancelled) == 0)
        {
            return;
        }

        string? text = cancelled != 0 ? null : Marshal.PtrToStringUTF8(result);
        MGP.KeyboardInput_Cancel();
        TaskCompletionSource<string> completion = _completion;
        _completion = null;
        completion.TrySetResult(text!);
    }

    internal static void PlatformDispose()
    {
        if (_completion != null)
        {
            PlatformCancel(null!);
        }
    }
}
