// #region License
// /*
// Microsoft Public License (Ms-PL)
// XnaTouch - Copyright © 2009 The XnaTouch Team
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
using System;
using System.Diagnostics;

#if OPENGL
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
#endif

namespace Microsoft.Xna.Framework.Graphics
{
  public class SamplerState : GraphicsResource
  {
#if DIRECTX
        private SharpDX.Direct3D11.SamplerState _state;
#endif

#if OPENGL
        private static float MaxTextureMaxAnisotropy = 4;
#if GLES
        private const GetPName GetPNameMaxTextureMaxAnisotropy = (GetPName)All.MaxTextureMaxAnisotropyExt;
        private const TextureParameterName TextureParameterNameTextureMaxAnisotropy = (TextureParameterName)All.TextureMaxAnisotropyExt;
#else
        private const GetPName GetPNameMaxTextureMaxAnisotropy = (GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt;
        private const TextureParameterName TextureParameterNameTextureMaxAnisotropy = (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt;
#endif
#endif

    static SamplerState () 
        {
#if OPENGL
#if GLES
			if (GraphicsCapabilities.SupportsTextureFilterAnisotropic)
            {
                GL.GetFloat(GetPNameMaxTextureMaxAnisotropy, ref SamplerState.MaxTextureMaxAnisotropy);
            }
#else
            GL.GetFloat(GetPNameMaxTextureMaxAnisotropy, out SamplerState.MaxTextureMaxAnisotropy);
#endif
            GraphicsExtensions.CheckGLError();
#endif
	
			_anisotropicClamp = new Utilities.ObjectFactoryWithReset<SamplerState>(() => new SamplerState
            {
                Name = "SamplerState.AnisotropicClamp",
				Filter = TextureFilter.Anisotropic,
				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
			});
			
			_anisotropicWrap = new Utilities.ObjectFactoryWithReset<SamplerState>(() => new SamplerState
            {
                Name = "SamplerState.AnisotropicWrap",
				Filter = TextureFilter.Anisotropic,
				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
			});
			
			_linearClamp = new Utilities.ObjectFactoryWithReset<SamplerState>(() => new SamplerState
            {
                Name = "SamplerState.LinearClamp",
				Filter = TextureFilter.Linear,
				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
			});
			
			_linearWrap = new Utilities.ObjectFactoryWithReset<SamplerState>(() => new SamplerState
            {
                Name = "SamplerState.LinearWrap",
				Filter = TextureFilter.Linear,
				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
			});
			
			_pointClamp = new Utilities.ObjectFactoryWithReset<SamplerState>(() => new SamplerState
            {
                Name = "SamplerState.PointClamp",
				Filter = TextureFilter.Point,
				AddressU = TextureAddressMode.Clamp,
				AddressV = TextureAddressMode.Clamp,
				AddressW = TextureAddressMode.Clamp,
			});
			
			_pointWrap = new Utilities.ObjectFactoryWithReset<SamplerState>(() => new SamplerState
            {
                Name = "SamplerState.PointWrap",
				Filter = TextureFilter.Point,
				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
			});
		}
		
		private static readonly Utilities.ObjectFactoryWithReset<SamplerState> _anisotropicClamp;
        private static readonly Utilities.ObjectFactoryWithReset<SamplerState> _anisotropicWrap;
        private static readonly Utilities.ObjectFactoryWithReset<SamplerState> _linearClamp;
        private static readonly Utilities.ObjectFactoryWithReset<SamplerState> _linearWrap;
        private static readonly Utilities.ObjectFactoryWithReset<SamplerState> _pointClamp;
        private static readonly Utilities.ObjectFactoryWithReset<SamplerState> _pointWrap;

        public static SamplerState AnisotropicClamp { get { return _anisotropicClamp.Value; } }
        public static SamplerState AnisotropicWrap { get { return _anisotropicWrap.Value; } }
        public static SamplerState LinearClamp { get { return _linearClamp.Value; } }
        public static SamplerState LinearWrap { get { return _linearWrap.Value; } }
        public static SamplerState PointClamp { get { return _pointClamp.Value; } }
        public static SamplerState PointWrap { get { return _pointWrap.Value; } }
        
        public TextureAddressMode AddressU { get; set; }
		public TextureAddressMode AddressV { get; set; }
		public TextureAddressMode AddressW { get; set; }
		public TextureFilter Filter { get; set; }
		
		public int MaxAnisotropy { get; set; }
		public int MaxMipLevel { get; set; }
		public float MipMapLevelOfDetailBias { get; set; }

        public SamplerState()
        {
            this.Filter = TextureFilter.Linear;
            this.AddressU = TextureAddressMode.Wrap;
            this.AddressV = TextureAddressMode.Wrap;
            this.AddressW = TextureAddressMode.Wrap;
            this.MaxAnisotropy = 4;
            this.MaxMipLevel = 0;
            this.MipMapLevelOfDetailBias = 0.0f;
        }
    
#if DIRECTX

        internal static void ResetStates()
        {
            _anisotropicClamp.Reset();
            _anisotropicWrap.Reset();
            _linearClamp.Reset();
            _linearWrap.Reset();
            _pointClamp.Reset();
            _pointWrap.Reset();
        }

        protected internal override void GraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _state);
            base.GraphicsDeviceResetting();
        }

        internal SharpDX.Direct3D11.SamplerState GetState(GraphicsDevice device)
        {
            if (_state == null)
            {
                // We're now bound to a device... no one should
                // be changing the state of this object now!
                GraphicsDevice = device;

                // Build the description.
                var desc = new SharpDX.Direct3D11.SamplerStateDescription();

                desc.AddressU = GetAddressMode(AddressU);
                desc.AddressV = GetAddressMode(AddressV);
                desc.AddressW = GetAddressMode(AddressW);

                desc.Filter = GetFilter(Filter);
                desc.MaximumAnisotropy = MaxAnisotropy;
                desc.MipLodBias = MipMapLevelOfDetailBias;

                // TODO: How do i do these?
                desc.MinimumLod = 0.0f;
                desc.BorderColor = new SharpDX.Color4(0, 0, 0, 0);

                // To support feature level 9.1 these must 
                // be set to these exact values.
                desc.MaximumLod = float.MaxValue;
                desc.ComparisonFunction = SharpDX.Direct3D11.Comparison.Never;

                // Create the state.
                _state = new SharpDX.Direct3D11.SamplerState(GraphicsDevice._d3dDevice, desc);
            }

            Debug.Assert(GraphicsDevice == device, "The state was created for a different device!");

            return _state;
        }

        private static SharpDX.Direct3D11.Filter GetFilter(TextureFilter filter)
        {
            switch (filter)
            {
                case TextureFilter.Anisotropic:
                    return SharpDX.Direct3D11.Filter.Anisotropic;

                case TextureFilter.Linear:
                    return SharpDX.Direct3D11.Filter.MinMagMipLinear;

                case TextureFilter.LinearMipPoint:
                    return SharpDX.Direct3D11.Filter.MinMagLinearMipPoint;

                case TextureFilter.MinLinearMagPointMipLinear:
                    return SharpDX.Direct3D11.Filter.MinLinearMagPointMipLinear;

                case TextureFilter.MinLinearMagPointMipPoint:
                    return SharpDX.Direct3D11.Filter.MinLinearMagMipPoint;

                case TextureFilter.MinPointMagLinearMipLinear:
                    return SharpDX.Direct3D11.Filter.MinPointMagMipLinear;

                case TextureFilter.MinPointMagLinearMipPoint:
                    return SharpDX.Direct3D11.Filter.MinPointMagLinearMipPoint;

                case TextureFilter.Point:
                    return SharpDX.Direct3D11.Filter.MinMagMipPoint;

                case TextureFilter.PointMipLinear:
                    return SharpDX.Direct3D11.Filter.MinMagPointMipLinear;

                default:
                    throw new ArgumentException("Invalid texture filter!");
            }
        }

        private static SharpDX.Direct3D11.TextureAddressMode GetAddressMode(TextureAddressMode mode)
        {
            switch (mode)
            {
                case TextureAddressMode.Clamp:
                    return SharpDX.Direct3D11.TextureAddressMode.Clamp;

                case TextureAddressMode.Mirror:
                    return SharpDX.Direct3D11.TextureAddressMode.Mirror;

                case TextureAddressMode.Wrap:
                    return SharpDX.Direct3D11.TextureAddressMode.Wrap;

                default:
                    throw new ArgumentException("Invalid texture address mode!");
            }
        }

#endif // DIRECTX


#if OPENGL

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

#endif // OPENGL

  }
}

