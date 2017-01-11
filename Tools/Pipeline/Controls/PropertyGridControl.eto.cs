// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class PropertyGridControl : Pad
    {
        DynamicLayout layout;
        StackLayout subLayout;
        PropertyGridTable propertyTable;

        private void InitializeComponent()
        {
            Title = "Properties";

            layout = new DynamicLayout();
            layout.BeginVertical();

            subLayout = new StackLayout();
            subLayout.Orientation = Orientation.Horizontal;

            layout.Add(subLayout);

            propertyTable = new PropertyGridTable();
            layout.Add(propertyTable);

            CreateContent(layout);
        }
   }
}

