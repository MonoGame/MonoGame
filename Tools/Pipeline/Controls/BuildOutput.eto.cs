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
        Scrollable scrollable1;
        Drawable drawable;

        private void InitializeComponent()
        {
            Title = "Build Output";

            panel = new Panel();

            textArea = new TextArea();
            textArea.Wrap = false;
            textArea.ReadOnly = true;

            scrollable1 = new Scrollable();
            scrollable1.BackgroundColor = DrawInfo.BackColor;
            scrollable1.ExpandContentWidth = true;
            scrollable1.ExpandContentHeight = true;
            drawable = new Drawable();
            scrollable1.Content = drawable;

            panel.Content = textArea;
            CreateContent(panel);

            drawable.MouseDown += Drawable_MouseDown;
            drawable.MouseMove += Drawable_MouseMove;
            drawable.MouseLeave += Drawable_MouseLeave;
            drawable.SizeChanged += Drawable_SizeChanged;
            drawable.Paint += Drawable_Paint;
            scrollable1.SizeChanged += Scrollable1_SizeChanged;
            scrollable1.Scroll += Scrollable1_Scroll;
        }
    }
}
