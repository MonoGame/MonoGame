namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class GraphicsCapabilities
    {
        private void PlatformInitialize(GraphicsDevice device)
        {
            SupportsNonPowerOfTwo = device.GraphicsProfile == GraphicsProfile.HiDef;
            SupportsTextureFilterAnisotropic = true;

            SupportsDepth24 = true;
            SupportsPackedDepthStencil = true;
            SupportsDepthNonLinear = false;
            SupportsTextureMaxLevel = true;

            // Texture compression
            SupportsDxt1 = true;
            SupportsS3tc = true;

            SupportsSRgb = true;

            SupportsTextureArrays = device.GraphicsProfile == GraphicsProfile.HiDef;
            SupportsDepthClamp = device.GraphicsProfile == GraphicsProfile.HiDef;
            SupportsVertexTextures = device.GraphicsProfile == GraphicsProfile.HiDef;
        }
    }
}