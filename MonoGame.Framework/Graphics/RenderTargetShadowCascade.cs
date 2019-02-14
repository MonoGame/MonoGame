using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RenderTargetShadowCascade : Texture2D, IRenderTarget
    {
        public DepthFormat DepthStencilFormat { get; private set; }

        //public int MultiSampleCount { get; private set; }

        public RenderTargetUsage RenderTargetUsage { get; private set; }

        public bool IsContentLost { get { return false; } }

        public event EventHandler<EventArgs> ContentLost;

        private bool SuppressEventHandlerWarningsUntilEventsAreProperlyImplemented()
        {
            return ContentLost != null;
        }

        public RenderTargetShadowCascade(GraphicsDevice graphicsDevice, int width, int height, int arraySize)
        : base(graphicsDevice, width, height, false, SurfaceFormat.Single, SurfaceType.Texture, false, arraySize)
        {
            //DepthFormat preferredDepthFormat = DepthFormat.Depth_R32_Typeless;
            DepthStencilFormat = DepthFormat.Depth_R32_Typeless;
            //MultiSampleCount = 0;
            RenderTargetUsage = RenderTargetUsage.DiscardContents;

            PlatformConstruct(graphicsDevice, width, height);
        }

        protected internal override void GraphicsDeviceResetting()
        {
            PlatformGraphicsDeviceResetting();
            base.GraphicsDeviceResetting();
        }
    }
}
