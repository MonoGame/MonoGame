// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
namespace Microsoft.Xna.Framework.Graphics;

public abstract partial class Texture
{
    internal unsafe MGG_Texture* Handle;
    internal bool Owned = true;

    private unsafe void PlatformGraphicsDeviceResetting()
    {
        if (Handle != null && Owned)
        {
            MGG.Texture_Destroy(GraphicsDevice.Handle, Handle);
            Handle = null;
        }
    }

    protected unsafe override void Dispose(bool disposing)
    {
        PlatformGraphicsDeviceResetting();

        base.Dispose(disposing);
    }
}
