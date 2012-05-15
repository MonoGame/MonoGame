
using Windows.ApplicationModel.Core;
using Microsoft.Xna.Framework;

namespace MonoGame.Framework
{
    public class GameFrameworkViewSource<T> : IFrameworkViewSource
        where T : Game, new()
    {
        public IFrameworkView CreateView()
        {
            return new MetroFrameworkView<T>();
        }
    }
}
