// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics;

internal partial class GraphicsCapabilities
{
    private void PlatformInitialize(GraphicsDevice device)
    {
        SupportsNonPowerOfTwo = device.GraphicsProfile == GraphicsProfile.HiDef && device.SupportsNonPowerOfTwo;
        SupportsTextureFilterAnisotropic = device.SupportsTextureFilterAnisotropic;

        SupportsDepth24 = device.SupportsDepth24;
        SupportsPackedDepthStencil = device.SupportsPackedDepthStencil;
        SupportsDepthNonLinear = device.SupportsDepthNonLinear;
        SupportsTextureMaxLevel = device.SupportsTextureMaxLevel;

        // Texture compression
        SupportsDxt1 = device.SupportsDxt1;
        SupportsS3tc = device.SupportsS3tc;

        SupportsSRgb = device.SupportsSRgb;

        SupportsTextureArrays = device.GraphicsProfile == GraphicsProfile.HiDef && device.SupportsTextureArrays;
        SupportsDepthClamp = device.GraphicsProfile == GraphicsProfile.HiDef && device.SupportsDepthClamp;
        SupportsVertexTextures = device.GraphicsProfile == GraphicsProfile.HiDef && device.SupportsVertexTextures;
        SupportsFloatTextures = device.SupportsFloatTextures;
        SupportsHalfFloatTextures = device.SupportsHalfFloatTextures;
        SupportsNormalized = device.SupportsNormalized;

        SupportsInstancing = device.SupportsInstancing;
        SupportsBaseIndexInstancing = device.SupportsBaseIndexInstancing;
        SupportsSeparateBlendStates = device.SupportsSeparateBlendStates;

        MaxTextureAnisotropy = device.GraphicsProfile == GraphicsProfile.Reach ?
                               Math.Max(2, device.MaxTextureAnisotropy) :
                               device.MaxTextureAnisotropy;
    }

}
