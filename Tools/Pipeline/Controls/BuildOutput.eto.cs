// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class BuildOutput : Pad
    {
        Panel panel;
        TextArea textArea;
        Scrollable scrollable;
        Drawable drawable;
        Eto.Drawing.Point scrollPosition;

        private void InitializeComponent()
        {
            Title = "Build Output";

            panel = new Panel();

            textArea = new TextArea();
            textArea.Wrap = false;
            textArea.ReadOnly = true;

            scrollable = new Scrollable();
            scrollable.BackgroundColor = DrawInfo.BackColor;
            scrollable.ExpandContentWidth = true;
            scrollable.ExpandContentHeight = true;
            drawable = new Drawable();
            scrollable.Content = drawable;

            scrollable.Scroll += Scrollable1_Scroll1;

            panel.Content = textArea;
            CreateContent(panel);

            drawable.MouseDown += Drawable_MouseDown;
            drawable.MouseMove += Drawable_MouseMove;
            drawable.MouseLeave += Drawable_MouseLeave;
            drawable.SizeChanged += Drawable_SizeChanged;
            drawable.Paint += Drawable_Paint;
            scrollable.SizeChanged += Scrollable1_SizeChanged;
            scrollable.Scroll += Scrollable1_Scroll;
        }

        private void Scrollable1_Scroll1(object sender, ScrollEventArgs e)
        {
            scrollPosition = scrollable.ScrollPosition;
        }
    }
}
