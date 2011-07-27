using System;
using System.Collections.Generic;
using System.Drawing;

using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Microsoft.Xna.Framework.GamerServices
{
	public static class MonoGameGamerServicesHelper
	{
		public static void ShowSigninSheet()
		{
			
			
			NSApplication NSApp = NSApplication.SharedApplication;
			NSWindow gameWindow = NSApp.MainWindow;
			SigninController controller = new SigninController();
			
			NSWindow window = controller.Window;
			
			//NSNib nib = new NSNib("MonoGamerSigninSheet", NSBundle.)
//			MonoGamerSigninSheetController controller = new MonoGamerSigninSheetController();
//			NSWindow window = controller.Window;
			
			NSApp.BeginSheet(window, gameWindow);
			NSApp.RunModalForWindow(window);
			// sheet is up here.....
			
			NSApp.EndSheet(window);
			NSApp.EndSheet(window);
			window.OrderOut(gameWindow);			
		}
	}
}

