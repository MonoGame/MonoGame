// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

namespace Microsoft.Xna.Framework.Graphics;

public partial class Texture2D : Texture
{
    static partial void PlatformOpenFromFileStream(string path, ref Stream stream, ref bool handled)
    {
        handled = true;
        stream = TitleContainer.OpenStream(path);
    }
}
