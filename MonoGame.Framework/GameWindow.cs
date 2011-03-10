using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework
{
    public abstract class GameWindow
    {
        public event EventHandler<EventArgs> ClientSizeChanged;
        public event EventHandler<EventArgs> OrientationChanged;
        public event EventHandler<EventArgs> ScreenDeviceNameChanged;

        public abstract void BeginScreenDeviceChange(bool willBeFullScreen);
        public abstract void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight);

        public void EndScreenDeviceChange(string screenDeviceName) { }

        protected void OnActivated(){}

        protected void OnClientSizeChanged()
        {
            if (ClientSizeChanged != null)
            {
                ClientSizeChanged(this, EventArgs.Empty);
            }
        }

        protected void OnDeactivated(){}

        protected void OnOrientationChanged()
        {
            if (OrientationChanged != null)
            {
                OrientationChanged(this, EventArgs.Empty);
            }
        }

        protected void OnPaint(){}

        protected void OnScreenDeviceNameChanged()
        {
            if (ScreenDeviceNameChanged != null)
            {
                ScreenDeviceNameChanged(this, EventArgs.Empty);
            }
        }
        
        public abstract bool AllowUserResizing { get; set; }
        public abstract Rectangle ClientBounds { get; }
        public abstract DisplayOrientation CurrentOrientation { get; internal set; }
        public abstract IntPtr Handle { get; }
        public abstract string ScreenDeviceName { get; }

        public string Title { get; set; }
        protected abstract void SetTitle(string title);
    }
}
