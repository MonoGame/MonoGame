using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    public class SharedGraphicsDeviceManager : IGraphicsDeviceService, IDisposable
    {
        public GraphicsDevice GraphicsDevice
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<EventArgs> DeviceCreated;

        public event EventHandler<EventArgs> DeviceDisposing;

        public event EventHandler<EventArgs> DeviceReset;

        public event EventHandler<EventArgs> DeviceResetting;

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
