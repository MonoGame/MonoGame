// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input;


public static partial class MessageBox
{
    internal static unsafe MGP_Window* _window;

    private static TaskCompletionSource<int?> _taskCompletionSource;

    private static unsafe Task<int?> PlatformShow(string title, string description, List<string> buttons)
    {
        _taskCompletionSource = new TaskCompletionSource<int?>();

        string buttonsStr = string.Join("\0", buttons) + "\0";
        int result = MGP.Window_ShowMessageBox(_window, title, description, buttonsStr, buttons.Count);

        _taskCompletionSource.SetResult(result);
        return _taskCompletionSource.Task;
    }

    private static void PlatformCancel(int? result)
    {
        if (_taskCompletionSource != null)
            _taskCompletionSource.SetResult(result);
    }
}
