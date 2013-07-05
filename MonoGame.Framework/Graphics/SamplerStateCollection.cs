// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
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

#if DIRECTX
                _d3dDirty |= 1 << index;
#endif
            }
		}

        internal void Clear()
        {
            for (var i = 0; i < _samplers.Length; i++)
                _samplers[i] = SamplerState.LinearWrap;
            
#if DIRECTX
            _d3dDirty = int.MaxValue;
#endif
        }

        /// <summary>
        /// Mark all the sampler slots as dirty.
        /// </summary>
        internal void Dirty()
        {
#if DIRECTX
            _d3dDirty = int.MaxValue;
#endif
        }

        internal void SetSamplers(GraphicsDevice device)
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
