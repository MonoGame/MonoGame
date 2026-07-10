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
#if DESKTOPGL || ANGLE
            return new SdlGamePlatform(game);
#elif WINDOWS && DIRECTX
            return new MonoGame.Framework.WinFormsGamePlatform(game);
#endif
        }
   }
}
