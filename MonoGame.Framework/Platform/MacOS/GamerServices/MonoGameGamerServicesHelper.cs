using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing;

using Foundation;
using AppKit;
using RectF = CoreGraphics.CGRect;
using PointF = CoreGraphics.CGPoint;

namespace Microsoft.Xna.Framework.GamerServices
{
	internal static class MonoGameGamerServicesHelper
	{
		internal static void ShowSigninSheet ()
		{

			NSApplication NSApp = NSApplication.SharedApplication;
			NSWindow gameWindow = NSApp.MainWindow;

			SigninController controller = new SigninController ();

			NSWindow window = controller.Window;

			// Something has happened with BeginSheet and needs to be looked into.
			// Until then just use modal for now.
			var frame = window.Frame;
			var location = new PointF (gameWindow.Frame.Bottom, gameWindow.Frame.Left);
			location = new PointF(gameWindow.Frame.Location.X, gameWindow.Frame.Location.Y);

			window.SetFrameOrigin(location);
			NSApp.BeginInvokeOnMainThread(delegate {
				Guide.isVisible = true;
//				NSApp.BeginSheet (window, gameWindow);
				NSApp.RunModalForWindow (window);
//				// sheet is up here.....
//
//				NSApp.EndSheet (window);
				window.OrderOut (gameWindow);
				Guide.isVisible = false;
//
			});
			//window.MakeKeyAndOrderFront(gameWindow);
//				SignedInGamer sig = new SignedInGamer();
//				sig.DisplayName = "MonoMac Gamer";
//				sig.Gamertag = "MonoMac Gamer";
//				sig.InternalIdentifier = Guid.NewGuid();
//
//				Gamer.SignedInGamers.Add(sig);
		}

		internal static List<MonoGameLocalGamerProfile> DeserializeProfiles ()
		{
			var path = StorageLocation;
			var fileName = Path.Combine(path, "LocalProfiles.dat");
			var profiles  = new List<MonoGameLocalGamerProfile> ();
			try {
				using (Stream stream = File.Open (fileName, FileMode.Open)) {
					BinaryFormatter bin = new BinaryFormatter ();

					profiles = (List<MonoGameLocalGamerProfile>)bin.Deserialize (stream);
				}
				
			} catch (IOException) {
			}	
			return profiles;
		}

		internal static void SerializeProfiles (List<MonoGameLocalGamerProfile> profiles)
		{
			var path = StorageLocation;
			var fileName = Path.Combine(path, "LocalProfiles.dat");
			try {
				using (Stream stream = File.Open (fileName, FileMode.Create)) {
					BinaryFormatter bin = new BinaryFormatter ();
					bin.Serialize (stream, profiles);
				}
			} catch (IOException) {
			}	
		}
		
		internal static string StorageLocation 
		{
			get {
				return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			}	
		}
	}
}

