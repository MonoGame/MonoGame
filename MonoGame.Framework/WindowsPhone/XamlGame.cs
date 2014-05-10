using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Xna.Framework.Input;
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

            private void OnPointerPressed(DrawingSurfaceManipulationHost sender, PointerEventArgs args)
            {
                var pointerPoint = args.CurrentPoint;

                // To convert from DIPs (device independent pixels) to screen resolution pixels.
                var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
                var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * dipFactor;
                TouchPanel.AddEvent((int)pointerPoint.PointerId, TouchLocationState.Pressed, pos);
            }

            private void OnPointerMoved(DrawingSurfaceManipulationHost sender, PointerEventArgs args)
            {
                var pointerPoint = args.CurrentPoint;

                // To convert from DIPs (device independent pixels) to screen resolution pixels.
                var dipFactor = DisplayProperties.LogicalDpi / 96.0f;
                var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * dipFactor;
                TouchPanel.AddEvent((int)pointerPoint.PointerId, TouchLocationState.Moved, pos);
            }

            private void OnPointerReleased(DrawingSurfaceManipulationHost sender, PointerEventArgs args)
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
        /// <param name="page">The XAML page containing the drawing surface to which we render the scene and recieve input events.</param>
        /// <returns></returns>
        /// 
        static public T Create(string launchParameters, PhoneApplicationPage page)
        {
            if (launchParameters == null)
                throw new NullReferenceException("The launch parameters cannot be null!");
            if (page == null)
                throw new NullReferenceException("The page parameter cannot be null!");

            UIElement drawingSurface = page.Content as DrawingSurfaceBackgroundGrid;
            
            MediaElement mediaElement = null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(page.Content); i++)
            {
                var child = VisualTreeHelper.GetChild(page.Content, i);
                if (child is MediaElement)
                    mediaElement = (MediaElement)child;
                else if (drawingSurface == null && child is DrawingSurface)
                    drawingSurface = (DrawingSurface)child;
            }

            if (!(drawingSurface is DrawingSurfaceBackgroundGrid) && !(drawingSurface is DrawingSurface))
                throw new NullReferenceException("The drawing surface could not be found!");

            if (mediaElement == null)
                throw new NullReferenceException("The media element could not be found! Add it to the GamePage.");

            Microsoft.Xna.Framework.Media.MediaPlayer._mediaElement = mediaElement;

            WindowsPhoneGamePlatform.LaunchParameters = launchParameters;
            WindowsPhoneGameWindow.Width = ((FrameworkElement)drawingSurface).ActualWidth;
            WindowsPhoneGameWindow.Height = ((FrameworkElement)drawingSurface).ActualHeight;
            WindowsPhoneGameWindow.Page = page;

            Microsoft.Xna.Framework.Audio.SoundEffect.InitializeSoundEffect();

            page.BackKeyPress += PageOnBackKeyPress;

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
                var drawingSurfaceUpdateHandler = new DrawingSurfaceUpdateHandler(game);
                DrawingSurface ds = (DrawingSurface)drawingSurface;

                RoutedEventHandler onLoadedHandler = (object sender, RoutedEventArgs e) =>
                {
                    if (sender != ds)
                        return;

                    if (initializedSurfaces.ContainsKey(ds) == false)
                    {
                        // Hook-up native component to DrawingSurface
                        ds.SetContentProvider(drawingSurfaceUpdateHandler.ContentProvider);
                        ds.SetManipulationHandler(surfaceTouchHandler);

                        // Make sure surface is not initialized twice...
                        initializedSurfaces.Add(ds, true);
                    }
                };

                // Don't wait for loaded event here since control might
                // be loaded already.
                onLoadedHandler(ds, null);

                ds.Unloaded += OnDrawingSurfaceUnloaded;
                ds.Loaded += onLoadedHandler;
            }

            // Return the constructed, but not initialized game.
            return game;
        }

        private static void PageOnBackKeyPress(object sender, CancelEventArgs e)
        {
            if (!e.Cancel)
            {
                GamePad.Back = true;
                e.Cancel = true;
            }
        }

        private static readonly System.Collections.Generic.Dictionary<DrawingSurface, bool> initializedSurfaces = new System.Collections.Generic.Dictionary<DrawingSurface, bool>();

        private static void OnDrawingSurfaceUnloaded(object sender, RoutedEventArgs e)
        {
            DrawingSurface drawingSurface = (DrawingSurface)sender;
            drawingSurface.SetContentProvider(null);
            drawingSurface.SetManipulationHandler(null);

            initializedSurfaces.Remove(drawingSurface);
        }
    }
}