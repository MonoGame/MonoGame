// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    partial class CRDescDialog : DialogBase
    {
        private Button _buttonAdd, _buttonRemove;

        private void InitializeComponent()
        {
            Title = "Character Regions";
            Width = 400;
            Height = 240;
            Resizable = true;

            var hbox = new DynamicLayout();
            hbox.DefaultSpacing = new Size(5, 5);
            hbox.BeginHorizontal();

            _gridView = new GridView();

            hbox.Add(_gridView, true, true);

            var vbox = new StackLayout();
            vbox.Spacing = 5;

            _buttonAdd = new Button();
            _buttonAdd.Width = 10;
            _buttonAdd.Text = "+";
            vbox.Items.Add(new StackLayoutItem(_buttonAdd));

            _buttonRemove = new Button();
            _buttonRemove.Width = 10;
            _buttonRemove.Text = "-";
            vbox.Items.Add(new StackLayoutItem(_buttonRemove));

            hbox.Add(vbox, false, false);

            CreateContent(hbox);
        }
    }
}

