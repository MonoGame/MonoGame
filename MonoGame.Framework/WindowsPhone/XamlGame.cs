using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using SharpDX;
using SharpDX.Direct3D11;
using Windows.Phone.Input.Interop;
using Windows.UI.Core;
using System.Windows.Controls;
using Windows.Graphics.Display;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace MonoGame.Framework.WindowsPhone
{
    /// <summary>
    /// Static class for initializing a Game object for a XAML application.
    /// </summary>
    /// <typeparam name="T">A class derived from Game.</typeparam>
    public class XamlGame<T> : IDrawingSurfaceManipulationHandler
        where T : Game, new()
    {
        class ContentProvider : DrawingSurfaceBackgroundContentProviderNativeBase
        {
            public ContentProvider(XamlGame<T> controller)
            {
                _controller = controller;
            }

            public override void Connect(DrawingSurfaceRuntimeHost host, Device device)
            {
                _host = host;
            }

            public override void Disconnect()
            {
                _host = null;
            }

            public override void Draw(Device device, DeviceContext context, RenderTargetView renderTargetView)
            {
                _controller.Tick(device, context, renderTargetView);
                _host.RequestAdditionalFrame();
            }

            public override void PrepareResources(DateTime presentTargetTime, DrawingSizeF desiredRenderTargetSize)
            {
                WindowsPhoneGameWindow.Width = desiredRenderTargetSize.Width;
                WindowsPhoneGameWindow.Height = desiredRenderTargetSize.Height;
            }

            private readonly XamlGame<T> _controller;
            DrawingSurfaceRuntimeHost _host;
        }

        private Device _device;
        private DeviceContext _context;
        private Game _game;

        public XamlGame()
        {            
        }
 
        /// <summary>
        /// Creates your Game class initializing it to worth within a XAML application window.
        /// </summary>
        /// <param name="launchParameters">The command line arguments from launch.</param>
        /// <param name="drawingSurface">The XAML drawing surface to which we render the scene and recieve input events.</param>
        /// <returns></returns>
        public Game Create(string launchParameters, DrawingSurfaceBackgroundGrid drawingSurface)
        {
            if (launchParameters == null)
                throw new NullReferenceException("The launch parameters cannot be null!");
            if (drawingSurface == null)
                throw new NullReferenceException("The drawing surface cannot be null!");

            WindowsPhoneGamePlatform.LaunchParameters = launchParameters;

            drawingSurface.SetBackgroundContentProvider(new ContentProvider(this));
            drawingSurface.SetBackgroundManipulationHandler(this);

            // Construct the game.
            _game = new T();

            // Set the swap chain panel on the graphics mananger.
            if (_game.graphicsDeviceManager == null)
                throw new NullReferenceException("You must create the GraphicsDeviceManager in the Game constructor!");

            return _game;
        }

        public void SetManipulationHost(DrawingSurfaceManipulationHost manipulationHost)
        {
            manipulationHost.PointerPressed += OnPointerPressed;
            manipulationHost.PointerMoved += OnPointerMoved;
            manipulationHost.PointerReleased += OnPointerReleased;
        }

        protected void OnPointerPressed(DrawingSurfaceManipulationHost sender, PointerEventArgs args)
        {
            var pointerPoint = args.CurrentPoint;

            // To convert from DIPs (device independent pixels) to screen resolution pixels.
            var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
            var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * dipFactor;
            TouchPanel.AddEvent((int)pointerPoint.PointerId, TouchLocationState.Pressed, pos);
        }

        protected void OnPointerMoved(DrawingSurfaceManipulationHost sender, PointerEventArgs args)
        {
            var pointerPoint = args.CurrentPoint;

            // To convert from DIPs (device independent pixels) to screen resolution pixels.
            var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
            var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * dipFactor;
            TouchPanel.AddEvent((int)pointerPoint.PointerId, TouchLocationState.Moved, pos);
        }

        protected void OnPointerReleased(DrawingSurfaceManipulationHost sender, PointerEventArgs args)
        {
            var pointerPoint = args.CurrentPoint;

            // To convert from DIPs (device independent pixels) to screen resolution pixels.
            var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
            var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * dipFactor;
            TouchPanel.AddEvent((int)pointerPoint.PointerId, TouchLocationState.Released, pos);
        }

        internal void Tick(Device device, DeviceContext context, RenderTargetView renderTargetView)
        {
            var deviceChanged = _device != device || _context != context;
            _device = device;
            _context = context;

            if (!_game.Initialized)
            {
                DrawingSurfaceState.Device = _device;
                DrawingSurfaceState.Context = _context;
                DrawingSurfaceState.RenderTargetView = renderTargetView;
                deviceChanged = false;

                // Start running the game.
                _game.Run(GameRunBehavior.Asynchronous);
            }

            if (deviceChanged)
                _game.GraphicsDevice.UpdateDevice(device, context);
            _game.GraphicsDevice.UpdateTarget(renderTargetView);
            _game.Tick();
        }
    }
}