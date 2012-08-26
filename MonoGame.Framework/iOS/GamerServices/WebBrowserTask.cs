using System;

using Microsoft.Xna.Framework;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework.GamerServices
{
    public  class WebBrowserTask
    {
        #region Fields
        private NSUrl nsurl;
        #endregion

        #region Properties
        public Uri Uri
        {
            set
            {
                if (value.IsAbsoluteUri && (value.Scheme == Uri.UriSchemeHttp || value.Scheme == Uri.UriSchemeHttps))
                    this.nsurl = new NSUrl(value.OriginalString);
                else
                    throw new ArgumentOutOfRangeException("Uri", "This property supports only absolute URIs that use the HTTP or HTTPS protocol.");
            }
        }
        #endregion

        #region Methods
        public void Show()
        {
            if (this.nsurl != null)
                UIApplication.SharedApplication.OpenUrl(nsurl);
        }
        #endregion
    }
}

