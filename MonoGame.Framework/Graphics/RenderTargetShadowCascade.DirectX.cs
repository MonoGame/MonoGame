using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Resource = SharpDX.Direct3D11.Resource;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetShadowCascade
    {
        internal DepthStencilView _depthStencilView;

        private void PlatformConstruct(GraphicsDevice graphicsDevice, int width, int height)
        {
            GenerateIfRequired();
        }

        private void GenerateIfRequired()
        {
            if (_depthStencilView != null)
                return;

            // see https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch10.html

            var depthStencilViewDescription = new DepthStencilViewDescription();
            depthStencilViewDescription.Format = SharpDX.DXGI.Format.D32_Float;
            depthStencilViewDescription.Dimension = DepthStencilViewDimension.Texture2DArray;
            depthStencilViewDescription.Texture2DArray.ArraySize = ArraySize;
            depthStencilViewDescription.Texture2DArray.FirstArraySlice = 0;
            depthStencilViewDescription.Texture2DArray.MipSlice = 0;
            _depthStencilView = new DepthStencilView(GraphicsDevice._d3dDevice, GetTexture(), depthStencilViewDescription);
        }

        private void PlatformGraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref _depthStencilView);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref _depthStencilView);
            }

            base.Dispose(disposing);
        }

        RenderTargetView IRenderTarget.GetRenderTargetView(int arraySlice)
        {
            return null;
        }

        DepthStencilView IRenderTarget.GetDepthStencilView()
        {
            GenerateIfRequired();
            return _depthStencilView;
        }


        internal override Resource CreateTexture()
        {
            var rt = base.CreateTexture();
            return rt;
        }

        protected override ShaderResourceView CreateShaderResourceView()
        {
            var shaderResourceViewDescription = new ShaderResourceViewDescription();
            shaderResourceViewDescription.Format = SharpDX.DXGI.Format.R32_Float;
            shaderResourceViewDescription.Dimension = ShaderResourceViewDimension.Texture2DArray;
            shaderResourceViewDescription.Texture2DArray.ArraySize = ArraySize;
            shaderResourceViewDescription.Texture2DArray.FirstArraySlice = 0;
            shaderResourceViewDescription.Texture2DArray.MipLevels = 1;

            return new SharpDX.Direct3D11.ShaderResourceView(GraphicsDevice._d3dDevice, GetTexture(), shaderResourceViewDescription);
        }

        protected internal override Texture2DDescription GetTexture2DDescription()
        {
            var desc = new Texture2DDescription();
            desc.Width = width;
            desc.Height = height;
            desc.MipLevels = 1;
            desc.ArraySize = ArraySize;
            desc.Format = SharpDX.DXGI.Format.R32_Typeless;
            desc.BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource;
            desc.CpuAccessFlags = CpuAccessFlags.None;
            desc.SampleDescription = CreateSampleDescription();
            desc.Usage = ResourceUsage.Default;
            desc.OptionFlags = ResourceOptionFlags.None;

            //if (_shared)
            //    desc.OptionFlags |= ResourceOptionFlags.Shared;

            return desc;
        }
    }
}
