// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework;

partial class TitleContainer
{
    static partial void PlatformInit()
    {
        Location = AppContext.BaseDirectory;
    }

    static partial void PlatformCheckStreamPath(string name)
    {
        // Block remote absolute URIs so browser title storage stays limited to app-hosted content.
        if (!Uri.TryCreate(name, UriKind.Absolute, out Uri uri))
            return;

        if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            throw new ArgumentException("Invalid filename. TitleContainer.OpenStream does not accept remote absolute URIs.", nameof(name));
    }

    static partial void PlatformFetchContent(string name, ref bool handled, ref bool response)
    {
        if (ContentProvider == null)
            return;

        handled = true;
        response = ContentProvider.FetchContent(NormalizeRelativePath(name)).GetAwaiter().GetResult();
    }

    private static Stream PlatformOpenStream(string safeName)
    {
        if (ContentProvider != null)
        {
            try
            {
                return ContentProvider.OpenReadStream(safeName);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private static Stream PlatformOpenWriteStream(string safeName)
    {
        throw new NotImplementedException();
    }
}
