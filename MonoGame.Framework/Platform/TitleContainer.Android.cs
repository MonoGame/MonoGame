// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {
        private static Stream PlatformOpenStream(string safeName)
        {
            return Android.App.Application.Context.Assets.Open(safeName);
        }
    }
}

