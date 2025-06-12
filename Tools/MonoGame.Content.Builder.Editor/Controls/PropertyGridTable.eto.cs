// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class PropertyGridTable : Scrollable
    {
        PixelLayout pixel1;
        Drawable drawable;

        private void InitializeComponent()
        {
            BackgroundColor = DrawInfo.BackColor;
            ExpandContentWidth = true;

            pixel1 = new PixelLayout();
            pixel1.BackgroundColor = DrawInfo.BackColor;

            drawable = new Drawable();
            drawable.Height = 100;
            pixel1.Add(drawable, 0, 0);

            Content = pixel1;

            drawable.Paint += Drawable_Paint;
            drawable.MouseDown += Drawable_MouseDown;
            drawable.MouseUp += Drawable_MouseUp;
            drawable.MouseMove += Drawable_MouseMove;
            drawable.MouseLeave += Drawable_MouseLeave;
            SizeChanged += PropertyGridTable_SizeChanged;
        }
    }
}
