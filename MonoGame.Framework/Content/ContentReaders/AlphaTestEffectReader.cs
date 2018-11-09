﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content
{
    class AlphaTestEffectReader : ContentTypeReader<AlphaTestEffect>
    {
        protected internal override AlphaTestEffect Read(ContentReader input, AlphaTestEffect existingInstance)
        {
            var effect = new AlphaTestEffect(input.GraphicsDevice);

            effect.Texture = input.ReadExternalReference<Texture>() as Texture2D;
            effect.AlphaFunction = (CompareFunction)input.ReadInt32();
            effect.ReferenceAlpha = (int)input.ReadUInt32();
            effect.DiffuseColor = input.ReadVector3();
            effect.Alpha = input.ReadSingle();
            effect.VertexColorEnabled = input.ReadBoolean();
            return effect;
        }
    }
}
