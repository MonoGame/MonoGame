// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Sce.PlayStation.Core.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RasterizerState
    {
        static readonly Dictionary<CullMode, CullFaceMode> MapCullMode = new Dictionary<CullMode, CullFaceMode> {
            {CullMode.None, CullFaceMode.None},
            {CullMode.CullClockwiseFace, CullFaceMode.Front}, // Cull cw
            {CullMode.CullCounterClockwiseFace, CullFaceMode.Back}, // Cull ccw
        };
        
        internal void PlatformApplyState(GraphicsDevice device)
        {
            var g = device.Context;
            
            g.SetCullFace(MapCullMode[CullMode], CullFaceDirection.Cw); // Front == cw
            g.Enable(EnableMode.CullFace, this.CullMode != CullMode.None);
            
            // FIXME: Everything else
        }
    }
}
