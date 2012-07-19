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
    class MetroFrameworkView<T> : IFrameworkView
        where T : Game, new()
    {
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
                MetroGamePlatform.LaunchParameters = ((LaunchActivatedEventArgs)args).Arguments;

                // Construct the game.
                _game = new T();
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
            MetroGameWindow.Instance.Initialize(window);
        }

        public void Uninitialize()
        {
            // TODO: I have no idea when and if this is
            // called... as of Win8 build 8250 this seems 
            // like its never called.
        }
    }
}
