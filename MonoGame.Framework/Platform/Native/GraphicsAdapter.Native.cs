// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.ObjectModel;

namespace Microsoft.Xna.Framework.Graphics;

partial class GraphicsAdapter
{
    private static void PlatformInitializeAdapters(out ReadOnlyCollection<GraphicsAdapter> adapters)
    {
        adapters = new ReadOnlyCollection<GraphicsAdapter>([]);
    }

    private bool PlatformIsProfileSupported(GraphicsProfile graphicsProfile)
    {
        return false;
    }
}
