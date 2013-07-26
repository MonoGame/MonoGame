using System;
using Microsoft.Xna.Framework;
using SharpDX;
using SharpDX.Direct3D11;

namespace MonoGame.Framework.WindowsPhone
{
    class SurfaceUpdateHandler
    {
        private Device _device;
        private DeviceContext _context;
        private readonly Game _game;
        private DrawingSurfaceRuntimeHost _host;


        public SurfaceUpdateHandler(Game game)
        {
            _game = game;
        }

        public void Connect(DrawingSurfaceRuntimeHost host)
        {
            _host = host;
        }

        public void Disconnect()
        {
            // DeviceResetting events
            _game.graphicsDeviceManager.OnDeviceResetting(EventArgs.Empty);
            if (_game.GraphicsDevice != null)
                _game.GraphicsDevice.OnDeviceResetting();

            Microsoft.Xna.Framework.Input.Touch.TouchPanel.ReleaseAllTouches();

            _host = null;
        }

        public void Draw(Device device, DeviceContext context, RenderTargetView renderTargetView)
        {
            var deviceChanged = _device != device || _context != context;
            _device = device;
            _context = context;

            if (deviceChanged)
            {
                DrawingSurfaceState.Device = _device;
                DrawingSurfaceState.Context = _context;
                DrawingSurfaceState.RenderTargetView = renderTargetView;
            }

            if (!_game.Initialized)
            {
                // Start running the game.
                _game.Run(GameRunBehavior.Asynchronous);
            }
            else if (deviceChanged)
            {
                _game.GraphicsDevice.Initialize();

                Microsoft.Xna.Framework.Content.ContentManager.ReloadGraphicsContent();

                // DeviceReset events
                _game.graphicsDeviceManager.OnDeviceReset(EventArgs.Empty);
                _game.GraphicsDevice.OnDeviceReset();
            }

            _game.GraphicsDevice.UpdateTarget(renderTargetView);
            _game.GraphicsDevice.ResetRenderTargets();
            _game.Tick();

            _host.RequestAdditionalFrame();
        }

        public void UpdateGameWindowSize(Size2F desiredRenderTargetSize)
        {
            WindowsPhoneGameWindow.Width = desiredRenderTargetSize.Width;
            WindowsPhoneGameWindow.Height = desiredRenderTargetSize.Height;
        }
    }
}
