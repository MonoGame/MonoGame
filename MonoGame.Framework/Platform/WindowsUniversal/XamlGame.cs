// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;


namespace MonoGame.Framework
{
    /// <summary>
    /// Static class for initializing a Game object for a XAML application.
    /// </summary>
    /// <typeparam name="T">A class derived from Game with a public parameterless constructor.</typeparam>
    [CLSCompliant(false)]
    public static class XamlGame<T>
        where T : Game, new()
    {
        /// <summary>
        /// Creates your Game class initializing it to work within a XAML application window.
        /// </summary>
        /// <param name="launchParameters">The command line arguments from launch.</param>
        /// <param name="window">The core window object.</param>
        /// <param name="swapChainPanel">The XAML SwapChainPanel to which we render the scene and receive input events.</param>
        /// <returns>Returns an instance of T generated with default parameterless T constructor</returns>
        static public T Create(string launchParameters, CoreWindow window, SwapChainPanel swapChainPanel)
        {
            return Create(() => new T(), launchParameters, window, swapChainPanel);
        }

        /// <summary>
        /// Creates your Game class initializing it with <paramref name="gameConstructor"/> to work within a XAML application window.
        /// </summary>
        /// <param name="gameConstructor">The method to construct T</param>
        /// <param name="launchParameters">The command line arguments from launch.</param>
        /// <param name="window">The core window object.</param>
        /// <param name="swapChainPanel">The XAML SwapChainPanel to which we render the scene and receive input events.</param>
        /// <returns>Returns an instance of T generated with <paramref name="gameConstructor"/></returns>
        static public T Create(Func<T> gameConstructor, string launchParameters, CoreWindow window, SwapChainPanel swapChainPanel)
        {
            if (gameConstructor == null)
                throw new NullReferenceException(nameof(gameConstructor));
            if (launchParameters == null)
                throw new NullReferenceException("The launch parameters cannot be null!");
            if (window == null)
                throw new NullReferenceException("The window cannot be null!");
            if (swapChainPanel == null)
                throw new NullReferenceException("The swap chain panel cannot be null!");

            // Save any launch parameters to be parsed by the platform.
            UAPGamePlatform.LaunchParameters = launchParameters;

            // Setup the window class.
            UAPGameWindow.Instance.Initialize(window, swapChainPanel, UAPGamePlatform.TouchQueue);

            // Construct the game.
            var game = gameConstructor();

            // Set the swap chain panel on the graphics mananger.
            if (game.graphicsDeviceManager == null)
                throw new NullReferenceException("You must create the GraphicsDeviceManager in the Game constructor!");
            game.graphicsDeviceManager.SwapChainPanel = swapChainPanel;

            // Start running the game.
            game.Run(GameRunBehavior.Asynchronous);

            // Return the created game object.
            return game;
        }

    }
}
