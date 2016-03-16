// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
#if WINRT
using System.Threading.Tasks;
#elif IOS
using Foundation;
using UIKit;
#elif MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.Foundation;
#else
using Foundation;
#endif
#endif
using Microsoft.Xna.Framework.Utilities;
using MonoGame.Utilities;

namespace Microsoft.Xna.Framework
{
    public static class TitleContainer
    {
        static TitleContainer() 
        {
#if WINDOWS || DESKTOPGL
#if DESKTOPGL
            // Check for the package Resources Folder first. This is where the assets
            // will be bundled.
            if (CurrentPlatform.OS == OS.MacOSX)
                Location = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "..", "Resources");
            if (!Directory.Exists (Location))
#endif
            Location = AppDomain.CurrentDomain.BaseDirectory;
#elif WINRT
            Location = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
#elif IOS || MONOMAC
			Location = NSBundle.MainBundle.ResourcePath;
#else
            Location = string.Empty;
#endif

#if IOS
            SupportRetina = UIScreen.MainScreen.Scale >= 2.0f;
            RetinaScale = (int)Math.Round(UIScreen.MainScreen.Scale);
#endif
        }

        static internal string Location { get; private set; }
#if IOS
        static internal bool SupportRetina { get; private set; }
        static internal int RetinaScale { get; private set; }
#endif

#if WINRT

        private static async Task<Stream> OpenStreamAsync(string name)
        {
            var package = Windows.ApplicationModel.Package.Current;

            try
            {
                var storageFile = await package.InstalledLocation.GetFileAsync(name);
                var randomAccessStream = await storageFile.OpenReadAsync();
                return randomAccessStream.AsStreamForRead();
            }
            catch (IOException)
            {
                // The file must not exist... return a null stream.
                return null;
            }
        }

#endif // WINRT

        /// <summary>
        /// Returns an open stream to an exsiting file in the title storage area.
        /// </summary>
        /// <param name="name">The filepath relative to the title storage area.</param>
        /// <returns>A open stream or null if the file is not found.</returns>
        public static Stream OpenStream(string name)
        {
            // Normalize the file path.
            var safeName = GetFilename(name);

            // We do not accept absolute paths here.
            if (Path.IsPathRooted(safeName))
                throw new ArgumentException("Invalid filename. TitleContainer.OpenStream requires a relative path.");

#if WINRT
            var stream = Task.Run( () => OpenStreamAsync(safeName).Result ).Result;
            if (stream == null)
                throw new FileNotFoundException(name);

            return stream;
#elif ANDROID
            return Android.App.Application.Context.Assets.Open(safeName);
#elif IOS
            var absolutePath = Path.Combine(Location, safeName);
            if (SupportRetina)
            {
                for (var scale = RetinaScale; scale >= 2; scale--)
                {
                    // Insert the @#x immediately prior to the extension. If this file exists
                    // and we are on a Retina device, return this file instead.
                    var absolutePathX = Path.Combine(Path.GetDirectoryName(absolutePath),
                                                      Path.GetFileNameWithoutExtension(absolutePath)
                                                      + "@" + scale + "x" + Path.GetExtension(absolutePath));
                    if (File.Exists(absolutePathX))
                        return File.OpenRead(absolutePathX);
                }
            }
            return File.OpenRead(absolutePath);
#else
            var absolutePath = Path.Combine(Location, safeName);
            return File.OpenRead(absolutePath);
#endif
        }

        // TODO: This is just path normalization.  Remove this
        // and replace it with a proper utility function.  I'm sure
        // this same logic is duplicated all over the code base.
        internal static string GetFilename(string name)
        {
            return FileHelpers.NormalizeFilePathSeparators(new Uri("file:///" + name).LocalPath.Substring(1));
        }
    }
}

