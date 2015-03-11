// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
  public partial class SamplerState
  {
#if GLES
        private const TextureParameterName TextureParameterNameTextureMaxAnisotropy = (TextureParameterName)All.TextureMaxAnisotropyExt;
        private const TextureParameterName TextureParameterNameTextureMaxLevel = (TextureParameterName)0x813D;
#else
        private const TextureParameterName TextureParameterNameTextureMaxAnisotropy = (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt;
        private const TextureParameterName TextureParameterNameTextureMaxLevel = TextureParameterName.TextureMaxLevel;
#endif

        internal void Activate(GraphicsDevice device, TextureTarget target, bool useMipmaps, int glTexture)
        {
            if (GraphicsDevice == null)
            {
                // We're now bound to a device... no one should
                // be changing the state of this object now!
                GraphicsDevice = device;
            }
            Debug.Assert(GraphicsDevice == device, "The state was created for a different device!");

            float newMaxAnisotropy;
            TextureMinFilter newMinFilter;
            TextureMagFilter newMagFilter;
            switch (Filter)
      {
      case TextureFilter.Point:
                newMaxAnisotropy = 1.0f;
                newMinFilter = useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest;
                newMagFilter = TextureMagFilter.Nearest;
        break;
      case TextureFilter.Linear:
				newMaxAnisotropy = 1.0f;
                newMinFilter = useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear;
                newMagFilter = TextureMagFilter.Linear;
        break;
      case TextureFilter.Anisotropic:
				newMaxAnisotropy = MathHelper.Clamp(this.MaxAnisotropy, 1.0f, GraphicsDevice.GraphicsCapabilities.MaxTextureAnisotropy);
                newMinFilter = useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear;
                newMagFilter = TextureMagFilter.Linear;
        break;
      case TextureFilter.PointMipLinear:
				newMaxAnisotropy = 1.0f;
                newMinFilter = useMipmaps ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest;
                newMagFilter = TextureMagFilter.Nearest;
        break;
            case TextureFilter.LinearMipPoint:
				newMaxAnisotropy = 1.0f;
                newMinFilter = useMipmaps ?  TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear;
                newMagFilter = TextureMagFilter.Linear;
                break;
            case TextureFilter.MinLinearMagPointMipLinear:
				newMaxAnisotropy = 1.0f;
                newMinFilter = useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear;
                newMagFilter = TextureMagFilter.Nearest;
                break;
            case TextureFilter.MinLinearMagPointMipPoint:
				newMaxAnisotropy = 1.0f;
                newMinFilter = useMipmaps ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear;
                newMagFilter = TextureMagFilter.Nearest;
                break;
            case TextureFilter.MinPointMagLinearMipLinear:
				newMaxAnisotropy = 1.0f;
                newMinFilter = useMipmaps ? TextureMinFilter.NearestMipmapLinear: TextureMinFilter.Nearest;
                newMagFilter = TextureMagFilter.Linear;
                break;
            case TextureFilter.MinPointMagLinearMipPoint:
				newMaxAnisotropy = 1.0f;
                newMinFilter = useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest;
                newMagFilter = TextureMagFilter.Linear;
                break;
      default:
        throw new NotSupportedException();
      }

            if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
            {
                GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, newMaxAnisotropy);
                GraphicsExtensions.CheckGLError();
            }

            TextureMinFilter lastTextureMinFilter;
            if (!device._lastTextureMinFilter.TryGetValue(glTexture, out lastTextureMinFilter) || lastTextureMinFilter != newMinFilter)
            {
                GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)newMinFilter);
                GraphicsExtensions.CheckGLError();

                device._lastTextureMinFilter[glTexture] = newMinFilter;
            }
            
            TextureMagFilter lastTextureMagFilter;
            if (!device._lastTextureMagFilter.TryGetValue(glTexture, out lastTextureMagFilter) || lastTextureMagFilter != newMagFilter)
            {
                GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)newMagFilter);
                GraphicsExtensions.CheckGLError();

                device._lastTextureMagFilter[glTexture] = newMagFilter;
            }

      // Set up texture addressing.
      GL.TexParameter(target, TextureParameterName.TextureWrapS, (int)GetWrapMode(AddressU));
            GraphicsExtensions.CheckGLError();
            GL.TexParameter(target, TextureParameterName.TextureWrapT, (int)GetWrapMode(AddressV));
            GraphicsExtensions.CheckGLError();
#if !GLES
            // LOD bias is not supported by glTexParameter in OpenGL ES 2.0
            GL.TexParameter(target, TextureParameterName.TextureLodBias, MipMapLevelOfDetailBias);
            GraphicsExtensions.CheckGLError();
#endif
            if (GraphicsDevice.GraphicsCapabilities.SupportsTextureMaxLevel)
            {
                if (this.MaxMipLevel > 0)
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterNameTextureMaxLevel, this.MaxMipLevel);
                }
                else
                {
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterNameTextureMaxLevel, 1000);
                }
            }
        }

    private int GetWrapMode(TextureAddressMode textureAddressMode)
    {
      switch(textureAddressMode)
      {
      case TextureAddressMode.Clamp:
        return (int)TextureWrapMode.ClampToEdge;
      case TextureAddressMode.Wrap:
        return (int)TextureWrapMode.Repeat;
      case TextureAddressMode.Mirror:
        return (int)All.MirroredRepeat;
      default:
        throw new ArgumentException("No support for " + textureAddressMode);
      }
    }
  }
}

