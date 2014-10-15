// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class Texture2DContent : TextureContent
    {
        public MipmapChain Mipmaps { get; set; }

        public Texture2DContent() : 
            base(new MipmapChainCollection())
        {
            Faces.Add(new MipmapChain());
        }

        public override void Validate(GraphicsProfile? targetProf)
        {
            throw new NotImplementedException();
        }
    }
}
