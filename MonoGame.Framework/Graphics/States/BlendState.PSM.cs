// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Sce.PlayStation.Core.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class BlendState
    {
        internal void PlatformApplyState(GraphicsDevice device)
        {
            device._graphics.Enable(EnableMode.Blend);
            device._graphics.SetBlendFuncAlpha(PSSHelper.ToBlendFuncMode(device.BlendState.AlphaBlendFunction),
                                          PSSHelper.ToBlendFuncFactor(device.BlendState.AlphaSourceBlend),
                                          PSSHelper.ToBlendFuncFactor(device.BlendState.AlphaDestinationBlend));
            device._graphics.SetBlendFuncRgb(PSSHelper.ToBlendFuncMode(device.BlendState.ColorBlendFunction),
                                          PSSHelper.ToBlendFuncFactor(device.BlendState.ColorSourceBlend),
                                          PSSHelper.ToBlendFuncFactor(device.BlendState.ColorDestinationBlend));
        }
	}
}

