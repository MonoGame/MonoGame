// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics;

internal partial class GraphicsCapabilities
{
    private void PlatformInitialize(GraphicsDevice device)
    {
        SupportsNonPowerOfTwo = device.GraphicsProfile == GraphicsProfile.HiDef;
        SupportsTextureFilterAnisotropic = true;

        SupportsDepth24 = true;
        SupportsPackedDepthStencil = true;
        SupportsDepthNonLinear = false;
        SupportsTextureMaxLevel = true;

        // Texture compression
        SupportsDxt1 = true;
        SupportsS3tc = true;

        SupportsSRgb = true;

        SupportsTextureArrays = device.GraphicsProfile == GraphicsProfile.HiDef;
        SupportsDepthClamp = device.GraphicsProfile == GraphicsProfile.HiDef;
        SupportsVertexTextures = device.GraphicsProfile == GraphicsProfile.HiDef;
        SupportsFloatTextures = true;
        SupportsHalfFloatTextures = true;
        SupportsNormalized = true;

        SupportsInstancing = true;
        SupportsBaseIndexInstancing = true;
        SupportsSeparateBlendStates = true;

        MaxTextureAnisotropy = (device.GraphicsProfile == GraphicsProfile.Reach) ? 2 : 16;

        _maxMultiSampleCount = 4;// GetMaxMultiSampleCount(device);
    }

}
