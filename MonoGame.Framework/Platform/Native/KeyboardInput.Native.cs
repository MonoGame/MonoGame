// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input;

public static partial class KeyboardInput
{
    private static Task<string> PlatformShow(string title, string description, string defaultText, bool usePasswordMode)
    {
        return Task.FromResult("");
    }

    private static void PlatformCancel(string result)
    {

    }
}
