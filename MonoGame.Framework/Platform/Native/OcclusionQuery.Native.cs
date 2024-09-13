// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Graphics;

partial class OcclusionQuery
{
    internal unsafe MGG_OcclusionQuery* Handle;

    private unsafe void PlatformConstruct()
    {
        Handle = MGG.OcclusionQuery_Create(GraphicsDevice.Handle);
    }

    private unsafe void PlatformBegin()
    {
        MGG.OcclusionQuery_Begin(GraphicsDevice.Handle, Handle);
    }

    private unsafe void PlatformEnd()
    {
        MGG.OcclusionQuery_End(GraphicsDevice.Handle, Handle);
    }

    private unsafe bool PlatformGetResult(out int pixelCount)
    {
        return MGG.OcclusionQuery_GetResult(GraphicsDevice.Handle, Handle, out pixelCount);
    }

    protected unsafe override void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                MGG.OcclusionQuery_Destroy(GraphicsDevice.Handle, Handle);
                Handle = null;
            }
        }

        base.Dispose(disposing);
    }
}
