using MonoGame.Framework;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace $rootnamespace$
{
    public partial class GamePage : SwapChainBackgroundPanel
    {
        private Game1 _game;

        // Constructor
        public GamePage()
        {
            InitializeComponent();
        }

		public GamePage(string launchArguments)
        {
            _game = XamlGame<Game1>.Create(launchArguments, Window.Current.CoreWindow, this);
        }
    }
}