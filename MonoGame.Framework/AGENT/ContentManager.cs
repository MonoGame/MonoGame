using System;
using System.Resources;
using Microsoft.SPOT;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public class ContentManager
    {
        private ResourceManager manager;

        internal ContentManager(ResourceManager manager)
        {
            this.manager = manager;
        }

        public Texture2D LoadTexture(Enum name)
        {
            Bitmap bitmap = (Bitmap)ResourceUtility.GetObject(manager, name);
            Texture2D texture2D = new Texture2D();
            texture2D.Bitmap = bitmap;
            return texture2D;
        }

        public SpriteFont LoadSpriteFont(Enum name)
        {
            Font font = (Font)ResourceUtility.GetObject(manager, name);
            SpriteFont spriteFont = new SpriteFont();
            spriteFont.Font = font;
            return spriteFont;
        }
    }
}
