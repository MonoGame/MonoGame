// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Microsoft.Xna.Framework
{
    class ModernFrameworkView<T> : IFrameworkView
        where T : Game, new()
    {
        public ModernFrameworkView(Action<T, IActivatedEventArgs> gameConstructorCustomizationDelegate)
        {
            this._gameConstructorCustomizationDelegate = gameConstructorCustomizationDelegate;
        }

        private Action<T, IActivatedEventArgs> _gameConstructorCustomizationDelegate = null;
        private CoreApplicationView _applicationView;
        private T _game;

        public void Initialize(CoreApplicationView applicationView)
        {
            _applicationView = applicationView;

            _applicationView.Activated += ViewActivated;
        }

        private void ViewActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Launch)
            {
                // Save any launch parameters to be parsed by the platform.
                ModernGamePlatform.LaunchParameters = ((LaunchActivatedEventArgs)args).Arguments;
                ModernGamePlatform.PreviousExecutionState = ((LaunchActivatedEventArgs)args).PreviousExecutionState;

                // Construct the game.                
                _game = new T();

                //Initializes it, if delegate was provided
                if (_gameConstructorCustomizationDelegate != null)
                    _gameConstructorCustomizationDelegate(_game, args);

            }
            else if (args.Kind == ActivationKind.Protocol)
            {
                // Save any protocol launch parameters to be parsed by the platform.
                var protocolArgs = args as ProtocolActivatedEventArgs;
                ModernGamePlatform.LaunchParameters = protocolArgs.Uri.AbsoluteUri;
                ModernGamePlatform.PreviousExecutionState = protocolArgs.PreviousExecutionState;

                // Construct the game if it does not exist
                // Protocol can be used to reactivate a suspended game
                if (_game == null)
                {
                    _game = new T();

                    //Initializes it, if delegate was provided
                    if (_gameConstructorCustomizationDelegate != null)
                        _gameConstructorCustomizationDelegate(_game, args);
                }
            }
        }

        public void Load(string entryPoint)
        {
        }

        public void Run()
        {
            // Initialize and run the game.
            _game.Run();
        }
       
        public void SetWindow(CoreWindow window)
        {
            // Initialize the singleton window.
            ModernGameWindow.Instance.Initialize(window, null, ModernGamePlatform.TouchQueue);
        }

        public void Uninitialize()
        {
            // TODO: I have no idea when and if this is
            // called... as of Win8 build 8250 this seems 
            // like its never called.
        }
    }
}
