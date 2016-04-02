// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Gtk;
using Gdk;

namespace MonoGame.Tools.Pipeline
{
    public partial class DeleteDialog : Dialog
    {
        Label label1;
        TextView textView1;
        ScrolledWindow scroll1;

        private void Build()
        {
            this.Title = "Delete Items";
            this.DefaultWidth = 370;
            this.DefaultHeight = 250;

#if GTK3
            var vbox1 = this.ContentArea;
            vbox1.Margin = 4;
#else
            var vbox1 = this.VBox;
#endif
            vbox1.Spacing = 4;

            label1 = new Label("The following items that will be deleted:");
            label1.SetAlignment(0f, 0.5f);
            vbox1.PackStart(label1, false, true, 0);

            scroll1 = new ScrolledWindow();

            textView1 = new TextView();
            textView1.Editable = false;
            textView1.CursorVisible = false;
            scroll1.Add(textView1);

            vbox1.PackStart(scroll1, true, true, 1);

#if GTK3
            var geom = new Geometry();
            geom.MinWidth = this.DefaultWidth;
            geom.MinHeight = this.DefaultHeight;
            this.SetGeometryHints(this, geom, WindowHints.MinSize);
#endif

            this.ShowAll();
        }
    }
}

