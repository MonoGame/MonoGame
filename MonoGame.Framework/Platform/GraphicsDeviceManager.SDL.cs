// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework
{
    public partial class GraphicsDeviceManager
    {
        partial void PlatformInitialize(PresentationParameters presentationParameters)
        {
            CreateOpenGLWindow(presentationParameters);
        }

        internal static void CreateOpenGLWindow(PresentationParameters presentationParameters)
        {
            var backBufferFormat = presentationParameters.BackBufferFormat;
            var surfaceFormat = backBufferFormat.GetColorFormat();
            var depthStencilFormat = presentationParameters.DepthStencilFormat;
            presentationParameters.MultiSampleCount = GraphicsDevice.NormalizeMultiSampleCount(presentationParameters.MultiSampleCount, 0);

            Sdl.GL.SetAttribute(Sdl.GL.Attribute.RedSize, surfaceFormat.R);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.GreenSize, surfaceFormat.G);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.BlueSize, surfaceFormat.B);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.AlphaSize, surfaceFormat.A);

            if (backBufferFormat == SurfaceFormat.ColorSRgb || backBufferFormat == SurfaceFormat.Bgr32SRgb || backBufferFormat == SurfaceFormat.Bgra32SRgb)
            {
                Sdl.GL.SetAttribute(Sdl.GL.Attribute.FramebufferSRGBCapable, 1);
            }

            switch (depthStencilFormat)
            {
                case DepthFormat.None:
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 0);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth16:
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 16);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth24:
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 24);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth24Stencil8:
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.DepthSize, 24);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.StencilSize, 8);
                    break;
            }

            Sdl.GL.SetAttribute(Sdl.GL.Attribute.DoubleBuffer, 1);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextMajorVersion, 2);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextMinorVersion, 1);

            if (presentationParameters.MultiSampleCount > 0)
            {
                // Store the "default" multisample count, which will be lowered if necessary to meet GL_MAX_SAMPLES.
                var multiSampleCount = presentationParameters.MultiSampleCount;

                // We should create a new temporary window to make a best-effort on grabbing GL_MAX_SAMPLES.
                var temporaryWindowHandle = Sdl.Window.Create(
                    "glContextInfoWindow",
                    0,
                    0,
                    0,
                    0,
                    Sdl.Window.State.OpenGL |
                    Sdl.Window.State.Hidden
                );

                // If we do have an SDL window handle now, we can create a temporary GL context to grab GL_MAX_SAMPLES.
                // Otherwise, just use the previous value (which by default would be 32 if PreferMultiSampling is true).
                if (temporaryWindowHandle != IntPtr.Zero)
                {
                    try
                    {
                        var temporaryGLContext = Sdl.GL.CreateContext(temporaryWindowHandle);

                        if (temporaryGLContext != IntPtr.Zero)
                        {
                            try
                            {
                                var glMaxSamples = GL.GetMaxSamples();
                                multiSampleCount = Math.Min(glMaxSamples, multiSampleCount);
                            }
                            finally
                            {
                                Sdl.GL.DeleteContext(temporaryGLContext);
                            }
                        }
                    }
                    finally
                    {
                        // We need to destroy the locally-created temporary window instead of leaking it.
                        Sdl.Window.Destroy(temporaryWindowHandle);
                    }
                }

                // Only set the multisampling attributes if GL_MAX_SAMPLES was above 0.
                if (multiSampleCount > 0)
                {
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.MultiSampleBuffers, 1);
                    Sdl.GL.SetAttribute(Sdl.GL.Attribute.MultiSampleSamples, multiSampleCount);
                }

                // Make sure that the accessible value reflects any reduction in sample count.
                presentationParameters.MultiSampleCount = multiSampleCount;
            }
            else
            {
                // Since SDL retains the GL Attributes between window creations
                // we need to clear the MSAA request for fallback
                Sdl.GL.SetAttribute(Sdl.GL.Attribute.MultiSampleBuffers, 0);
                Sdl.GL.SetAttribute(Sdl.GL.Attribute.MultiSampleSamples, 0);
            }

            SdlGameWindow window = (SdlGameWindow)SdlGameWindow.Instance;
            window.CreateWindow();

            // Calling CreateWindow above replaces the SDL window
            // so we need to update the handle for the presentation parameters.
            presentationParameters.DeviceWindowHandle = window.Handle;
        }
    }
}
