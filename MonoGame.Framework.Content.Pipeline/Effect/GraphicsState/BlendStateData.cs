// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Effect
{
    internal class BlendStateData
    {
        public BlendFunction AlphaBlendFunction { get; set; }
        public Blend AlphaDestinationBlend { get; set; }
        public Blend AlphaSourceBlend { get; set; }
        public BlendFunction ColorBlendFunction { get; set; }
        public Blend ColorDestinationBlend { get; set; }
        public Blend ColorSourceBlend { get; set; }
        private ColorWriteChannels[] _colorWriteChannels { get; set; } = new ColorWriteChannels[4];
        public Color BlendFactor { get; set; }
        public int MultiSampleMask { get; set; }
        public bool IndependentBlendEnable { get; set; }

        public ColorWriteChannels ColorWriteChannels
        {
            get { return _colorWriteChannels[0]; }
            set { _colorWriteChannels[0] = value; }
        }

        public ColorWriteChannels ColorWriteChannels1
        {
            get { return _colorWriteChannels[1]; }
            set { _colorWriteChannels[1] = value; }
        }

        public ColorWriteChannels ColorWriteChannels2
        {
            get { return _colorWriteChannels[2]; }
            set { _colorWriteChannels[2] = value; }
        }

        public ColorWriteChannels ColorWriteChannels3
        {
            get { return _colorWriteChannels[3]; }
            set { _colorWriteChannels[3] = value; }
        }
    }
}
