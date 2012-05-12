using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Microsoft.Xna.Framework
{
    class MetroFrameworkView<T> : IFrameworkView
        where T : Game, new()
    {
        private T _game;

        public MetroFrameworkView()
        {
        }

        public void Initialize(CoreApplicationView applicationView)
        {
        }

        public void Load(string entryPoint)
        {
            // Start the game!
            _game = new T();
        }

        public void Run()
        {
            _game.Run();
        }

        public void SetWindow(CoreWindow window)
        {
            MetroGameWindow.Instance.Initialize(window);
        }

        public void Uninitialize()
        {
        }
    }
}
