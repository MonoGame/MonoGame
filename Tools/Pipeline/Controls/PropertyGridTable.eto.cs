// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class PropertyGridTable : Scrollable
    {
        Drawable drawable;

        private void InitializeComponent()
        {
            drawable = new Drawable();
            drawable.Style = "Test";
            drawable.Size = new Size(10, -1);

            Content = drawable;
            Style = "NoHorizontal";

            drawable.Paint += Drawable_Paint;
            drawable.MouseDown += Drawable_MouseDown;
            drawable.MouseUp += Drawable_MouseUp;
            drawable.MouseMove += Drawable_MouseMove;
            drawable.MouseLeave += Drawable_MouseLeave;
            drawable.MouseDoubleClick += Drawable_MouseDoubleClick;
            SizeChanged += PropertyGridTable_SizeChanged;
        }
    }
}

