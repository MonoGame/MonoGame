// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Input;

public static partial class MessageBox
{
    private static Task<int?> PlatformShow(string title, string description, List<string> buttons)
    {
        return Task.FromResult<int?>(0);
    }

    private static void PlatformCancel(int? result)
    {

    }
}
