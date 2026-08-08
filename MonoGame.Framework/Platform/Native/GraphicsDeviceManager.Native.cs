// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if OPENGL
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Interop;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework;

public partial class GraphicsDeviceManager
{
    partial void PlatformInitialize(PresentationParameters presentationParameters)
    {
        NativeGameWindow window = _game.Window as NativeGameWindow;
        if (window == null)
            return;

        MGP_OpenGLWindowCreateInfo openGLCreateInfo = default;

        SurfaceFormat backBufferFormat = presentationParameters.BackBufferFormat;
        ColorFormat surfaceFormat = backBufferFormat.GetColorFormat();

        openGLCreateInfo.RedSize = surfaceFormat.R;
        openGLCreateInfo.GreenSize = surfaceFormat.G;
        openGLCreateInfo.BlueSize = surfaceFormat.B;
        openGLCreateInfo.AlphaSize = surfaceFormat.A;
        openGLCreateInfo.FramebufferSrgbCapable =
            backBufferFormat == SurfaceFormat.ColorSRgb ||
            backBufferFormat == SurfaceFormat.Bgr32SRgb ||
            backBufferFormat == SurfaceFormat.Bgra32SRgb ? 1 : 0;

        switch (presentationParameters.DepthStencilFormat)
        {
            case DepthFormat.None:
                openGLCreateInfo.DepthSize = 0;
                openGLCreateInfo.StencilSize = 0;
                break;
            case DepthFormat.Depth16:
                openGLCreateInfo.DepthSize = 16;
                openGLCreateInfo.StencilSize = 0;
                break;
            case DepthFormat.Depth24:
                openGLCreateInfo.DepthSize = 24;
                openGLCreateInfo.StencilSize = 0;
                break;
            case DepthFormat.Depth24Stencil8:
                openGLCreateInfo.DepthSize = 24;
                openGLCreateInfo.StencilSize = 8;
                break;
        }

        if (presentationParameters.MultiSampleCount > 0)
        {
            openGLCreateInfo.MultiSampleBuffers = 1;
            openGLCreateInfo.MultiSampleSamples = presentationParameters.MultiSampleCount;
        }

        window.CreateWindow(openGLCreateInfo);
        presentationParameters.DeviceWindowHandle = window.Handle;
    }
}
#endif
