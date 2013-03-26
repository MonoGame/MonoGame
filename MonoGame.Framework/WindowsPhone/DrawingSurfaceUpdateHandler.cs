using System;
using System.Windows;
using System.Threading;
using SharpDX;
using SharpDX.Direct3D11;

namespace MonoGame.Framework.WindowsPhone
{
    class DrawingSurfaceUpdateHandler : Component
    {
        private Device _device;
        private DeviceContext _deviceContext;
        private Texture2D _renderTarget;
        private RenderTargetView _renderTargetView;
        private readonly DrawingSurfaceContentProvider _drawingSurfaceContentProvider;

        public object ContentProvider { get { return _drawingSurfaceContentProvider; } }


        public DrawingSurfaceUpdateHandler(Microsoft.Xna.Framework.Game game)
        {
            _drawingSurfaceContentProvider = new DrawingSurfaceContentProvider(this, game);
        }

        private void CreateWindowSizeDependentResources(Size2F size)
        {
            Texture2DDescription renderTargetDesc = new Texture2DDescription
            {
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                Width = (int)size.Width,
                Height = (int)size.Height,
                ArraySize = 1,
                MipLevels = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.SharedKeyedmutex | ResourceOptionFlags.SharedNthandle,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0)
            };

            _renderTarget = ToDispose(new Texture2D(_device, renderTargetDesc));
            _renderTargetView = ToDispose(new RenderTargetView(_device, _renderTarget));

            var viewport = new SharpDX.ViewportF(0, 0, (float)size.Width, (float)size.Height);
            _deviceContext.Rasterizer.SetViewport(viewport);
        }

        private void CreateDeviceResources()
        {
            // This flag adds support for surfaces with a different color channel ordering
            // than the API default. It is required for compatibility with Direct2D.
            DeviceCreationFlags creationFlags = DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug;

            // This array defines the set of DirectX hardware feature levels this app will support.
            // Note the ordering should be preserved.
            // Don't forget to declare your application's minimum required feature level in its
            // description.  All applications are assumed to support 9.1 unless otherwise stated.

            SharpDX.Direct3D.FeatureLevel[] featureLevels = 
	        {
                SharpDX.Direct3D.FeatureLevel.Level_11_1,
		        SharpDX.Direct3D.FeatureLevel.Level_11_0,
		        SharpDX.Direct3D.FeatureLevel.Level_10_1,
		        SharpDX.Direct3D.FeatureLevel.Level_10_0,
		        SharpDX.Direct3D.FeatureLevel.Level_9_3
	        };

            // Create the Direct3D 11 API device object and a corresponding context.
            using (var defaultDevice = new SharpDX.Direct3D11.Device(SharpDX.Direct3D.DriverType.Hardware, creationFlags, featureLevels))
                _device = defaultDevice.QueryInterface<SharpDX.Direct3D11.Device1>();

            // Get Direct3D 11.1 context
            _deviceContext = ToDispose(_device.ImmediateContext.QueryInterface<SharpDX.Direct3D11.DeviceContext1>());
        }

        private void DisposeResources()
        {
            RemoveAndDispose(ref _device);
            RemoveAndDispose(ref _deviceContext);
            RemoveAndDispose(ref _renderTarget);
            RemoveAndDispose(ref _renderTargetView);
        }


        class DrawingSurfaceContentProvider : DrawingSurfaceContentProviderNativeBase
        {
            private DrawingSurfaceRuntimeHost _host;
            private DrawingSurfaceSynchronizedTexture _synchronizedTexture;
            private readonly DrawingSurfaceUpdateHandler _drawingSurfaceUpdateHandler;
            private readonly SurfaceUpdateHandler _surfaceUpdateHandler;

            public DrawingSurfaceContentProvider(DrawingSurfaceUpdateHandler drawingSurfaceUpdateHandler, Microsoft.Xna.Framework.Game game)
            {
                _drawingSurfaceUpdateHandler = drawingSurfaceUpdateHandler;
                _surfaceUpdateHandler = new SurfaceUpdateHandler(game);
            }

            public override void Connect(DrawingSurfaceRuntimeHost host)
            {
                _host = host;
                _surfaceUpdateHandler.Connect(host);
            }

            public override void Disconnect()
            {
                _surfaceUpdateHandler.Disconnect();
                _host = null;
                SharpDX.Utilities.Dispose(ref _synchronizedTexture);
                _drawingSurfaceUpdateHandler.DisposeResources();
            }

            public override void GetTexture(Size2F surfaceSize, out DrawingSurfaceSynchronizedTexture synchronizedTexture, out RectangleF textureSubRectangle)
            {
                if (_synchronizedTexture == null)
                {
                    _drawingSurfaceUpdateHandler.CreateDeviceResources();
                    _drawingSurfaceUpdateHandler.CreateWindowSizeDependentResources(surfaceSize);
                    _synchronizedTexture = _host.CreateSynchronizedTexture(_drawingSurfaceUpdateHandler._renderTarget);
                }

                synchronizedTexture = _synchronizedTexture;
                textureSubRectangle = new RectangleF
                {
                    Right = surfaceSize.Width,
                    Bottom = surfaceSize.Height
                };

                _surfaceUpdateHandler.UpdateGameWindowSize(surfaceSize);

                _synchronizedTexture.BeginDraw();

                _surfaceUpdateHandler.Draw(
                    _drawingSurfaceUpdateHandler._device,
                    _drawingSurfaceUpdateHandler._deviceContext,
                    _drawingSurfaceUpdateHandler._renderTargetView);

               _synchronizedTexture.EndDraw();
            }

            public override void PrepareResources(DateTime presentTargetTime, out SharpDX.Bool isContentDirty)
            {
                isContentDirty = true;
            }
        }
    }
}
