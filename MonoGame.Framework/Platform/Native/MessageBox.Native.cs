// MonoGame - Copyright (C) The MonoGame Team
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

        var button_bytes = new List<nint>();

        byte* _title = stackalloc byte[StringInterop.GetMaxSize(title)];
        StringInterop.CopyString(_title, title);
        byte* _description = stackalloc byte[StringInterop.GetMaxSize(description)];
        StringInterop.CopyString(_description, description);
        byte* _buttons = stackalloc byte[StringInterop.GetMaxSize(buttons)];
        StringInterop.CopyStrings(_buttons, buttons);

        int result = MGP.Window_ShowMessageBox(_window, _title, _description, _buttons, buttons.Count);

        _taskCompletionSource.SetResult(result);

        return _taskCompletionSource.Task;
    }

    private static void PlatformCancel(int? result)
    {
        if (_taskCompletionSource != null)
            _taskCompletionSource.SetResult(result);
    }
}
