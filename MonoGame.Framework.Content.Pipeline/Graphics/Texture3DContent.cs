// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class Texture3DContent : TextureContent
    {
        public Texture3DContent() :
            base(new MipmapChainCollection(0, false))
        {
        }

        public override void Validate(GraphicsProfile? targetProf)
        {
            throw new NotImplementedException();
        }

        public override void GenerateMipmaps(bool overwriteExistingMipmaps)
        {
            throw new NotImplementedException();
        }
    }
}
