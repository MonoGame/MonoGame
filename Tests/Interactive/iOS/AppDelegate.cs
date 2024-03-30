// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MonoGame.InteractiveTests.iOS {
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate {

		UIWindow _window;
		UINavigationController _navigationController;
		RootViewController _rootViewController;
		
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			_window = new UIWindow (UIScreen.MainScreen.Bounds);

			_rootViewController = new RootViewController ();
			_navigationController = new UINavigationController (_rootViewController);
			_window.RootViewController = _navigationController;

			_window.MakeKeyAndVisible ();
			return true;
		}
	}
}

