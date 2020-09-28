using System;
using Eto.Drawing;

namespace MonoGame.Tools.Pipeline
{
    static class DrawInfo
    {
        private static bool once;

        public static Font TextFont;
        public static int TextHeight;
        public static Color TextColor, BackColor, HoverTextColor, HoverBackColor, DisabledTextColor, BorderColor;

        static DrawInfo()
        {
            TextFont = SystemFonts.Default();
            TextHeight = (int)SystemFonts.Default().LineHeight;
            TextColor = SystemColors.ControlText;
            BackColor = SystemColors.ControlBackground;
            HoverTextColor = SystemColors.HighlightText;
            HoverBackColor = SystemColors.Highlight;
            DisabledTextColor = SystemColors.ControlText;
            DisabledTextColor.A = 0.6f;
            BorderColor = Global.IsGtk ? SystemColors.WindowBackground : SystemColors.Control;
        }

        public static void SetPixelsPerPoint(Graphics g)
        {
            if (!once && !Global.IsGtk)
            {
                once = true;
                TextHeight = (int)(SystemFonts.Default().LineHeight * g.PixelsPerPoint + 0.5);
            }
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
