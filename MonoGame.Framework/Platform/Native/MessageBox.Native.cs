// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input;

public static partial class MessageBox
{
    internal static unsafe MGP_Window* _window;

    private static unsafe Task<int?> PlatformShow(string title, string description, List<string> buttons)
    {
        var result = MGP.Window_ShowMessageBox(_window, title, description, buttons.ToArray(), buttons.Count);
        return Task.FromResult<int?>(result);
    }

    private static void PlatformCancel(int? result)
    {
        // TODO: How should we do this?
    }
}
