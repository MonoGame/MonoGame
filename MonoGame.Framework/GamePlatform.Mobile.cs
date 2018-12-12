// MonoGame - Copyright (C) The MonoGame Team
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
#if FORMS
            return new iOSFormsGamePlatform(game);
#else
            return new iOSGamePlatform(game);
#endif
#elif ANDROID
#if FORMS
            return new AndroidFormsGamePlatform(game);
#else
            return new AndroidGamePlatform(game);
#endif
#elif WINDOWS_PHONE81
            return new MetroGamePlatform(game);
#elif WEB
            return new WebGamePlatform(game);
#endif
        }
    }
}
