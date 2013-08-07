using Microsoft.SPOT;
using Microsoft.SPOT.Presentation.Media;

namespace Microsoft.Xna.Framework.Graphics
{
    public class GraphicsDevice
    {
        private GraphicsDeviceManager manager;

        internal Bitmap CurrentDraw
        {
            get;
            set;
        }

        public Viewport Viewport
        {
            get;
            private set;
        }

        internal GraphicsDevice(GraphicsDeviceManager manager)
        {
            this.manager = manager;
            Viewport = new Viewport(0, 0, Bitmap.MaxWidth, Bitmap.MaxHeight);
        }

        public void Clear(Color color)
        {
            CurrentDraw.DrawRectangle(color, 0, 0, 0, CurrentDraw.Width, CurrentDraw.Height, 0, 0, color, 0, 0, color, 0, 0, 1);
        }

        public void SetRenderTarget(RenderTarget2D target)
        {
            if (target == null)
            {
                CurrentDraw = manager.Game.Display;
            }
            else
            {
                CurrentDraw = target.Bitmap;
            }
        }
    }
}
