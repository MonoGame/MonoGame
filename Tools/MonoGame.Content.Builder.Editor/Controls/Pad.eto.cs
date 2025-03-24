// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto.Drawing;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
#if GTK
    public partial class Pad : GroupBox
#else
    public partial class Pad : Panel
#endif
    {
        private DynamicLayout layout;
        private StackLayout stack;
        private ImageView imageSettings;
        private Panel panelLabel;
        private Label label;

        private void InitializeComponent()
        {
            layout = new DynamicLayout();

            panelLabel = new Panel();
            panelLabel.Padding = new Padding(5);

            if (!Global.IsGtk)
                panelLabel.Height = 25;

            stack = new StackLayout();
            stack.Orientation = Orientation.Horizontal;

            label = new Label();
            label.Font = new Font(label.Font.Family, label.Font.Size - 1, FontStyle.Bold);
            stack.Items.Add(new StackLayoutItem(label, true));

            imageSettings = new ImageView();
            imageSettings.Image = Global.GetEtoIcon("Icons.Settings.png");
            imageSettings.Visible = false;
            stack.Items.Add(new StackLayoutItem(imageSettings, false)); 

            panelLabel.Content = stack;

            layout.AddRow(panelLabel);

            Content = layout;

            imageSettings.MouseDown += ImageSettings_MouseDown;
        }
    }
}

