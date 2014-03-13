// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey

using System;
using System.Collections.Generic;

#if OPENGL
#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
using TextureUnit = OpenTK.Graphics.ES20.All;
using TextureTarget = OpenTK.Graphics.ES20.All;
#endif
#endif

namespace Microsoft.Xna.Framework.Graphics
{

    public sealed class SamplerStateCollection
	{
        private SamplerState[] _samplers;

#if DIRECTX
        private int _d3dDirty;
#endif

		internal SamplerStateCollection( int maxSamplers )
        {
            _samplers = new SamplerState[maxSamplers];
		    Clear();
        }
		
		public SamplerState this [int index] 
        {
			get 
            { 
                return _samplers[index]; 
            }

			set 
            {
                if (_samplers[index] == value)
                    return;

                _samplers[index] = value;

                PlatformSetSamplerState(index);
            }
		}

        private void PlatformSetSamplerState(int index)
        {
#if DIRECTX
            _d3dDirty |= 1 << index;
#endif
        }

        internal void Clear()
        {
            for (var i = 0; i < _samplers.Length; i++)
                _samplers[i] = SamplerState.LinearWrap;

            PlatformClear();
        }

        private void PlatformClear()
        {
#if DIRECTX
            _d3dDirty = int.MaxValue;
#endif
        }

        /// <summary>
        /// Mark all the sampler slots as dirty.
        /// </summary>
        internal void Dirty()
        {
            PlatformDirty();
        }

        private void PlatformDirty()
        {
#if DIRECTX
            _d3dDirty = int.MaxValue;
#endif
        }

        internal void SetSamplers(GraphicsDevice device)
        {
            PlatformSetSamplers(device);
        }

        private void PlatformSetSamplers(GraphicsDevice device)
        {
#if DIRECTX
            // Skip out if nothing has changed.
            if (_d3dDirty == 0)
                return;

            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.
            var pixelShaderStage = device._d3dContext.PixelShader;

            for (var i = 0; i < _samplers.Length; i++)
            {
                var mask = 1 << i;
                if ((_d3dDirty & mask) == 0)
                    continue;

                var sampler = _samplers[i];
                SharpDX.Direct3D11.SamplerState state = null;
                if (sampler != null)
                    state = sampler.GetState(device);

                pixelShaderStage.SetSampler(i, state);

                _d3dDirty &= ~mask;
                if (_d3dDirty == 0)
                    break;
            }

            _d3dDirty = 0;

#elif OPENGL

            for (var i = 0; i < _samplers.Length; i++)
            {
                var sampler = _samplers[i];
                var texture = device.Textures[i];

                if (sampler != null && texture != null && sampler != texture.glLastSamplerState)
                {
                    // TODO: Avoid doing this redundantly (see TextureCollection.SetTextures())
                    // However, I suspect that rendering from the same texture with different sampling modes
                    // is a relatively rare occurrence...
                    GL.ActiveTexture(TextureUnit.Texture0 + i);
                    GraphicsExtensions.CheckGLError();

                    // NOTE: We don't have to bind the texture here because it is already bound in
                    // TextureCollection.SetTextures(). This, of course, assumes that SetTextures() is called
                    // before this method is called. If that ever changes this code will misbehave.
                    // GL.BindTexture(texture.glTarget, texture.glTexture);
                    // GraphicsExtensions.CheckGLError();

                    sampler.Activate(texture.glTarget, texture.LevelCount > 1);
                    texture.glLastSamplerState = sampler;
                }
            }
#elif PSM
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
#endif
        }
	}
}
