using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Microsoft.Xna.Framework
{
    public interface IViewControllerHooks
    {
        void ViewDidAppear(bool animated);
        void ViewDidDisappear(bool animated);
    }
}