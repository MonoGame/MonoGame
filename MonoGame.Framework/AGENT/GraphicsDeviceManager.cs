using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public class GraphicsDeviceManager
    {
        internal Game Game
        {
            get;
            set;
        }

        public GraphicsDeviceManager(Game game)
        {
            Game = game;
            GraphicsDevice = new GraphicsDevice(this);
        }

        public GraphicsDevice GraphicsDevice
        {
            get;
            private set;
        }
    }
}
