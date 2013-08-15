using System;
using Microsoft.SPOT;

namespace Microsoft.Xna.Framework.Graphics
{
    public class RenderTarget2D : Texture2D
    {
        public RenderTarget2D(GraphicsDevice graphicsDevice, int width, int height)
        {
            Bitmap = new Bitmap(width, height);
        }
    }
}
