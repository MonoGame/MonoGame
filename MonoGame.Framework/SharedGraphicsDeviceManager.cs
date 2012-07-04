using System;
using Microsoft.Xna.Framework.Graphics;

#if WINRT
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

#if WINRT
        public SwapChainBackgroundPanel SwapChainPanel { get; set; }
#endif 

        public event EventHandler<EventArgs> DeviceCreated;

        public event EventHandler<EventArgs> DeviceDisposing;

        public event EventHandler<EventArgs> DeviceReset;

        public event EventHandler<EventArgs> DeviceResetting;

        public static SharedGraphicsDeviceManager Current { get; private set; }

        public void ApplyChanges()
        {
            var createDevice = GraphicsDevice == null;
            if (createDevice)
                GraphicsDevice = new GraphicsDevice();

            GraphicsDevice.PresentationParameters.BackBufferWidth = PreferredBackBufferWidth;
            GraphicsDevice.PresentationParameters.BackBufferHeight = PreferredBackBufferHeight;
            GraphicsDevice.PresentationParameters.BackBufferFormat = PreferredBackBufferFormat;
            GraphicsDevice.PresentationParameters.DepthStencilFormat = PreferredDepthStencilFormat;
            GraphicsDevice.PresentationParameters.MultiSampleCount = MultiSampleCount;
            GraphicsDevice.PresentationParameters.PresentationInterval = PresentationInterval;
            GraphicsDevice.PresentationParameters.IsFullScreen = false;
            GraphicsDevice.PresentationParameters.SwapChainPanel = SwapChainPanel;

            if (createDevice)
                GraphicsDevice.Initialize();
            else
            {
                GraphicsDevice.CreateSizeDependentResources();
                GraphicsDevice.ApplyRenderTargets(null);
            }
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
