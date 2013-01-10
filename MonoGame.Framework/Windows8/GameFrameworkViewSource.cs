using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Activation;
using Microsoft.Xna.Framework;
using System;


namespace MonoGame.Framework
{
    public class GameFrameworkViewSource<T> : IFrameworkViewSource
        where T : Game, new()
    {
        private Action<T, IActivatedEventArgs> _gameConstructorCustomizationDelegate = null;

        public GameFrameworkViewSource(Action<T, IActivatedEventArgs> gameConstructorCustomizationDelegate = null)
        {
            this._gameConstructorCustomizationDelegate = gameConstructorCustomizationDelegate;
        }

        public IFrameworkView CreateView()
        {
            return new MetroFrameworkView<T>(_gameConstructorCustomizationDelegate);
        }
    }
}
