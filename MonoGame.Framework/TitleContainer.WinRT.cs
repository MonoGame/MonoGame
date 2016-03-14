using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Utilities;
using System.IO;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
    public static partial class TitleContainer
    {
        static TitleContainer() 
        {
            Location = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
        }

        internal static Stream PlatformOpenStream (string safeName)
        {
            var stream = Task.Run( () => OpenStreamAsync(safeName).Result ).Result;
            if (stream == null)
                throw new FileNotFoundException(safeName);

            return stream;
        }

        internal static List<string> PlatformGetFiles (string directory, string filter)
        {
            return Task.Run(() => GetFilesAsync(directory).Result).Result;
        }

        private static async Task<List<string>> GetFilesAsync(string directory)
        {
            var package = Windows.ApplicationModel.Package.Current;
            var results = new List<string>();
            try {
                var folder = await package.InstalledLocation.GetFolderAsync(directory);
                if (folder != null)
                {
                    var files = await folder.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.DefaultQuery);
                    foreach (var f in files)
                    {
                        results.Add(Path.Combine (directory, f.Name));
                    }
                }
                return results;
            }
            catch (IOException)
            {
                // The file must not exist... return a null stream.
                return results;
            }
        }

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
    }
}

