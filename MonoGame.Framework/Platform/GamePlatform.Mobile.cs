// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework
{
    partial class GamePlatform
    {
        internal static GamePlatform PlatformCreate(Game game)
        {
#if IOS
            return new iOSGamePlatform(game);
#elif ANDROID
            return new AndroidGamePlatform(game);
#elif WINDOWS_PHONE81
            return new MetroGamePlatform(game);
#elif WEB
            return new WebGamePlatform(game);
#endif
        }
    }
}
