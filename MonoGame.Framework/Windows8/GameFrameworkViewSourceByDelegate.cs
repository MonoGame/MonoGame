using Windows.ApplicationModel.Core;
using Microsoft.Xna.Framework;
using System;
using Windows.ApplicationModel.Activation;

namespace MonoGame.Framework
{    
    public class GameFrameworkViewSourceByDelegate<T> : IFrameworkViewSource
        where T : Game
    {
        private Func<CoreApplicationView, IActivatedEventArgs, T> createGameInstanceDelegate;
        public GameFrameworkViewSourceByDelegate(Func<CoreApplicationView, IActivatedEventArgs, T> createGameInstanceDelegate)
        {
            this.createGameInstanceDelegate = createGameInstanceDelegate;
        }

        #region IFrameworkViewSource Members

        public IFrameworkView CreateView()
        {
            return new MetroFrameworkViewByDelegate<T>(this.createGameInstanceDelegate);
        }

        #endregion
    }
}
