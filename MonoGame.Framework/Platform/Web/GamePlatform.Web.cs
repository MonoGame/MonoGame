// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework;

partial class GamePlatform
{
    internal static GamePlatform PlatformCreate(Game game) => new WebGamePlatform(game);
}
