// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Foundation;
using UIKit;

namespace MonoGame.InteractiveTests.iOS
{
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        UIWindow _window;
        UINavigationController _navigationController;
        RootViewController _rootViewController;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            CreateHomePage_(null);
            return true;
        }

        /// <summary>
        /// Creates the home page and uses the continuation function to help the test screen
        /// continue running other tests as needed.
        /// </summary>
        private void CreateHomePage_(Action<RootViewController> newScreenFunc)
        {
            // This may be called any number of times as the user runs a test, goes
            // back to the home page etc. Ensure we cleanly remove/re-add Window/ViewController
            // upon re-entering the homepage.
            if (_window != null)
            {
                _window.RemoveFromSuperview();
                _window = null;
            }

            _window = new UIWindow(UIScreen.MainScreen.Bounds);
            if (_rootViewController != null)
            {
                _rootViewController.View?.RemoveFromSuperview();
                _rootViewController.RemoveFromParentViewController();
            }

            _rootViewController = new RootViewController(newScreenFunc);
            _navigationController = new UINavigationController(_rootViewController);
            _window.RootViewController = _navigationController;
            _rootViewController.Exiting += (s, nextFunc) => { CreateHomePage_(nextFunc); };
            _window.MakeKeyAndVisible();
        }
    }
}
