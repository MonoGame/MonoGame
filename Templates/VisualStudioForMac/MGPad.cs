// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto;
using Eto.Drawing;
using Eto.Forms;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoGame.Tools.Pipeline;

namespace MonoGame.IDE.VisualStudioForMac
{
    public class MGPad : PadContent
    {
        private Application _application;
        private MainWindow _window;
        private Gtk.Widget _control;

        public MGPad()
        {
            try
            {
                _application = new Application(Platforms.Gtk2);
                _application.Attach();
            }
            catch
            {
                // Ok, Eto Forms extension already loaded a backend
            }

            DrawInfo.TextFont = new Font(".AppleSystemUIFont", 12);
            DrawInfo.TextHeight = (int)DrawInfo.TextFont.LineHeight;

            if (IdeApp.Preferences.UserInterfaceTheme == Theme.Dark)
            {
                DrawInfo.TextColor = new Color(0.843f, 0.843f, 0.843f);
                DrawInfo.BackColor = new Color(0.251f, 0.251f, 0.251f);
                DrawInfo.HoverTextColor = new Color(1f, 1f, 1f);
                DrawInfo.HoverBackColor = new Color(0.129f, 0.341f, 0.800f);
                DrawInfo.BorderColor = new Color(0.322f, 0.322f, 0.322f);
            }
            else
            {
                DrawInfo.TextColor = new Color(0.153f, 0.153f, 0.153f);
                DrawInfo.BackColor = new Color(1f, 1f, 1f);
                DrawInfo.HoverTextColor = new Color(1f, 1f, 1f);
                DrawInfo.HoverBackColor = new Color(0.129f, 0.341f, 0.800f);
                DrawInfo.BorderColor = new Color(0.871f, 0.871f, 0.871f);
            }

            DrawInfo.DisabledTextColor = DrawInfo.TextColor;
            DrawInfo.DisabledTextColor.A = 0.6f;

            _window = new MainWindow();
            PipelineController.Create(_window);

            _control = _window.ToNative(true);
            _control.ShowAll();
        }

        public override MonoDevelop.Components.Control Control => _control;
    }
}
