// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Eto;
using Eto.WinForms.Forms.Controls;
using Eto.WinForms.Forms.Menu;
using Eto.WinForms.Forms.ToolBar;
using Microsoft.Win32;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MonoGame.Tools.Pipeline
{
    static class NativeMethods
    {
        [DllImport("dwmapi.dll", EntryPoint = "#127")]
        internal static extern void DwmGetColorizationParameters(ref DwmColorizationParams param);

        public struct DwmColorizationParams
        {
            public uint ColorizationColor;
            public uint ColorizationAfterglow;
            public uint ColorizationColorBalance;
            public uint ColorizationAfterglowBalance;
            public uint ColorizationBlurBalance;
            public uint ColorizationGlassReflectionIntensity;
            public uint ColorizationOpaqueBlend;
        }
    }

    public static class Styles
    {
        public static Color GetColor(uint value)
        {
            return Color.FromArgb
            (
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
            );
        }

        public static void Load()
        {
            Style.Add<LabelHandler>("Wrap", h => h.Control.MaximumSize = new Size(400, 0));
            Style.Add<GridViewHandler>("GridView", h =>
            {
                h.Control.BackgroundColor = SystemColors.Window;
                h.Control.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            });
            Style.Add<MenuBarHandler>("MenuBar", h =>
            {
                if (Global.IsWindows10)
                {
                    var param = new NativeMethods.DwmColorizationParams();
                    NativeMethods.DwmGetColorizationParameters(ref param);

                    var backcolor = GetColor(param.ColorizationColor);
                    var textcolor = backcolor.GetBrightness() > 0.4 ? Color.Black : Color.White;

                    h.Control.FindForm().Activated += delegate
                    {
                        h.Control.BackColor = backcolor;
                        h.Control.ForeColor = textcolor;
                    };

                    h.Control.FindForm().Deactivate += delegate
                    {
                        h.Control.BackColor = Color.White;
                        h.Control.ForeColor = SystemColors.InactiveCaptionText;
                    };
                }
            });
            Style.Add<ToolBarHandler>("ToolBar", h =>
            {
                h.Control.Padding = new System.Windows.Forms.Padding(4);
                h.Control.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
                h.Control.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            });
            Style.Add<TreeViewHandler>("FilterView", h =>
            {
                h.Control.ItemHeight = 20;
                h.Control.ShowLines = false;
                h.Control.FullRowSelect = true;
            });
        }

        private static void Control_LostFocus(object sender, System.EventArgs e)
        {

        }
    }
}
