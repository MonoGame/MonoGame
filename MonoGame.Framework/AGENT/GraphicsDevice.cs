using Microsoft.SPOT;

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

        public void Clear()
        {
            CurrentDraw.Clear();
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
