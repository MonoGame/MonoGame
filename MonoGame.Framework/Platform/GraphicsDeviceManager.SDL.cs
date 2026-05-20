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
            var surfaceFormat = _game.graphicsDeviceManager.PreferredBackBufferFormat.GetColorFormat();
            var depthStencilFormat = _game.graphicsDeviceManager.PreferredDepthStencilFormat;

            // TODO Need to get this data from the Presentation Parameters
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.RedSize, surfaceFormat.R);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.GreenSize, surfaceFormat.G);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.BlueSize, surfaceFormat.B);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.AlphaSize, surfaceFormat.A);

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

                // There should be an available SDL window handle created in SdlGameWindow on PlatformCreate().
                var sdlWindowHandle = SdlGameWindow.Instance.Handle;

                // If for some reason that code gets removed or the handle is cleared before this code is executed,
                // we should create a new temporary window to make a best-effort on grabbing GL_MAX_SAMPLES.
                var temporaryWindowHandle = IntPtr.Zero;
                if (sdlWindowHandle == IntPtr.Zero)
                {
                    temporaryWindowHandle = Sdl.Window.Create(
                        "glContextInfoWindow",
                        0,
                        0,
                        0,
                        0,
                        Sdl.Window.State.OpenGL |
                        Sdl.Window.State.Hidden
                    );
                    sdlWindowHandle = temporaryWindowHandle;
                }

                // If we do have an SDL window handle now, we can create a temporary GL context to grab GL_MAX_SAMPLES.
                // Otherwise, just use the previous value (which by default would be 32 if PreferMultiSampling is true).
                if (sdlWindowHandle != IntPtr.Zero)
                {
                    var temporaryGLContext = Sdl.GL.CreateContext(sdlWindowHandle);

                    var glMaxSamples = GL.GetMaxSamples();
                    multiSampleCount = Math.Min(glMaxSamples, multiSampleCount);

                    Sdl.GL.DeleteContext(temporaryGLContext);

                    if (temporaryWindowHandle != IntPtr.Zero)
                    {
                        // If for some reason the expected previously-created window was not available, and we
                        // created a new one in this method, then destroy the locally-created temporary window.
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

            ((SdlGameWindow)SdlGameWindow.Instance).CreateWindow();
        }
    }
}
