// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto;
using Eto.Wpf.Forms;
using Eto.Wpf.Forms.Menu;
using Eto.Wpf.Forms.ToolBar;

namespace MonoGame.Tools.Pipeline
{
    public static class Styles
    {
        public static void Load()
        {
            Style.Add<MenuBarHandler>("MenuBar", h =>
            {
                h.Control.Background = System.Windows.SystemColors.ControlLightLightBrush;
            });

            Style.Add<ToolBarHandler>("ToolBar", h =>
            {
                h.Control.Background = System.Windows.SystemColors.ControlLightLightBrush;

                h.Control.Loaded += delegate
                {
                    var overflowGrid = h.Control.Template.FindName("OverflowGrid", h.Control) as System.Windows.FrameworkElement;

                    if (overflowGrid != null)
                        overflowGrid.Visibility = System.Windows.Visibility.Collapsed;

                    foreach(var item in h.Control.Items)
                    {
                        var i = item as System.Windows.Controls.Button;

                        if (i != null)
                        {
                            i.Opacity = i.IsEnabled ? 1.0 : 0.2;
                            i.IsEnabledChanged += (sender, e) => i.Opacity = i.IsEnabled ? 1.0 : 0.2;
                        }
                    }
                };
            });
        }
    }
}
