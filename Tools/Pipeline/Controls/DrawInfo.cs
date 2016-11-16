using System;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    static class DrawInfo
    {
        public static int TextHeight;
        public static Color TextColor, BackColor, HoverTextColor, HoverBackColor, DisabledTextColor, BorderColor;

        static DrawInfo()
        {
            TextHeight = (int)SystemFonts.Default().LineHeight;
            TextColor = SystemColors.ControlText;
            BackColor = SystemColors.ControlBackground;
            HoverTextColor = SystemColors.HighlightText;
            HoverBackColor = SystemColors.Highlight;
            DisabledTextColor = SystemColors.ControlText;
            DisabledTextColor.A = 0.4f;
            BorderColor = Global.Unix ? SystemColors.WindowBackground : SystemColors.Control;
        }

        public static Color GetTextColor(bool selected, bool disabled)
        {
            if (disabled)
                return DisabledTextColor;

            return selected ? HoverTextColor : TextColor;
        }

        public static Color GetBackgroundColor(bool selected)
        {
            return selected ? HoverBackColor : BackColor;
        }
    }
}
