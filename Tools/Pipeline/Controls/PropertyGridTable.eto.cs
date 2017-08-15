// MonoGame - Copyright (C) The MonoGame Team
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

            pixel1 = new PixelLayout();
            pixel1.BackgroundColor = DrawInfo.BackColor;

            drawable = new Drawable();
            drawable.Height = 100;
            pixel1.Add(drawable, 0, 0);

            Content = pixel1;

            pixel1.Style = "Stretch";
            drawable.Style = "Stretch";

#if MONOMAC
            drawable.Width = 10;
#endif

            drawable.Paint += Drawable_Paint;
            drawable.MouseDown += Drawable_MouseDown;
            drawable.MouseUp += Drawable_MouseUp;
            drawable.MouseMove += Drawable_MouseMove;
            drawable.MouseLeave += Drawable_MouseLeave;
            SizeChanged += PropertyGridTable_SizeChanged;
        }
    }
}

