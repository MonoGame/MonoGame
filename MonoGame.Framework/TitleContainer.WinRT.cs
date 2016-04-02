// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {
        static partial void PlatformInit()
        {
            Location = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
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

        private static Stream PlatformOpenStream(string safeName)
        {
            return Task.Run(() => OpenStreamAsync(safeName).Result).Result;
        }
    }
}

