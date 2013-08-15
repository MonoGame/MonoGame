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
            Bitmap bitmap = null;
            object data = ResourceUtility.GetObject(manager, name);
            string dataType = data.GetType().AssemblyQualifiedName;
            string bitmapType = typeof(Bitmap).AssemblyQualifiedName;
            if (dataType == bitmapType)
            {
                bitmap = (Bitmap)data;
            }
            else
            {
                try
                {
                    bitmap = new Bitmap((byte[])data, Bitmap.BitmapImageType.Bmp);
                }
                catch
                {
                    bitmap = new Bitmap((byte[])data, Bitmap.BitmapImageType.Jpeg);
                }
            }
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
