using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input.Touch;


namespace Microsoft.Xna.Framework.Windows
{
    public static class MessageExtensions
    {     
        public static int GetPointerId(this Message msg)
        {
            return (int)msg.WParam;
        }

        public static Point GetPointerLocation(this Message msg)
        {
            return new Point()
                       {
                           X = (int)msg.LParam,
                           Y = (int)((long)msg.LParam >> 32),
                       };
        }                
    }

    internal class WinFormsGameForm : Form
    {
        public const int WM_POINTERUP = 0x0247;
        public const int WM_POINTERDOWN = 0x0246;
        public const int WM_POINTERUPDATE = 0x0245;  

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            var state = TouchLocationState.Invalid;

            if (m.Msg == WM_POINTERUP)
                state = TouchLocationState.Released;
            else if (m.Msg == WM_POINTERDOWN)
                state = TouchLocationState.Pressed;
            else if (m.Msg == WM_POINTERUPDATE)
                state = TouchLocationState.Moved;

            if (state != TouchLocationState.Invalid)
            {
                var id = m.GetPointerId();
                var position = m.GetPointerLocation();
                var vec = new Vector2(position.X, position.Y);

                TouchPanel.AddEvent(id, state, vec, false);

                // MSDN: If an application processes this message, it should return zero.
                m.Result = new IntPtr(0);
            }

            base.WndProc(ref m);
        }
    }
}
