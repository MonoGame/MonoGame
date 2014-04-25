// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

#if MONOMAC
using MonoMac.OpenGL;
#elif WINDOWS || LINUX
using OpenTK.Graphics.OpenGL;
#elif GLES
using OpenTK.Graphics.ES20;
using TextureTarget = OpenTK.Graphics.ES20.All;
using TextureMinFilter = OpenTK.Graphics.ES20.All;
using TextureParameterName = OpenTK.Graphics.ES20.All;
using GetPName = OpenTK.Graphics.ES20.All;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
  public partial class SamplerState
  {
        private static float MaxTextureMaxAnisotropy = 4;
#if GLES
        private const GetPName GetPNameMaxTextureMaxAnisotropy = (GetPName)All.MaxTextureMaxAnisotropyExt;
        private const TextureParameterName TextureParameterNameTextureMaxAnisotropy = (TextureParameterName)All.TextureMaxAnisotropyExt;
#else
        private const GetPName GetPNameMaxTextureMaxAnisotropy = (GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt;
        private const TextureParameterName TextureParameterNameTextureMaxAnisotropy = (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt;
#endif

    internal void Activate(TextureTarget target, bool useMipmaps = false)
    {
            switch (Filter)
      {
      case TextureFilter.Point:
				if (GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                {
                    GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                    GraphicsExtensions.CheckGLError();
                }
        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest));
                GraphicsExtensions.CheckGLError();
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GraphicsExtensions.CheckGLError();
        break;
      case TextureFilter.Linear:
				if (GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                {
                    GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                    GraphicsExtensions.CheckGLError();
                }
        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
                GraphicsExtensions.CheckGLError();
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GraphicsExtensions.CheckGLError();
        break;
      case TextureFilter.Anisotropic:
				if (GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                {
                    GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, MathHelper.Clamp(this.MaxAnisotropy, 1.0f, SamplerState.MaxTextureMaxAnisotropy));
                    GraphicsExtensions.CheckGLError();
                }
        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
                GraphicsExtensions.CheckGLError();
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GraphicsExtensions.CheckGLError();
        break;
      case TextureFilter.PointMipLinear:
				if (GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                {
                    GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                    GraphicsExtensions.CheckGLError();
                }
        GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest));
                GraphicsExtensions.CheckGLError();
        GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GraphicsExtensions.CheckGLError();
        break;
            case TextureFilter.LinearMipPoint:
				if (GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                {
                    GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                    GraphicsExtensions.CheckGLError();
                }
                GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear));
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GraphicsExtensions.CheckGLError();
                break;
            case TextureFilter.MinLinearMagPointMipLinear:
				if (GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                {
                    GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                    GraphicsExtensions.CheckGLError();
                }
                GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear));
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GraphicsExtensions.CheckGLError();
                break;
            case TextureFilter.MinLinearMagPointMipPoint:
				if (GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                {
                    GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                    GraphicsExtensions.CheckGLError();
                }
                GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.LinearMipmapNearest: TextureMinFilter.Linear));
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GraphicsExtensions.CheckGLError();
                break;
            case TextureFilter.MinPointMagLinearMipLinear:
				if (GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                {
                    GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                    GraphicsExtensions.CheckGLError();
                }
                GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapLinear: TextureMinFilter.Nearest));
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GraphicsExtensions.CheckGLError();
                break;
            case TextureFilter.MinPointMagLinearMipPoint:
				if (GraphicsCapabilities.SupportsTextureFilterAnisotropic)
                {
                    GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, 1.0f);
                    GraphicsExtensions.CheckGLError();
                }
                GL.TexParameter(target, TextureParameterName.TextureMinFilter, (int)(useMipmaps ? TextureMinFilter.NearestMipmapNearest: TextureMinFilter.Nearest));
                GraphicsExtensions.CheckGLError();
                GL.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GraphicsExtensions.CheckGLError();
                break;
      default:
        throw new NotSupportedException();
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
        return (int)TextureWrapMode.MirroredRepeat;
      default:
        throw new ArgumentException("No support for " + textureAddressMode);
      }
    }
  }
}

