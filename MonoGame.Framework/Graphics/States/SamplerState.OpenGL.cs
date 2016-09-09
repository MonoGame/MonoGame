// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

#if MONOMAC
#if PLATFORM_MACOS_LEGACY
using MonoMac.OpenGL;
#else
using OpenTK.Graphics.OpenGL;
#endif
#elif DESKTOPGL
using OpenGL;
using ExtTextureFilterAnisotropic = OpenGL.TextureParameterName;
#elif GLES
using OpenTK.Graphics.ES20;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
  public partial class SamplerState
  {
      private readonly float[] _openGLBorderColor = new float[4];

#if GLES
        private const TextureParameterName TextureParameterNameTextureMaxAnisotropy = (TextureParameterName)All.TextureMaxAnisotropyExt;
        private const TextureParameterName TextureParameterNameTextureMaxLevel = (TextureParameterName)0x813D;
#else
        private const TextureParameterName TextureParameterNameTextureMaxAnisotropy = (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt;
        private const TextureParameterName TextureParameterNameTextureMaxLevel = TextureParameterName.TextureMaxLevel;
#endif

        internal void Activate(GraphicsDevice device, Texture texture)
        {
			TextureTarget target = texture.glTarget;
			bool useMipmaps = texture.LevelCount > 1;
			int glTexture = texture.glTexture;

            if (GraphicsDevice == null)
            {
                // We're now bound to a device... no one should
                // be changing the state of this object now!
                GraphicsDevice = device;
            }
            Debug.Assert(GraphicsDevice == device, "The state was created for a different device!");

            float newMaxAnisotropy;
            int newMinFilter;
            int newMagFilter;
            switch (Filter)
      {
      case TextureFilter.Point:
                newMaxAnisotropy = 1.0f;
				newMinFilter = (int)(useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest);
				newMagFilter = (int)TextureMagFilter.Nearest;
        break;
      case TextureFilter.Linear:
				newMaxAnisotropy = 1.0f;
				newMinFilter = (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear);
				newMagFilter = (int)TextureMagFilter.Linear;
        break;
      case TextureFilter.Anisotropic:
				newMaxAnisotropy = MathHelper.Clamp(this.MaxAnisotropy, 1.0f, GraphicsDevice.GraphicsCapabilities.MaxTextureAnisotropy);
				newMinFilter = (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear);
				newMagFilter = (int)TextureMagFilter.Linear;
        break;
      case TextureFilter.PointMipLinear:
				newMaxAnisotropy = 1.0f;
				newMinFilter = (int)(useMipmaps ? TextureMinFilter.NearestMipmapLinear : TextureMinFilter.Nearest);
				newMagFilter = (int)TextureMagFilter.Nearest;
        break;
            case TextureFilter.LinearMipPoint:
				newMaxAnisotropy = 1.0f;
				newMinFilter = (int)(useMipmaps ?  TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear);
				newMagFilter = (int)TextureMagFilter.Linear;
                break;
            case TextureFilter.MinLinearMagPointMipLinear:
				newMaxAnisotropy = 1.0f;
				newMinFilter = (int)(useMipmaps ? TextureMinFilter.LinearMipmapLinear : TextureMinFilter.Linear);
				newMagFilter = (int)TextureMagFilter.Nearest;
                break;
            case TextureFilter.MinLinearMagPointMipPoint:
				newMaxAnisotropy = 1.0f;
				newMinFilter = (int)(useMipmaps ? TextureMinFilter.LinearMipmapNearest : TextureMinFilter.Linear);
				newMagFilter = (int)TextureMagFilter.Nearest;
                break;
            case TextureFilter.MinPointMagLinearMipLinear:
				newMaxAnisotropy = 1.0f;
				newMinFilter = (int)(useMipmaps ? TextureMinFilter.NearestMipmapLinear: TextureMinFilter.Nearest);
				newMagFilter = (int)TextureMagFilter.Linear;
                break;
            case TextureFilter.MinPointMagLinearMipPoint:
				newMaxAnisotropy = 1.0f;
				newMinFilter = (int)(useMipmaps ? TextureMinFilter.NearestMipmapNearest : TextureMinFilter.Nearest);
				newMagFilter = (int)TextureMagFilter.Linear;
                break;
      default:
        throw new NotSupportedException();
      }

            if (GraphicsDevice.GraphicsCapabilities.SupportsTextureFilterAnisotropic)
            {
                GL.TexParameter(target, TextureParameterNameTextureMaxAnisotropy, newMaxAnisotropy);
                GraphicsExtensions.CheckGLError();
            }

			if (texture._lastTextureMinFilter != newMinFilter)
            {
                GL.TexParameter(target, TextureParameterName.TextureMinFilter, newMinFilter);
                GraphicsExtensions.CheckGLError();

				texture._lastTextureMinFilter = newMinFilter;
            }
            
			if (texture._lastTextureMagFilter != newMagFilter)
            {
                GL.TexParameter(target, TextureParameterName.TextureMagFilter, newMagFilter);
                GraphicsExtensions.CheckGLError();

				texture._lastTextureMagFilter = newMagFilter;
            }

      		// Set up texture addressing.
			int newWrapS = (int)GetWrapMode(AddressU);
			if (texture._lastTextureWrapS != newWrapS)
			{
				GL.TexParameter(target, TextureParameterName.TextureWrapS, newWrapS);
            	GraphicsExtensions.CheckGLError();

				texture._lastTextureWrapS = newWrapS;
			}

			int newWrapT = (int)GetWrapMode(AddressV);
			if (texture._lastTextureWrapT != newWrapT)
			{
				GL.TexParameter(target, TextureParameterName.TextureWrapT, newWrapT);
				GraphicsExtensions.CheckGLError();

				texture._lastTextureWrapT = newWrapT;
			}
#if !GLES
            // Border color is not supported by glTexParameter in OpenGL ES 2.0
            _openGLBorderColor[0] = BorderColor.R / 255.0f;
            _openGLBorderColor[1] = BorderColor.G / 255.0f;
            _openGLBorderColor[2] = BorderColor.B / 255.0f;
            _openGLBorderColor[3] = BorderColor.A / 255.0f;
            GL.TexParameter(target, TextureParameterName.TextureBorderColor, _openGLBorderColor);
            GraphicsExtensions.CheckGLError();
            // LOD bias is not supported by glTexParameter in OpenGL ES 2.0
            GL.TexParameter(target, TextureParameterName.TextureLodBias, MipMapLevelOfDetailBias);
            GraphicsExtensions.CheckGLError();
            // Comparison samplers are not supported in OpenGL ES 2.0 (without an extension, anyway)
            switch (FilterMode)
            {
                case TextureFilterMode.Comparison:
                    GL.TexParameter(target, TextureParameterName.TextureCompareMode, (int) TextureCompareMode.CompareRefToTexture);
                    GraphicsExtensions.CheckGLError();
                    GL.TexParameter(target, TextureParameterName.TextureCompareFunc, (int) ComparisonFunction.GetDepthFunction());
                    GraphicsExtensions.CheckGLError();
                    break;
                case TextureFilterMode.Default:
                    GL.TexParameter(target, TextureParameterName.TextureCompareMode, (int) TextureCompareMode.None);
                    GraphicsExtensions.CheckGLError();
                    break;
                default:
                    throw new InvalidOperationException("Invalid filter mode!");
            }
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
        return (int)TextureWrapMode.MirroredRepeat;
#if !GLES
      case TextureAddressMode.Border:
        return (int)TextureWrapMode.ClampToBorder;
#endif
      default:
        throw new ArgumentException("No support for " + textureAddressMode);
      }
    }
  }
}

