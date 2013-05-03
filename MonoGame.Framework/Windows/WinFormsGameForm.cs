using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input.Touch;
using XnaKey = Microsoft.Xna.Framework.Input.Keys;


namespace Microsoft.Xna.Framework.Windows
{
    internal static class MessageExtensions
    {     
        public static int GetPointerId(this Message msg)
        {
            return (short)msg.WParam;
        }

        public static System.Drawing.Point GetPointerLocation(this Message msg)
        {
            var lowword = msg.LParam.ToInt32();

            return new System.Drawing.Point()
                       {
                           X = (short)(lowword),
                           Y = (short)(lowword >> 16),
                       };
        }                
    }

    [System.ComponentModel.DesignerCategory("Code")]
    internal class WinFormsGameForm : Form
    {
        public const int WM_POINTERUP = 0x0247;
        public const int WM_POINTERDOWN = 0x0246;
        public const int WM_POINTERUPDATE = 0x0245;

        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP = 0x0105;

        public const int WM_SYSCOMMAND = 0x0112;

        
        internal List<XnaKey> KeyState { get; set; }

        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern short GetKeyState(int keyCode);

        private static XnaKey KeyTranslate(IntPtr wparam, IntPtr lparam)
        {
            // Detect the extended right hand alt/control/shift keys.
            var extended = (lparam.ToInt32() & (1<<24)) != 0;

            switch ((Keys)wparam.ToInt32())
            {
                case Keys.ControlKey:
                    return extended ? XnaKey.RightControl : XnaKey.LeftControl;

                case Keys.Menu:
                    return extended ? XnaKey.RightAlt : XnaKey.LeftAlt;

                case Keys.ShiftKey:
                    {
                        // We have to ask if the left or right shift is down.
                        return (GetKeyState((int) Keys.LShiftKey) & 0x8000) == 0x8000
                                   ? XnaKey.LeftShift
                                   : XnaKey.RightShift;
                    }

                default:
                    return (XnaKey)wparam;
            }
        }

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            var state = TouchLocationState.Invalid;
           
            switch (m.Msg)
            {
                case WM_SYSCOMMAND:
                    // Disable the system menu from being toggled by
                    // keyboard input so we can process the ALT key.
                    if (m.WParam.ToInt32() == 0xF100) // SC_KEYMENU
                    {
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case WM_SYSKEYDOWN:
                case WM_KEYDOWN:
                    {
                        var key = KeyTranslate(m.WParam, m.LParam);
                        if (KeyState != null && !KeyState.Contains(key))
                            KeyState.Add(key);
                    }
                    break;

                case WM_SYSKEYUP:
                case WM_KEYUP:
                    {
                        var key = KeyTranslate(m.WParam, m.LParam);
                        if (KeyState != null)
                            KeyState.Remove(key);
                    }
                    break;

                case WM_POINTERUP:
                    state = TouchLocationState.Released;
                    break;
                case WM_POINTERDOWN:
                    state = TouchLocationState.Pressed;
                    break;
                case WM_POINTERUPDATE:
                    state = TouchLocationState.Moved;
                    break;
            }

            if (state != TouchLocationState.Invalid)
            {
                var id = m.GetPointerId();

                var position = m.GetPointerLocation();
                position = PointToClient(position);
                var vec = new Vector2(position.X, position.Y);

                TouchPanel.AddEvent(id, state, vec, false);
            }

            base.WndProc(ref m);
        }
    }
}
