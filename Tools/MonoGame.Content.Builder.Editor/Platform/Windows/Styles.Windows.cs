﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto;
using Eto.Forms;
using Eto.Wpf.Forms;
using Eto.Wpf.Forms.Menu;
using Eto.Wpf.Forms.ToolBar;

namespace MonoGame.Tools.Pipeline
{
    public static class Styles
    {
        public static void Load()
        {
            Style.Add<FormHandler>("MainWindow", h =>
            {
                var displayBounds = Eto.Forms.Screen.DisplayBounds;

                if(h.Location.X < displayBounds.Left || h.Location.X > displayBounds.Right ||
                   h.Location.Y < displayBounds.Top || h.Location.Y > displayBounds.Bottom)
                {
                    h.WindowState = Eto.Forms.WindowState.Normal;
                    h.Location = new Eto.Drawing.Point(182, 182);
                    h.Size = new Eto.Drawing.Size(900, 550);

                    var splitterEnumerator = h.Widget.Children.GetEnumerator();
                    if( splitterEnumerator.MoveNext() == true && splitterEnumerator.Current is Splitter)
                        ((Splitter)splitterEnumerator.Current).Position = 200;

                    if (splitterEnumerator.MoveNext() == true && splitterEnumerator.Current is Splitter)
                        ((Splitter)splitterEnumerator.Current).Position = 230;
                }
            });

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
