// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace MonoGame.InteractiveTests.TestUI
{
    ///<summary>Shows a label with some text + position.</summary>
    public class Label : View
    {
        public Label()
        {
            TextColor = Color.Black;
            Padding = new PaddingF(3);
        }

        public SpriteFont Font { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public PaddingF Padding { get; set; }

        public override Point SizeThatFits(Point size)
        {
            if (Font == null || string.IsNullOrWhiteSpace(Text))
                return Frame.Size;

            var sizeVector = Font.MeasureString(Text);
            return new Point((int)(sizeVector.X + Padding.Horizontal), (int)(sizeVector.Y + Padding.Vertical));
        }

        protected override void DrawForeground(DrawContext context, GameTime gameTime)
        {
            if (Font != null && !string.IsNullOrWhiteSpace(Text))
            {
                context.SpriteBatch.DrawString(
                    Font, Text, new Vector2(Padding.Left, Padding.Top), TextColor);
            }
        }
    }
}
