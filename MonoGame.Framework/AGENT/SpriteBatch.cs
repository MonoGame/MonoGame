using System;
using System.Text;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.Xna.Framework.Graphics
{
    public class SpriteBatch
    {
        private GraphicsDevice graphicsDevice;
        private bool drawing;

        public SpriteBatch(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            drawing = false;
        }

        public void Begin()
        {
            drawing = true;
        }

        /*public void Draw(Texture2D texture, Rectangle destinationRectangle)
        {
            if (!drawing)
            {
                throw new InvalidOperationException("Cannot be called till after Begin is called");
            }
            graphicsDevice.CurrentDraw.StretchImage(destinationRectangle.X, destinationRectangle.Y, texture.Bitmap, destinationRectangle.Width, destinationRectangle.Height, 1);
        }*/

        public void Draw(Texture2D texture, Vector2 position)
        {
            if (!drawing)
            {
                throw new InvalidOperationException("Cannot be called till after Begin is called");
            }
            Draw(texture, position, new Rectangle(0, 0, texture.Width, texture.Height));
        }

        public void Draw(Texture2D texture, Vector2 position, Rectangle sourceRectangle)
        {
            if (!drawing)
            {
                throw new InvalidOperationException("Cannot be called till after Begin is called");
            }
            graphicsDevice.CurrentDraw.DrawImage((int)position.X, (int)position.Y, texture.Bitmap, sourceRectangle.X, sourceRectangle.Y, sourceRectangle.Width, sourceRectangle.Height);
        }

        /*public void Draw(Texture2D texture, Vector2 position, Rectangle sourceRectangle, float rotation)
        {
            if (!drawing)
            {
                throw new InvalidOperationException("Cannot be called till after Begin is called");
            }
            graphicsDevice.CurrentDraw.RotateImage((int)Math.Round(rotation), (int)position.X, (int)position.Y, texture.Bitmap, sourceRectangle.X, sourceRectangle.Y, sourceRectangle.Width, sourceRectangle.Height, 1);
        }*/

        public void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
        {
            if (!drawing)
            {
                throw new InvalidOperationException("Cannot be called till after Begin is called");
            }
            graphicsDevice.CurrentDraw.DrawText(text, spriteFont.Font, color, (int)position.X, (int)position.Y);
        }

        public void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
        {
            if (!drawing)
            {
                throw new InvalidOperationException("Cannot be called till after Begin is called");
            }
            graphicsDevice.CurrentDraw.DrawText(text.ToString(), spriteFont.Font, color, (int)position.X, (int)position.Y);
        }

        public void End()
        {
            drawing = false;
            graphicsDevice.CurrentDraw.Flush();
        }
    }
}
