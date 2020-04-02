// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Effect
{
    public class SamplerStateData
    {
        public TextureAddressMode AddressU { get; set; }
        public TextureAddressMode AddressV { get; set; }
        public TextureAddressMode AddressW { get; set; }
        public Color BorderColor { get; set; }
        public TextureFilter Filter { get; set; }
        public int MaxAnisotropy { get; set; }
        public int MaxMipLevel { get; set; }
        public float MipMapLevelOfDetailBias { get; set; }
        public TextureFilterMode FilterMode { get; set; }
        public CompareFunction ComparisonFunction { get; set; }
    }
}