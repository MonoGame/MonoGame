using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SharpDX;
using SharpDX.Direct3D11;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Windows.Graphics.Display;
using Windows.Phone.Input.Interop;
using Windows.UI.Core;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace MonoGame.Framework.WindowsPhone
{
    /// <summary>
    /// Static class for initializing a Game object for a XAML application.
    /// </summary>
    /// <typeparam name="T">A class derived from Game.</typeparam>
    public static class XamlGame<T>
        where T : Game, new()
    {
        class SurfaceTouchHandler : IDrawingSurfaceManipulationHandler
        {
            public void SetManipulationHost(DrawingSurfaceManipulationHost manipulationHost)
            {
                manipulationHost.PointerPressed += OnPointerPressed;
                manipulationHost.PointerMoved += OnPointerMoved;
                manipulationHost.PointerReleased += OnPointerReleased;
            }

            private static void OnPointerPressed(DrawingSurfaceManipulationHost sender, PointerEventArgs args)
            {
                var pointerPoint = args.CurrentPoint;

                // To convert from DIPs (device independent pixels) to screen resolution pixels.
                var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
                var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * dipFactor;
                TouchPanel.AddEvent((int)pointerPoint.PointerId, TouchLocationState.Pressed, pos);
            }

            private static void OnPointerMoved(DrawingSurfaceManipulationHost sender, PointerEventArgs args)
            {
                var pointerPoint = args.CurrentPoint;

                // To convert from DIPs (device independent pixels) to screen resolution pixels.
                var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
                var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * dipFactor;
                TouchPanel.AddEvent((int)pointerPoint.PointerId, TouchLocationState.Moved, pos);
            }

            private static void OnPointerReleased(DrawingSurfaceManipulationHost sender, PointerEventArgs args)
            {
                var pointerPoint = args.CurrentPoint;

                // To convert from DIPs (device independent pixels) to screen resolution pixels.
                var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
                var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * dipFactor;
                TouchPanel.AddEvent((int)pointerPoint.PointerId, TouchLocationState.Released, pos);
            }
        }

        class DrawingSurfaceBackgroundContentProvider : DrawingSurfaceBackgroundContentProviderNativeBase                                                        
        {
            private readonly SurfaceUpdateHandler _surfaceUpdateHandler;


            public DrawingSurfaceBackgroundContentProvider(T game)
            {
                _surfaceUpdateHandler = new SurfaceUpdateHandler(game);
            }

            public override void Connect(DrawingSurfaceRuntimeHost host, Device device)
            {
                _surfaceUpdateHandler.Connect(host);
            }

            public override void Disconnect()
            {
                _surfaceUpdateHandler.Disconnect();
            }

            public override void Draw(Device device, DeviceContext context, RenderTargetView renderTargetView)
            {
                _surfaceUpdateHandler.Draw(device, context, renderTargetView);
            }

            public override void PrepareResources(DateTime presentTargetTime, ref Size2F desiredRenderTargetSize)
            {
                _surfaceUpdateHandler.UpdateGameWindowSize(desiredRenderTargetSize);
            }
        }

 
        /// <summary>
        /// Creates your Game class initializing it to worth within a XAML application window.
        /// </summary>
        /// <param name="launchParameters">The command line arguments from launch.</param>
        /// <param name="drawingSurface">The XAML drawing surface to which we render the scene and recieve input events.</param>
        /// <returns></returns>
        static public T Create(string launchParameters, PhoneApplicationPage page, UIElement drawingSurface = null)
        {
            if (launchParameters == null)
                throw new NullReferenceException("The launch parameters cannot be null!");
            if (page == null)
                throw new NullReferenceException("The page parameter cannot be null!");

            if (drawingSurface == null)
                drawingSurface = page.Content;

            if (!(drawingSurface is DrawingSurfaceBackgroundGrid) && !(drawingSurface is DrawingSurface))
                throw new NullReferenceException("The drawing surface could not be found!");
            
            WindowsPhoneGamePlatform.LaunchParameters = launchParameters;
            WindowsPhoneGameWindow.Width = ((FrameworkElement)drawingSurface).ActualWidth;
            WindowsPhoneGameWindow.Height = ((FrameworkElement)drawingSurface).ActualHeight;
            WindowsPhoneGameWindow.Page = page;

            page.BackKeyPress += Microsoft.Xna.Framework.Input.GamePad.GamePageWP8_BackKeyPress;

            // Construct the game.
            var game = new T();
            if (game.graphicsDeviceManager == null)
                throw new NullReferenceException("You must create the GraphicsDeviceManager in the Game constructor!");

            SurfaceTouchHandler surfaceTouchHandler = new SurfaceTouchHandler();

            if (drawingSurface is DrawingSurfaceBackgroundGrid)
            {
                // Hookup the handlers for updates and touch.
                DrawingSurfaceBackgroundGrid drawingSurfaceBackgroundGrid = (DrawingSurfaceBackgroundGrid)drawingSurface;
                drawingSurfaceBackgroundGrid.SetBackgroundContentProvider(new DrawingSurfaceBackgroundContentProvider(game));
                drawingSurfaceBackgroundGrid.SetBackgroundManipulationHandler(surfaceTouchHandler);
            }
            else
            {
                DrawingSurface ds = (DrawingSurface)drawingSurface;
                var drawingSurfaceUpdateHandler = new DrawingSurfaceUpdateHandler(game);

                // Hook-up native component to DrawingSurface
                ds.SetContentProvider(drawingSurfaceUpdateHandler.ContentProvider);
                ds.SetManipulationHandler(surfaceTouchHandler);
            }

            // Return the constructed, but not initialized game.
            return game;
        }
    }
}