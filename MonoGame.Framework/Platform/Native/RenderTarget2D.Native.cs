// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
namespace Microsoft.Xna.Framework.Graphics;

public partial class RenderTarget2D
{
    private unsafe void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height, bool mipMap, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage, bool shared)
    {
        Handle = MGG.RenderTarget_Create(
            GraphicsDevice.Handle,
            TextureType._2D,
            _format,
            width,
            height,
            1,
            _levelCount,
            ArraySize,
            preferredDepthFormat,
            preferredMultiSampleCount,
            usage);
    }

    private unsafe void PlatformGraphicsDeviceResetting()
    {
        if (Handle != null && Owned)
        {
            MGG.Texture_Destroy(GraphicsDevice.Handle, Handle);
            Handle = null;
        }
    }

    private static unsafe RenderTarget2D PlatformFromNativeHandle(
        GraphicsDevice graphicsDevice,
        nint handle,
        int width,
        int height,
        SurfaceFormat format = SurfaceFormat.Color,
        DepthFormat preferredDepthFormat = DepthFormat.None,
        int preferredMultiSampleCount = 0)
    {
        // Call native layer to create an MGG_Texture that wraps the external resource.
        var nativeTexture = MGG.RenderTarget_WrapNativeHandle(
            graphicsDevice.Handle,
            handle,
            format,
            width,
            height,
            preferredDepthFormat,
            preferredMultiSampleCount);

        // Use the protected constructor that takes SurfaceType.SwapChainRenderTarget.
        // This skips the PlatformConstruct() call.
        var renderTarget = new RenderTarget2D(
            graphicsDevice,
            width,
            height,
            false,
            format,
            preferredDepthFormat,
            preferredMultiSampleCount,
            RenderTargetUsage.DiscardContents,
            SurfaceType.SwapChainRenderTarget);

        // Assign the handle to the native wrapper for the source image.
        // We set Owned = true because MonoGame still owns the MGG_Texture* wrapper (including its views and depth buffer)
        // and needs to manage its lifetime.
        // MGG_Texture_Destroy will skip freeing the external image memory.
        renderTarget.Handle = nativeTexture;
        renderTarget.Owned = true;

        return renderTarget;
    }
}
