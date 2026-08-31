// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Microsoft.Xna.Framework;

[SupportedOSPlatform("browser")]
internal static partial class WebInterop
{
    // TitleContainer is synchronous.  Because of this, we need to keep asset
    // retrieval in the browser/JS layer for now.
    // Trying to do this through a native MG_Asset implementation would require
    // browser runtime mode changes or a separate async-to-sync bridge.
    // This was the route with the least surface change to the current browser
    // runtime and native asset path.
    [JSImport("globalThis.MonoGameWebHost.tryGetContentBase64")]
    internal static partial string? TryGetContentBase64(string relativePath);
}
