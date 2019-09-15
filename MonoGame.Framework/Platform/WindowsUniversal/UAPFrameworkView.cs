// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.ApplicationModel.Activation;

namespace Microsoft.Xna.Framework
{
    class UAPFrameworkView<T> : IFrameworkView
        where T : Game, new()
    {
        public UAPFrameworkView(Action<T, IActivatedEventArgs> gameConstructorCustomizationDelegate)
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
                UAPGamePlatform.LaunchParameters = ((LaunchActivatedEventArgs)args).Arguments;
                UAPGamePlatform.PreviousExecutionState = ((LaunchActivatedEventArgs)args).PreviousExecutionState;

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
                UAPGamePlatform.LaunchParameters = protocolArgs.Uri.AbsoluteUri;
                UAPGamePlatform.PreviousExecutionState = protocolArgs.PreviousExecutionState;

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
            UAPGameWindow.Instance.Initialize(window, null, UAPGamePlatform.TouchQueue);
        }

        public void Uninitialize()
        {
            // TODO: I have no idea when and if this is
            // called... as of Win8 build 8250 this seems 
            // like its never called.
        }
    }
}
