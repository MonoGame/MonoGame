// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

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
    public partial class SamplerState : GraphicsResource
    {
        public static SamplerState AnisotropicClamp
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.AnisotropicClamp);
                return GraphicsDeviceContext.Current.AnisotropicClamp.Value;
            }
		}

        public static SamplerState AnisotropicWrap
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.AnisotropicWrap);
                return GraphicsDeviceContext.Current.AnisotropicWrap.Value;
            }
        }

        public static SamplerState LinearClamp
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.LinearClamp);
                return GraphicsDeviceContext.Current.LinearClamp.Value;
            }
        }

        public static SamplerState LinearWrap
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.LinearWrap);
                return GraphicsDeviceContext.Current.LinearWrap.Value;
            }
        }

        public static SamplerState PointClamp
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.PointClamp);
                return GraphicsDeviceContext.Current.PointClamp.Value;
            }
        }

        public static SamplerState PointWrap
        {
            get
            {
                ThrowIfGraphicsDeviceContextNull();
                DebugAssertGraphicsDeviceContext(GraphicsDeviceContext.Current.PointWrap);
                return GraphicsDeviceContext.Current.PointWrap.Value;
            }
        }

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
    }
}

