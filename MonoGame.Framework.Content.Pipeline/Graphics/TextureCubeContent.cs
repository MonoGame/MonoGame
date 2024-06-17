// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class TextureCubeContent : TextureContent
    {
        public TextureCubeContent() :
            base(new MipmapChainCollection(6, true))
        {
        }

        public override void Validate(GraphicsProfile? targetProf)
        {
            throw new NotImplementedException();
        }
    }
}
