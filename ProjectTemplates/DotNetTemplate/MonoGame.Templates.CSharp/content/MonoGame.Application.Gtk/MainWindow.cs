using System;
using System.Runtime.InteropServices;
using System.Security;
using Gtk;
using Microsoft.Xna.Framework;

namespace MGNamespace
{
    class MainWindow : Window
    {
        private HeaderBar _headerBar;
        private Game1 _game;

        public MainWindow() : base(WindowType.Toplevel)
        {
            DefaultWidth = 640;
            DefaultHeight = 480;

            _headerBar = new HeaderBar();
            _headerBar.ShowCloseButton = true;
            _headerBar.Title = "Example Gtk Game";

            var btnPreferences = new Button();
            btnPreferences.TooltipText = "Fullscreen";
            btnPreferences.Image = Image.NewFromIconName("view-fullscreen-symbolic", IconSize.Button);
            _headerBar.PackEnd(btnPreferences);

            Titlebar = _headerBar;

            var vbox = new VBox();
            vbox.Margin = 4;
            vbox.Spacing = 4;

            var mgwidget = CreateMGWidget();
            vbox.PackStart(mgwidget, true, true, 0);

            var buttonClickMe = new Button();
            buttonClickMe.Label = "CLICK ME! (I do nothing, just like the fullscreen button...)";
            vbox.PackStart(buttonClickMe, false, true, 0);

            Child = vbox;

            Destroyed += MainWindow_Destroyed;
        }

        private Widget CreateMGWidget()
        {
            var vbox = new VBox();
            var dummycontrol = new GLArea();

            vbox.PackStart(dummycontrol, true, true, 0);
            this.Shown += (o, e) =>
            {
                dummycontrol.Context.MakeCurrent();
                _game = new Game1();
                vbox.PackStart(_game.Services.GetService<Widget>(), true, true, 0);
                _game.Run();
                vbox.Remove(dummycontrol);
                vbox.ShowAll();
            };

            return vbox;
        }

        private void MainWindow_Destroyed(object sender, EventArgs e)
        {
            _game.Exit();
            Application.Quit();
        }
    }
}