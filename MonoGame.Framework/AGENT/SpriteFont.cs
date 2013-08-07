using System.Text;
using Microsoft.SPOT;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed class SpriteFont
    {
        internal Font Font
        {
            get;
            set;
        }

        internal SpriteFont()
        {
        }

        public Vector2 MeasureString(string text)
        {
            int height = 0;
            int width = 0;
            Font.ComputeExtent(text, out width, out height);
            return new Vector2(height, width);
        }

        public Vector2 MeasureString(StringBuilder text)
        {
            int height = 0;
            int width = 0;
            Font.ComputeExtent(text.ToString(), out width, out height);
            return new Vector2(height, width);
        }
    }
}
