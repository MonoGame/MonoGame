// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Framework;


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
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, ref int lParam);

        private readonly WinFormsGameWindow _window;

        public const int WM_MOUSEHWHEEL = 0x020E;
        public const int WM_POINTERUP = 0x0247;
        public const int WM_POINTERDOWN = 0x0246;
        public const int WM_POINTERUPDATE = 0x0245;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_TABLET_QUERYSYSTEMGESTURESTA = (0x02C0 + 12);

        public const int WM_ENTERSIZEMOVE = 0x0231;
        public const int WM_EXITSIZEMOVE = 0x0232;

        public const int WM_SYSCOMMAND = 0x0112;

        /// <summary>
        /// IME Notifications Application: Status Window Closed
        /// </summary>
        public const int IMN_CLOSESTATUSWINDOW = 0x0001;
        /// <summary>
        /// IME Notifications Application: Will Create Status Window
        /// </summary>
        public const int IMN_OPENSTATUSWINDOW = 0x0002;
        /// <summary>
        /// IME Notification Application: Content in Candidate Window Will Change
        /// </summary>
        public const int IMN_CHANGECANDIDATE = 0x0003;
        /// <summary>
        /// IME Notification Application: Candidate Window Closed
        /// </summary>
        public const int IMN_CLOSECANDIDATE = 0x0004;
        /// <summary>
        /// IME Notification Application: Will Open Candidate Window
        /// </summary>
        public const int IMN_OPENCANDIDATE = 0x0005;

        /// <summary>
        /// Set a new extension style
        /// </summary>
        public const int GWL_EXSTYLE = -20;
        /// <summary>
        /// Set up a new application instance handle
        /// </summary>
        public const int GWL_HINSTANCE = -6;
        /// <summary>
        /// Set a new window identifier
        /// </summary>
        public const int GWL_ID = -12;
        /// <summary>
        /// Set a new window style
        /// </summary>
        public const int GWL_STYLE = -16;
        /// <summary>
        /// Sets the 32-bit value associated with the window
        /// Each window has a 32-bit value that is used by the application that created the window
        /// </summary>
        public const int GWL_USERDATA = -21;
        /// <summary>
        /// Set a new handler for the window
        /// </summary>
        public const int GWL_WNDPROC = -4;
        /// <summary>
        /// To change the parent window of a child window,
        /// use the SetParent function
        /// </summary>
        public const int GWL_HWNDPARENT = -8;

        public const int DLGC_WANTALLKEYS = 0x0004;
        public const int WM_GETDLGCODE = 0x0087;

        public const int GCS_COMPSTR = 0x0008;
        public const int GCS_RESULTSTR = 0x0800;

        public const int EM_GETSEL = 0xB0;
        public const int EM_LINEFROMCHAR = 0xC9;
        public const int EM_LINEINDEX = 0xBB;

        public bool AllowAltF4 = true;

        internal bool IsResizing { get; set; }
        

        #region Events

        public event EventHandler<HorizontalMouseWheelEventArgs> MouseHorizontalWheel;

        #endregion

        public WinFormsGameForm(WinFormsGameWindow window)
        {
            _window = window;
        }

        public void CenterOnPrimaryMonitor()
        {
             Location = new System.Drawing.Point(
                 (Screen.PrimaryScreen.WorkingArea.Width  - Width ) / 2,
                 (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
        }

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            var state = TouchLocationState.Invalid;

            switch (m.Msg)
            {
                case WM_TABLET_QUERYSYSTEMGESTURESTA:
                    {
                        // This disables the windows touch helpers, popups, and 
                        // guides that get in the way of touch gameplay.
                        const int flags = 0x00000001 | // TABLET_DISABLE_PRESSANDHOLD
                                          0x00000008 | // TABLET_DISABLE_PENTAPFEEDBACK
                                          0x00000010 | // TABLET_DISABLE_PENBARRELFEEDBACK
                                          0x00000100 | // TABLET_DISABLE_TOUCHUIFORCEON
                                          0x00000200 | // TABLET_DISABLE_TOUCHUIFORCEOFF
                                          0x00008000 | // TABLET_DISABLE_TOUCHSWITCH
                                          0x00010000 | // TABLET_DISABLE_FLICKS
                                          0x00080000 | // TABLET_DISABLE_SMOOTHSCROLLING 
                                          0x00100000; // TABLET_DISABLE_FLICKFALLBACKKEYS
                        m.Result = new IntPtr(flags);
                        return;
                    }
#if (WINDOWS && DIRECTX)
                case WM_KEYDOWN:
                    switch (m.WParam.ToInt32())
                    {
                        case 0x5B:  // Left Windows Key
                        case 0x5C: // Right Windows Key

                            if (_window.IsFullScreen && _window.HardwareModeSwitch)
                                this.WindowState = FormWindowState.Minimized;
 		 
                            break;
                    }
                    break;
#endif
                case WM_SYSCOMMAND:

                    var wParam = m.WParam.ToInt32();

                    if (!AllowAltF4 && wParam == 0xF060 && m.LParam.ToInt32() == 0 && Focused)
                    {
                        m.Result = IntPtr.Zero;
                        return;
                    }

                    // Disable the system menu from being toggled by
                    // keyboard input so we can own the ALT key.
                    if (wParam == 0xF100) // SC_KEYMENU
                    {
                        m.Result = IntPtr.Zero;
                        return;
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
                case WM_MOUSEHWHEEL:
                    var delta = (short)(((ulong)m.WParam >> 16) & 0xffff); ;
                    var handler = MouseHorizontalWheel;

                    if (handler != null)
                        handler(this, new HorizontalMouseWheelEventArgs(delta));
                    break;
                case WM_ENTERSIZEMOVE:
                    IsResizing = true;
                    break;
                case WM_EXITSIZEMOVE:
                    IsResizing = false;
                    break;
#if DIRECTX && WINDOWS
                case WindowMessage.ImeSetContext:
                    if (m.WParam.ToInt32() == 1)
                    {
                        IMM.ImmAssociateContext(Handle, _window.Himc);
                    }
                    break;
                case WindowMessage.InputLanguageChange:
                    // Don't pass this message to base class!
                    return;
                case WM_GETDLGCODE:
                    m.Result = (IntPtr)DLGC_WANTALLKEYS;
                    return;

                case WindowMessage.ImeStartCompostition:
                    m.Result = (IntPtr) 0;
                    CandidateList candidate;
                    IntPtr ptr;

                    var startSize = IMM.ImmGetCompositionString(_window.Himc, GCS_COMPSTR, null, 0);
                    var candidateSize = IMM.ImmGetCandidateList(_window.Himc, 0, IntPtr.Zero, 0);
                    ptr = Marshal.AllocHGlobal(startSize);
                    IMM.ImmGetCandidateList(_window.Himc, 0, ptr, candidateSize);
                    candidate = (CandidateList)Marshal.PtrToStructure(ptr, typeof(CandidateList));
                    Marshal.FreeHGlobal(ptr);
                    var startBuffer = new byte[startSize];
                    IMM.ImmGetCompositionString(_window.Himc, GCS_RESULTSTR, startBuffer, startSize);
                    _window.StartTextComposition(new TextCompositionEventArgs(Encoding.Unicode.GetString(startBuffer),
                        GetCursorPos(_window.Himc), candidate));
                    return;
                case WindowMessage.ImeEndComposition:
                    CandidateList endCandidate;
                    IntPtr endPtr;

                    var endSize = IMM.ImmGetCompositionString(_window.Himc, GCS_COMPSTR, null, 0);
                    var endCandidateSize = IMM.ImmGetCandidateList(_window.Himc, 0, IntPtr.Zero, 0);
                    endPtr = Marshal.AllocHGlobal(endSize);
                    IMM.ImmGetCandidateList(_window.Himc, 0, endPtr, endCandidateSize);
                    endCandidate = (CandidateList)Marshal.PtrToStructure(endPtr, typeof(CandidateList));
                    Marshal.FreeHGlobal(endPtr);

                    var endBuffer = new byte[endSize];
                    IMM.ImmGetCompositionString(_window.Himc, GCS_RESULTSTR, endBuffer, endSize);
                    _window.StopTextComposition(new TextCompositionEventArgs(Encoding.Unicode.GetString(endBuffer),
                        GetCursorPos(_window.Himc), endCandidate));
                    break;
#endif
            }

            if (state != TouchLocationState.Invalid)
            {
                var id = m.GetPointerId();

                var position = m.GetPointerLocation();
                position = PointToClient(position);
                var vec = new Vector2(position.X, position.Y);

                _window.TouchPanelState.AddEvent(id, state, vec, false);
            }

            base.WndProc(ref m);
        }
        
        private Point GetCursorPos(IntPtr hWnd)
        {
            var lParam = 0;
            const int wParam = 0;
            var i = SendMessage(hWnd, EM_GETSEL, wParam, ref lParam);
            var j = i / 65536;
            var lineNo = SendMessage(hWnd, EM_LINEFROMCHAR, j, ref lParam) + 1;
            var k = SendMessage(hWnd, EM_LINEINDEX, -1, ref lParam);
            var colNo = j - k + 1;
            var ret = new Point(lineNo, colNo);
            return ret;
        }
    }
}
