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

    private static unsafe Task<int?> PlatformShow(string title, string description, List<string> buttons)
    {
        string buttonsStr = string.Join("\0", buttons) + "\0";
        int result = MGP.Window_ShowMessageBox(_window, title, description, buttonsStr, buttons.Count);
        return Task.FromResult<int?>(result);
    }

    private static void PlatformCancel(int? result)
    {
        // TODO: How should we do this?
    }
}
