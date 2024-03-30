// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics;

partial class OcclusionQuery
{
    private void PlatformConstruct()
    {
    }

    private void PlatformBegin()
    {
    }

    private void PlatformEnd()
    {
    }

    private bool PlatformGetResult(out int pixelCount)
    {
        pixelCount = 0;
        return false;
    }
}
