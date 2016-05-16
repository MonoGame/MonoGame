// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class PropertyGridControl : DynamicLayout
    {
        Button btnGroup, btnAbc;
        PropertyGridTable propertyTable;

        private void InitializeComponent()
        {
            BeginVertical();

            var subLayout = new StackLayout();
            subLayout.Orientation = Orientation.Horizontal;

            btnGroup = new Button { Text = "Group" };
            subLayout.Items.Add(new StackLayoutItem(btnGroup, true));

            btnAbc = new Button { Text = "Abc" };
            subLayout.Items.Add(new StackLayoutItem(btnAbc, true));

            Add(subLayout);

            propertyTable = new PropertyGridTable();
            Add(propertyTable);

            btnAbc.Click += BtnAbc_Click;
            btnGroup.Click += BtnGroup_Click;
        }
   }
}

