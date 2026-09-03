// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MonoGame.Runtime.Browser.WebGL2;

[SupportedOSPlatform("browser")]
internal static partial class WebInterop
{
    [JSImport("globalThis.MonoGameWebHost.stageAssetPackAsync")]
    internal static partial Task StageAssetPackAsync(string assetPackName);
}
