// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Gtk;
using Mono.Unix;

namespace MonoGame.Tools.Pipeline
{
    public partial class CustomFolderDialog
    {
        VBox vbox1;
        HBox hbox1, hbox2;
        Entry entryPath;
        Button buttonBrowse;

        private void Build()
        {
            this.Title = Catalog.GetString("Select Folder");
            this.DefaultWidth = 350;

#if GTK2
            VBox w1 = this.VBox;
#elif GTK3
            Box w1 = this.ContentArea;
#endif
            w1.Spacing = 6;
            w1.BorderWidth = ((uint)(2));

            vbox1 = new VBox();
            vbox1.Spacing = 4;

            hbox1 = new HBox();

            entryPath = new Entry();
            hbox1.PackStart(entryPath, true, true, 0);

            buttonBrowse = new Button("...");
            buttonBrowse.Clicked += ButtonBrowse_Clicked;
            hbox1.PackStart(buttonBrowse, false, false, 1);

            vbox1.Add(hbox1);

            hbox2 = new HBox();

            for (uint i = 0; i < symbols.Length; i++)
            {
                var buttonSymbol = new Button(symbols[i]);
                buttonSymbol.Clicked += ButtonSymbol_Clicked;
                hbox2.PackStart(buttonSymbol, true, true, i);
            }

            vbox1.Add(hbox2);

            w1.Add(vbox1);

            this.AddButton(Catalog.GetString("Ok"), ResponseType.Ok);
            this.AddButton(Catalog.GetString("Cancel"), ResponseType.Cancel);
            this.DefaultResponse = ResponseType.Ok;

#if GTK3
            var geom = new Gdk.Geometry();
            geom.MinWidth = this.DefaultWidth;
            geom.MinHeight = this.DefaultHeight;
            geom.MaxWidth = 1000;
            geom.MaxHeight = this.DefaultHeight;
            this.SetGeometryHints(this, geom, Gdk.WindowHints.MinSize | Gdk.WindowHints.MaxSize);
#endif

            this.Response += OnResponse;
            this.ShowAll();
        }
    }
}

