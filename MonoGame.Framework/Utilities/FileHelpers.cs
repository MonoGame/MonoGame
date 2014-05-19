using System;
using System.IO;

namespace Microsoft.Xna.Framework.Utilities
{
	internal static class FileHelpers
	{
#if WINRT
		public static char notSeparator = '/';
		public static char separator = '\\';
#else
		public static char notSeparator = '\\';
		public static char separator = Path.DirectorySeparatorChar;
#endif

		public static string NormalizeFilePathSeparators(string name)
		{
            return name.Replace(notSeparator, separator);
		}

		public static void DirectoryCreate(string directory)
		{
			if (string.IsNullOrEmpty(directory))
				throw new ArgumentNullException("Parameter directory must contain a value.");

#if WINDOWS_STOREAPP
			var folder = ApplicationData.Current.LocalFolder;
			var task = folder.CreateFolderAsync(directory, CreationCollisionOption.OpenIfExists);
			task.AsTask().Wait();
#else
			Directory.CreateDirectory(directory);
#endif
		}
	}
}
