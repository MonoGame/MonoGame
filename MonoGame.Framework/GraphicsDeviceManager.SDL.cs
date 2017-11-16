// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework
{
    public partial class GraphicsDeviceManager
    {
        partial void PlatformInitialize(PresentationParameters presentationParameters)
        {
            int maxMsCount;
            using (GraphicsContext.CreateDummy())
            {
                GL.GetInteger(GetPName.MaxSamples, out maxMsCount);

                // reported ms count seems to be one POT larger than what's actually supported
                // TODO: verify this
                maxMsCount >>= 1;
            }
            GraphicsDevice.UpdateBackBufferPixelFormat(presentationParameters, maxMsCount);
           ((SdlGameWindow)SdlGameWindow.Instance).CreateWindow(presentationParameters);
        }
   }
}
