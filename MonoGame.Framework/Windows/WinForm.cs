using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;



namespace Microsoft.Xna.Framework.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WinProc
    {
        public const int WM_POINTERUP = 0x0247;
        public const int WM_POINTERDOWN = 0x0246;
        public const int WM_POINTERUPDATE = 0x0245;
        
        public IntPtr handle;
        public uint msg;
        public IntPtr wParam;
        public IntPtr lParam;            
    }           

    internal class WinForm : Form
    {
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {            
            switch (m.Msg)
            {
                case WinProc.WM_POINTERUP:
                    break;
                case WinProc.WM_POINTERDOWN:
                    break;
                case WinProc.WM_POINTERUPDATE:
                    break;
            }

            base.WndProc(ref m);
        }
    }
}
