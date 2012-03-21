using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace Microsoft.Xna.Framework
{
    internal class XnaGameWindowImpl : GameWindow
    {
        internal AndroidGameWindow nativeWindow;

        public XnaGameWindowImpl(Activity context, Game game)
        {
            nativeWindow = new AndroidGameWindow(context, game);
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
            throw new NotImplementedException();
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
            throw new NotImplementedException();
        }

        public override bool AllowUserResizing
        {
            get { return false; }
            set { }
        }

        public override Rectangle ClientBounds
        {
            get { return nativeWindow.ClientBounds; }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return nativeWindow.CurrentOrientation; }
        }

        public override IntPtr Handle
        {
            get { return nativeWindow.Handle; }
        }

        public override string ScreenDeviceName
        {
            get { return string.Empty; }
        }

        protected override void SetTitle(string title)
        {
            nativeWindow.Title = title;
        }

        // FIXME: Implement SetSupportedOrientations
        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            Console.WriteLine("Android needs to implement GameWindow.SetSupportedOrientations!");
        }
    }
}
