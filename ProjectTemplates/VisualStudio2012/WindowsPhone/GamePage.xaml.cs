using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using MonoGame.Framework.WindowsPhone;
using $safeprojectname$.Resources;

namespace $safeprojectname$
{
    public partial class GamePage : PhoneApplicationPage
    {
        private Game1 _game;

        // Constructor
        public GamePage()
        {
            InitializeComponent();

            Loaded += GamePageWP8_Loaded;

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void GamePageWP8_Loaded(object sender, RoutedEventArgs e)
        {
            _game = XamlGame<RumRun>.Create("", XnaSurface);
        }

        private void GamePageWP8_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Microsoft.Xna.Framework.Input.GamePad.OnBackPressed();

            // We should detect if Game.Exit() is called and only do this if it wasn't
            e.Cancel = true;
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}