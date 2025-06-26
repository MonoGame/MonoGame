// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Microsoft.Xna.Framework.Input
{
    public partial class MouseCursor
    {
        Cursor _cursor;
        bool _needsDisposing;

        internal Cursor Cursor { get { return _cursor; } }

        private MouseCursor(Cursor cursor, bool needsDisposing = false)
        {
            _cursor = cursor;
            _needsDisposing = needsDisposing;
        }

        private static void PlatformInitalize()
        {
            Arrow = new MouseCursor(Cursors.Arrow);
            IBeam = new MouseCursor(Cursors.IBeam);
            Wait = new MouseCursor(Cursors.WaitCursor);
            Crosshair = new MouseCursor(Cursors.Cross);
            WaitArrow = new MouseCursor(Cursors.AppStarting);
            SizeNWSE = new MouseCursor(Cursors.SizeNWSE);
            SizeNESW = new MouseCursor(Cursors.SizeNESW);
            SizeWE = new MouseCursor(Cursors.SizeWE);
            SizeNS = new MouseCursor(Cursors.SizeNS);
            SizeAll = new MouseCursor(Cursors.SizeAll);
            No = new MouseCursor(Cursors.No);
            Hand = new MouseCursor(Cursors.Hand);
        }

        private static MouseCursor PlatformFromTexture2D(Texture2D texture, int originx, int originy)
        {
            var w = texture.Width;
            var h = texture.Height;
            Cursor cursor = null;
            var bytes = new byte[w * h * 4];
            texture.GetData(bytes);
            var gcHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            // convert ABGR to ARGB
            for (int i = 0; i < bytes.Length; i += 4)
            {
                var r = bytes[i];
                bytes[i] = bytes[i + 2];
                bytes[i + 2] = r;
            }

            try
            {
                using (var bitmap = new Bitmap(w, h, h * 4, PixelFormat.Format32bppArgb, gcHandle.AddrOfPinnedObject()))
                {
                    IconInfo iconInfo = new IconInfo();
                    GetIconInfo(bitmap.GetHicon(), ref iconInfo);
                    iconInfo.xHotspot = originx;
                    iconInfo.yHotspot = originy;
                    iconInfo.fIcon = false;
                    cursor = new Cursor(CreateIconIndirect(ref iconInfo));
                }
            }
            finally
            {
                gcHandle.Free();
            }
            return new MouseCursor(cursor, needsDisposing: true);
        }

        private void PlatformDispose()
        {
            if (_needsDisposing && _cursor != null)
            {
                _cursor.Dispose();
                _cursor = null;
                _needsDisposing = false;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr MaskBitmap;
            public IntPtr ColorBitmap;
        };

        [DllImport("user32.dll")]
        static extern IntPtr CreateIconIndirect([In] ref IconInfo iconInfo);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);
    }
}
