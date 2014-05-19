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
	}
}
