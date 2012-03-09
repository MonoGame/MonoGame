
using Windows.ApplicationModel.Core;

namespace MonoGame.Framework
{
    public class GameFrameworkViewSource<T> : IFrameworkViewSource
        where T : IFrameworkView, new()
    {
        public IFrameworkView CreateView()
        {
            return new T();
        }
    }
}
