using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input.Touch;


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
                position = PointToClient(position);
                var vec = new Vector2(position.X, position.Y);

                TouchPanel.AddEvent(id, state, vec, false);
            }

            base.WndProc(ref m);
        }
    }
}
