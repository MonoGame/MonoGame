using System;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Effect
{
    internal class RasterizerStateData
    {
        public CullMode CullMode { get; set; }
        public float DepthBias { get; set; }
        public FillMode FillMode { get; set; }
        public bool MultiSampleAntiAlias { get; set; }
        public bool ScissorTestEnable { get; set; }
        public float SlopeScaleDepthBias { get; set; }
        public bool DepthClipEnable { get; set; }
    }
}