using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using MonoGame.Tests.Interface;

namespace MonoGame.Tests.iOS {
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate {

		private FileServer _fileServer;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			// FIXME: Figure out how to pass and receive arguments
			//        in MonoTouch applications.  The Main method
			//        has an empty array and NSProcessInfo has
			//        values specific to Mono launching/debugging.
			MobileInterface.RunAsync (new string [0]);

			_fileServer = new FileServer ();
			_fileServer.Prefixes.Add("http://+:6599/");
			_fileServer.Start ();
			return true;
		}
	}
}
