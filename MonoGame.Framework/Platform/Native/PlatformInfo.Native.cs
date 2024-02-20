// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.Framework.Utilities;

public static partial class PlatformInfo
{
    private static MonoGamePlatform PlatformGetMonoGamePlatform()
    {
        return MonoGamePlatform.DesktopGL;
    }

    private static GraphicsBackend PlatformGetGraphicsBackend()
    {
        return GraphicsBackend.OpenGL;
    }
}
