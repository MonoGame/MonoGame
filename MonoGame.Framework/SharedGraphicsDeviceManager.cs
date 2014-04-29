using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

#if WINRT && !WINDOWS_PHONE
using Windows.UI.Xaml.Controls;
#endif

namespace Microsoft.Xna.Framework
{
    public class SharedGraphicsDeviceManager : IGraphicsDeviceService, IDisposable
    {
        public static readonly int DefaultBackBufferHeight;
        public static readonly int DefaultBackBufferWidth;

        static SharedGraphicsDeviceManager()
        {
#if WINRT
            DefaultBackBufferWidth = 1024;
            DefaultBackBufferHeight = 768;
#endif
        }

        private GameTimer _timer;

        public SharedGraphicsDeviceManager()
        {
            if (Current != null)
                throw new InvalidOperationException("Only one device manager can be created per process!");
            
#if WINRT
            GraphicsProfile = GraphicsProfile.HiDef;

            PreferredBackBufferFormat = SurfaceFormat.Color;
            PreferredBackBufferWidth = DefaultBackBufferWidth;
            PreferredBackBufferHeight = DefaultBackBufferHeight;
            PreferredDepthStencilFormat = DepthFormat.Depth24;

            PresentationInterval = PresentInterval.One;

            SynchronizeWithVerticalRetrace = true;
#endif
            Current = this;
        }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public GraphicsProfile GraphicsProfile { get; set; }

        public int MultiSampleCount { get; set; }

        public SurfaceFormat PreferredBackBufferFormat { get; set; }

        public int PreferredBackBufferWidth { get; set; }

        public int PreferredBackBufferHeight { get; set; }

        public DepthFormat PreferredDepthStencilFormat { get; set; }

        public PresentInterval PresentationInterval { get; set; }

        public RenderTargetUsage RenderTargetUsage { get; set; }

        public bool SynchronizeWithVerticalRetrace { get; set; }

#if WINRT && !WINDOWS_PHONE
        [CLSCompliant(false)]
        public SwapChainBackgroundPanel SwapChainBackgroundPanel { get; set; }
#endif 

        public event EventHandler<EventArgs> DeviceCreated;

        public event EventHandler<EventArgs> DeviceDisposing;

        public event EventHandler<EventArgs> DeviceReset;

        public event EventHandler<EventArgs> DeviceResetting;

        public static SharedGraphicsDeviceManager Current { get; private set; }

        public void ApplyChanges()
        {
            var createDevice = GraphicsDevice == null;

            var presentationParameters = createDevice ? new PresentationParameters() : GraphicsDevice.PresentationParameters;
            presentationParameters.BackBufferWidth = PreferredBackBufferWidth;
            presentationParameters.BackBufferHeight = PreferredBackBufferHeight;
            presentationParameters.BackBufferFormat = PreferredBackBufferFormat;
            presentationParameters.DepthStencilFormat = PreferredDepthStencilFormat;
            presentationParameters.MultiSampleCount = MultiSampleCount;
            presentationParameters.PresentationInterval = PresentationInterval;
            presentationParameters.IsFullScreen = false;
            presentationParameters.SwapChainBackgroundPanel = this.SwapChainBackgroundPanel;

            if (createDevice)
            {
                GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile, presentationParameters);
            }
            else
            {
                GraphicsDevice.CreateSizeDependentResources();
                GraphicsDevice.ApplyRenderTargets(null);
            }

            // Set the new display size on the touch panel.
            //
            // TODO: In XNA this seems to be done as part of the 
            // GraphicsDevice.DeviceReset event... we need to get 
            // those working.
            //
            TouchPanel.DisplayWidth = GraphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = GraphicsDevice.PresentationParameters.BackBufferHeight;
        }

        public void Dispose()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }

            if ( GraphicsDevice != null )
            {
                GraphicsDevice.Dispose();
                GraphicsDevice = null;
            }

            Current = null;
        }
    }
}
