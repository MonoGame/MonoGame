using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    static class FontHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct ABC
        {
            public int abcA;
            public uint abcB;
            public int abcC;
        }

        [DllImport("gdi32.dll", ExactSpelling = true)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern int DeleteObject(IntPtr hObj);

        [DllImport("gdi32.dll", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool GetCharABCWidthsW(IntPtr hdc, uint uFirstChar, uint uLastChar, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct, SizeConst = 1)] ABC[] lpabc);

        public static ABC GetCharWidthABC(char ch, Font font, System.Drawing.Graphics gr)
        {
            ABC[] _temp = new ABC[1];
            IntPtr hDC = gr.GetHdc();
            Font ft = (Font)font.Clone();
            IntPtr hFt = ft.ToHfont();
            SelectObject(hDC, hFt);
            GetCharABCWidthsW(hDC, ch, ch, _temp);
            DeleteObject(hFt);
            gr.ReleaseHdc();

            return _temp[0];
        }

    }
}
