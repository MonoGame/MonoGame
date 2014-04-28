// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class SamplerStateCollection
    {
        private void PlatformSetSamplerState(int index)
        {
        }

        private void PlatformClear()
        {
        }

        private void PlatformDirty()
        {
        }

        internal void PlatformSetSamplers(GraphicsDevice device)
        {
            for (var i = 0; i < _samplers.Length; i++)
            {
                var sampler = _samplers[i];
                var texture = device.Textures[i] as Texture2D;
                if (texture == null)
                    continue;
                
                var psmTexture = texture._texture2D;
                
                // FIXME: Handle mip attributes
                
                // FIXME: Separable filters
                psmTexture.SetFilter(
                    sampler.Filter == TextureFilter.Point
                        ? Sce.PlayStation.Core.Graphics.TextureFilterMode.Nearest
                        : Sce.PlayStation.Core.Graphics.TextureFilterMode.Linear
                );
                // FIXME: The third address mode
                psmTexture.SetWrap(
                    sampler.AddressU == TextureAddressMode.Clamp
                        ? Sce.PlayStation.Core.Graphics.TextureWrapMode.ClampToEdge
                        : Sce.PlayStation.Core.Graphics.TextureWrapMode.Repeat,
                    sampler.AddressV == TextureAddressMode.Clamp
                        ? Sce.PlayStation.Core.Graphics.TextureWrapMode.ClampToEdge
                        : Sce.PlayStation.Core.Graphics.TextureWrapMode.Repeat
                );
            
            }            
        }
	}
}
