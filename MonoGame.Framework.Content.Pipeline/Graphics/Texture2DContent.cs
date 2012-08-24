using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class Texture2DContent : TextureContent
    {
        public MipmapChain Mipmaps { get; set; }

        public Texture2DContent() : 
            base(new MipmapChainCollection())
        {

        }

        public override void Validate(GraphicsProfile? targetProf)
        {
            throw new NotImplementedException();
        }


    }
}
