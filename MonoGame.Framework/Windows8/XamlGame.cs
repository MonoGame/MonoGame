using System;
using Microsoft.Xna.Framework;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Activation;

namespace MonoGame.Framework
{
    /// <summary>
    /// Static class for initializing a Game object for a XAML application.
    /// </summary>
    /// <typeparam name="T">A class derived from Game.</typeparam>
    [CLSCompliant(false)]
    public static class XamlGame<T>
        where T : Game, new()
    {
        /// <summary>
        /// Creates your Game class initializing it to worth within a XAML application window.
        /// </summary>
        /// <param name="launchParameters">The command line arguments from launch.</param>
        /// <param name="window">The core window object.</param>
        /// <param name="SwapChainBackgroundPanel">The XAML SwapChainBackgroundPanel to which we render the scene and recieve input events.</param>
        /// <returns></returns>
        static public T Create(string launchParameters, CoreWindow window, SwapChainBackgroundPanel swapChainBackgroundPanel)
        {
            if (launchParameters == null)
                throw new NullReferenceException("The launch parameters cannot be null!");
            if (window == null)
                throw new NullReferenceException("The window cannot be null!");
            if (swapChainBackgroundPanel == null)
                throw new NullReferenceException("The swap chain panel cannot be null!");

            // Save any launch parameters to be parsed by the platform.
            MetroGamePlatform.LaunchParameters = launchParameters;

            // Setup the window class.
            MetroGameWindow.Instance.Initialize(window, swapChainBackgroundPanel);

            // Construct the game.
            var game = new T();

            // Set the swap chain panel on the graphics mananger.
            if (game.graphicsDeviceManager == null)
                throw new NullReferenceException("You must create the GraphicsDeviceManager in the Game constructor!");
            game.graphicsDeviceManager.SwapChainBackgroundPanel = swapChainBackgroundPanel;

            // Start running the game.
            game.Run(GameRunBehavior.Asynchronous);

            // Return the created game object.
            return game;
        }
        
        /// <summary>
        /// Preserves the previous execution state in MetroGamePlatform and returns the constructed game object initialized with the given window.
        /// </summary>
        /// <param name="launchParameters">The command line arguments from launch.</param>
        /// <param name="window">The core window object.</param>
        /// <param name="swapChainBackgroundPanel">The XAML SwapChainBackgroundPanel to which we render the scene and recieve input events.</param>
        /// <returns></returns>
        static public T Create(LaunchActivatedEventArgs args, CoreWindow window, SwapChainBackgroundPanel swapChainBackgroundPanel)
        {
            MetroGamePlatform.PreviousExecutionState = args.PreviousExecutionState;

            return Create(args.Arguments, window, swapChainBackgroundPanel);
        }

        static public T Create(ProtocolActivatedEventArgs args, CoreWindow window, SwapChainBackgroundPanel swapChainBackgroundPanel)
        {
            MetroGamePlatform.PreviousExecutionState = args.PreviousExecutionState;

            return Create(args.Uri.AbsoluteUri, window, swapChainBackgroundPanel);
        }
    }
}
