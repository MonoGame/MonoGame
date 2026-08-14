// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework;

public partial class GraphicsDeviceManager
{
    partial void PlatformInitialize(PresentationParameters presentationParameters)
    {
        NativeGameWindow window = _game.Window as NativeGameWindow;
        if (window == null)
            return;

        MGP_WindowCreateInfo windowCreateInfo = default;

        FillWindowCreateInfo(presentationParameters, ref windowCreateInfo);

        window.CreateWindow(windowCreateInfo);
        presentationParameters.DeviceWindowHandle = window.Handle;
    }

    private static void FillWindowCreateInfo(PresentationParameters presentationParameters, ref MGP_WindowCreateInfo windowCreateInfo)
    {
        switch(presentationParameters.BackBufferFormat)
        {
            case SurfaceFormat.Alpha8:
                windowCreateInfo.RedSize = 0;
                windowCreateInfo.GreenSize = 0;
                windowCreateInfo.BlueSize = 0;
                windowCreateInfo.AlphaSize = 8;
                break;

            case SurfaceFormat.Bgr565:
                windowCreateInfo.RedSize = 5;
                windowCreateInfo.GreenSize = 6;
                windowCreateInfo.BlueSize = 5;
                windowCreateInfo.AlphaSize = 0;
                break;

            case SurfaceFormat.Bgra4444:
                windowCreateInfo.RedSize = 4;
                windowCreateInfo.GreenSize = 4;
                windowCreateInfo.BlueSize = 4;
                windowCreateInfo.AlphaSize = 4;
                break;

            case SurfaceFormat.Bgra5551:
                windowCreateInfo.RedSize = 5;
                windowCreateInfo.GreenSize = 5;
                windowCreateInfo.BlueSize = 5;
                windowCreateInfo.AlphaSize = 1;
                break;

            case SurfaceFormat.Bgr32:
            case SurfaceFormat.Bgr32SRgb:
                windowCreateInfo.RedSize = 8;
                windowCreateInfo.GreenSize = 8;
                windowCreateInfo.BlueSize = 8;
                windowCreateInfo.AlphaSize = 0;
                break;

            case SurfaceFormat.Bgra32:
            case SurfaceFormat.Bgra32SRgb:
            case SurfaceFormat.Color:
            case SurfaceFormat.ColorSRgb:
                windowCreateInfo.RedSize = 8;
                windowCreateInfo.GreenSize = 8;
                windowCreateInfo.BlueSize = 8;
                windowCreateInfo.AlphaSize = 8;
                break;

            case SurfaceFormat.Rgba1010102:
                windowCreateInfo.RedSize = 10;
                windowCreateInfo.GreenSize = 10;
                windowCreateInfo.BlueSize = 10;
                windowCreateInfo.AlphaSize = 2;
                break;

            default:
                throw new NotSupportedException();
        }

        windowCreateInfo.FramebufferSrgbCapable =
            presentationParameters.BackBufferFormat == SurfaceFormat.ColorSRgb ||
            presentationParameters.BackBufferFormat == SurfaceFormat.Bgr32SRgb ||
            presentationParameters.BackBufferFormat == SurfaceFormat.Bgra32SRgb ? 1 : 0;

        switch (presentationParameters.DepthStencilFormat)
        {
            case DepthFormat.None:
                windowCreateInfo.DepthSize = 0;
                windowCreateInfo.StencilSize = 0;
                break;
            case DepthFormat.Depth16:
                windowCreateInfo.DepthSize = 16;
                windowCreateInfo.StencilSize = 0;
                break;
            case DepthFormat.Depth24:
                windowCreateInfo.DepthSize = 24;
                windowCreateInfo.StencilSize = 0;
                break;
            case DepthFormat.Depth24Stencil8:
                windowCreateInfo.DepthSize = 24;
                windowCreateInfo.StencilSize = 8;
                break;
            default:
                throw new NotSupportedException();
        }

        if (presentationParameters.MultiSampleCount > 0)
        {
            windowCreateInfo.MultiSampleBuffers = 1;
            windowCreateInfo.MultiSampleSamples = presentationParameters.MultiSampleCount;
        }
    }
}
