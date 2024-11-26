// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics;

// TODO: We should review the API and decide how useful
// it is considering Vulkan, DirectX12, and consoles.

public partial class GraphicsDebug
{
    private bool PlatformTryDequeueMessage(out GraphicsDebugMessage message)
    {
        message = null;
        return false;
    }
}
