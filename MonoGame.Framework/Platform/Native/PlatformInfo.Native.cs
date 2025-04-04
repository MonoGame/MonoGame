// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;

namespace MonoGame.Framework.Utilities;

public static partial class PlatformInfo
{
    static readonly MonoGamePlatform _platform = MGP.Platform_GetPlatform();
    static readonly GraphicsBackend _graphics = MGP.Platform_GetGraphicsBackend();

    private static MonoGamePlatform PlatformGetMonoGamePlatform()
    {
        return _platform;
    }

    private static GraphicsBackend PlatformGetGraphicsBackend()
    {
        return _graphics;
    }
}
