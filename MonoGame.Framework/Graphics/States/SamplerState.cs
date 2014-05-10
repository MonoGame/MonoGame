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
        static SamplerState () 
        {
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

        internal static void ResetStates()
        {
            _anisotropicClamp.Reset();
            _anisotropicWrap.Reset();
            _linearClamp.Reset();
            _linearWrap.Reset();
            _pointClamp.Reset();
            _pointWrap.Reset();
        }
    }
}

