using System;
using JSIL;

namespace Microsoft.Xna.Framework
{
    class WebGameWindow : GameWindow
    {
        private WebGamePlatform _platform;
        
        public WebGameWindow(WebGamePlatform platform)
        {
            _platform = platform;
            
            Builtins.Eval(@"
 /* TODO: Create WebGL element */           
");
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
        }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
        }

        protected override void SetTitle(string title)
        {
            Builtins.Eval("window.title = '" + title + "';");
        }

        public override bool AllowUserResizing
        {
            get
            {
                return false;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override Rectangle ClientBounds
        {
            get
            {
                // TODO: Get size of web page.
                return new Rectangle(0, 0, 600, 600);
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get
            {
                return DisplayOrientation.Default;
            }
        }

        public override IntPtr Handle
        {
            get
            {
                return IntPtr.Zero;
            }
        }

        public override string ScreenDeviceName
        {
            get
            {
                return string.Empty;
            }
        }
    }
}

