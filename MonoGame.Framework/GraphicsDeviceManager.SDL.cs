// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using OpenGL;

namespace Microsoft.Xna.Framework
{
    public partial class GraphicsDeviceManager
    {
        partial void PlatformInitialize(PresentationParameters presentationParameters)
        {
            var surfaceFormat = _game.graphicsDeviceManager.PreferredBackBufferFormat.GetColorFormat();
            var depthStencilFormat = _game.graphicsDeviceManager.PreferredDepthStencilFormat;
            var msCount = ClampMultisampleCount(presentationParameters);

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

            if (msCount > 0)
            {
                Sdl.GL.SetAttribute(Sdl.GL.Attribute.MultiSampleBuffers, 1);
                Sdl.GL.SetAttribute(Sdl.GL.Attribute.MultiSampleSamples, msCount);
            }

            Sdl.GL.SetAttribute(Sdl.GL.Attribute.DoubleBuffer, 1);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextMajorVersion, 2);
            Sdl.GL.SetAttribute(Sdl.GL.Attribute.ContextMinorVersion, 1);

            ((SdlGameWindow)SdlGameWindow.Instance).CreateWindow(presentationParameters);
        }

        private int ClampMultisampleCount(PresentationParameters pp)
        {
            if (pp.MultiSampleCount < 2)
            {
                pp.MultiSampleCount = 0;
                return 0;
            }

            var msCount = pp.MultiSampleCount;

            // Round down MultiSampleCount to the nearest power of two
            // hack from http://stackoverflow.com/a/2681094
            // Note: this will return an incorrect, but large value
            // for very large numbers. That doesn't matter because
            // the number will get clamped below anyway in this case.
            msCount = msCount | (msCount >> 1);
            msCount = msCount | (msCount >> 2);
            msCount = msCount | (msCount >> 4);
            msCount -= msCount >> 1;

            // we need a window to create a context to check for MS support, so create a dummy window
            using (GraphicsContext.CreateDummy())
            {
                int maxMsCount;
                GL.GetInteger(GetPName.MaxSamples, out maxMsCount);

                // on my machine the maxMsCount is reported to be 16, but creating a window
                // fails when ms count is set to 16. Can someone else please check to see
                // if they have the same issue and if the following line is acceptable
                maxMsCount >>= 1;
                if (maxMsCount < msCount)
                    msCount = maxMsCount;
            }

            pp.MultiSampleCount = msCount;
            return msCount;
        }
    }
}
