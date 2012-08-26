using Microsoft.Xna.Framework;

using MonoTouch.Foundation;
using MonoTouch.MessageUI;
using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework.GamerServices
{
    public static class WebBrowserTask
    {
        public static void Show(string url)
        {
            NSUrl nsurl = new NSUrl(url);
            UIApplication.SharedApplication.OpenUrl(nsurl);
        }
    }
}

