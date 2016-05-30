// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class EffectMaterialContent : MaterialContent
    {
        public ExternalReference<EffectContent> Effect { get; set; }
        public ExternalReference<CompiledEffectContent> CompiledEffect { get; set; }
    }
}
