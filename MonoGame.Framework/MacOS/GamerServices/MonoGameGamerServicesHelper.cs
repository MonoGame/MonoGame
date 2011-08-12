using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using MonoMac.Foundation;
using MonoMac.AppKit;

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
			
			NSApp.BeginInvokeOnMainThread(delegate {
				Guide.isVisible = true;
				NSApp.BeginSheet (window, gameWindow);
				NSApp.RunModalForWindow (window);
				// sheet is up here.....
	
				NSApp.EndSheet (window);
				window.OrderOut (gameWindow);
				Guide.isVisible = false;
				
			});
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

