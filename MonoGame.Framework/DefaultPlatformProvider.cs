// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;


namespace Microsoft.Xna.Framework
{
  public class DefaultPlatformProvider : IPlatformProvider
  {
    public GamePlatform IPlatformProvider.CreatePlatform(Game game)
    {
      return GamePlatform.PlatformCreate(game);
    }
  }
}
