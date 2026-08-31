// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Runtime.Versioning;

namespace Microsoft.Xna.Framework;

[SupportedOSPlatform("browser")]
partial class TitleContainer
{
    static partial void PlatformInit()
    {
        Location = string.Empty;
    }

    static partial void PlatformCheckStreamPath(string name)
    {
        // Block remote absolute URIs so browser title storage stays limited to app-hosted content.
        if (!Uri.TryCreate(name, UriKind.Absolute, out Uri uri))
            return;

        if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            throw new ArgumentException("Invalid filename. TitleContainer.OpenStream does not accept remote absolute URIs.", nameof(name));
    }

    private static Stream PlatformOpenStream(string safeName)
    {
        return BrowserAssetCache.OpenReadStream(safeName);
    }

    private static Stream PlatformOpenWriteStream(string safeName)
    {
        throw new NotImplementedException();
    }
}
