// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    internal partial interface IRenderTarget
    {
        int GLTexture { get; }
        TextureTarget GLTarget { get; }
        int GLColorBuffer { get; set; }
        int GLDepthBuffer { get; set; }
        int GLStencilBuffer { get; set; }
        int MultiSampleCount { get; }
        int LevelCount { get; }

        TextureTarget GetFramebufferTarget(RenderTargetBinding renderTargetBinding);
    }
}
