// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto;
using Eto.WinForms.Forms.Controls;
using Eto.WinForms.Forms.Menu;
using Eto.WinForms.Forms.ToolBar;

namespace MonoGame.Tools.Pipeline
{
    public static class Styles
    {
        public static void Load()
        {
            Style.Add<LabelHandler>("Wrap", h => h.Control.MaximumSize = new System.Drawing.Size(400, 0));
            Style.Add<GridViewHandler>("GridView", h =>
            {
                h.Control.BackgroundColor = System.Drawing.SystemColors.Window;
                h.Control.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            });
            Style.Add<MenuBarHandler>("MenuBar", h => h.Control.BackColor = System.Drawing.SystemColors.Window);
            Style.Add<ToolBarHandler>("ToolBar", h => h.Control.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden );
        }
    }
}
