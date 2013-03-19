using System;
using System.IO;

using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using MonoTouch.ImageIO;

namespace MonoGame.Tests {
	partial class FramePixelData {
		public static FramePixelData FromFile (string path)
		{
			// HACK: The path should have been resolved correctly
			//       before now!
			//var documentsDir = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			//path = Path.Combine(documentsDir, path);

			if (!File.Exists (path))
				throw new FileNotFoundException (path);

			var extension = Path.GetExtension (path).ToLowerInvariant ();
			CGImage image;
			switch (extension) {
			case ".png":
				image = CGImageFromPNGFile(path);
				break;
			case ".jpg":
			case ".jpeg":
				image = CGImageFromJPEGFile(path);
				break;
			default:
				throw new NotSupportedException (string.Format (
					"FramePixelData.FromFile cannot load images of type: {0}", extension));
			}

			try {
				using (var data = image.DataProvider.CopyData ()) {
					var pixels = new Color[data.Length / 4];

					for (int i = 0; i < pixels.Length; ++i) {
						int dataIndex = i * 4;
						pixels[i].R = data[dataIndex];
						pixels[i].G = data[dataIndex + 1];
						pixels[i].B = data[dataIndex + 2];
						pixels[i].A = data[dataIndex + 3];
					}

					return new FramePixelData(image.Width, image.Height, pixels);
				}
			} finally {
				image.Dispose ();
			}
		}

		public void Save (string path)
		{
			var extension = Path.GetExtension (path).ToLowerInvariant ();
			string uttype;
			switch (extension) {
			case ".png": uttype = "public.png"; break;
			case ".jpg":
			case ".jpeg": uttype = "public.jpeg"; break;
			default:
				throw new NotSupportedException(string.Format(
					"FramePixelData.Save cannot save images of type: {0}", extension));
			}

			//var documentsDir = Environment.GetFolderPath (Environment.SpecialFolder.Personal);

			var handle = GCHandle.Alloc(_data, GCHandleType.Pinned);
			try {
				var sizeOfColor = Marshal.SizeOf(typeof(Color));
				using (var dataProvider = new CGDataProvider(
					handle.AddrOfPinnedObject(), _data.Length * sizeOfColor, false))
				using (var colorSpace = CGColorSpace.CreateDeviceRGB())
				using (var image = new CGImage(
					Width, Height, 8, 32, Width * sizeOfColor, colorSpace,
					CGBitmapFlags.ByteOrder32Little | CGBitmapFlags.First,
					dataProvider, null, false, CGColorRenderingIntent.Default)) {

					//var fullPath = Path.Combine (documentsDir, path);
					Directory.CreateDirectory(Path.GetDirectoryName(path));

					var url = NSUrl.FromFilename (Path.GetFullPath(path));
					var destination = CGImageDestination.FromUrl(url, uttype, 1);
					destination.AddImage(image, null);
					if (!destination.Close())
						throw new Exception (string.Format (
							"Failed to write the image to '{0}'", path));
				}
			} finally {
				handle.Free ();
			}
		}

		private static CGImage CGImageFromPNGFile (string filename)
		{
			var provider = new CGDataProvider (filename);
			return CGImage.FromPNG (provider, null, false, CGColorRenderingIntent.AbsoluteColorimetric);
		}

		private static CGImage CGImageFromJPEGFile (string filename)
		{
			var provider = new CGDataProvider (filename);
			return CGImage.FromJPEG (provider, null, false, CGColorRenderingIntent.AbsoluteColorimetric);
		}
	}
}
