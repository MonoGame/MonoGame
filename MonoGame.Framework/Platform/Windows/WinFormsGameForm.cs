// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Globalization;
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
        private delegate uint GetReadingString(uint himc, uint uReadingBufLen, StringBuilder lpwReadingBuf, ref int pnErrorIndex, ref bool pfIsVertical, ref uint puMaxReadingLen);
        private delegate bool ShowReadingWindow(uint himc, bool bShow);


        private IntPtr lang;
        private GetReadingString getReadingString;
        private ShowReadingWindow showReadingWindow;
        private readonly WinFormsGameWindow _window;

        #region Win32 Constants
        public const int WM_SETFOCUS                        = 0x0007;
        public const int WM_MOUSEHWHEEL                     = 0x020E;
        public const int WM_POINTERUP                       = 0x0247;
        public const int WM_POINTERDOWN                     = 0x0246;
        public const int WM_POINTERUPDATE                   = 0x0245;
        public const int WM_KEYDOWN                         = 0x0100;
        public const int WM_KEYUP                           = 0x0101;
        public const int WM_SYSKEYDOWN                      = 0x0104;
        public const int WM_SYSKEYUP                        = 0x0105;
        public const int WM_TABLET_QUERYSYSTEMGESTURESTA    = 0x02CC;

        public const int WM_ENTERSIZEMOVE                   = 0x0231;
        public const int WM_EXITSIZEMOVE                    = 0x0232;
        public const int WM_DROPFILES                       = 0x0233;

        public const int WM_SYSCOMMAND                      = 0x0112;

        public const int WM_SETTINGCHANGE                   = 0x001A;


        public const int WM_INPUTLANGCHANGE                 = 0x0051;

        public const int WM_IME_STARTCOMPOSITION            = 0x010D;
        public const int WM_IME_ENDCOMPOSITION              = 0x010E;
        public const int WM_IME_COMPOSITION                 = 0x010F;

        public const int WM_IME_SETCONTEXT                  = 0x0281;
        public const int WM_IME_NOTIFY                      = 0x0282;
        //parm for WM_IME_NOTIFY
            public const int IMN_CLOSESTATUSWINDOW          = 0x0001;
            public const int IMN_OPENSTATUSWINDOW           = 0x0002;
            public const int IMN_CHANGECANDIDATE            = 0x0003;
            public const int IMN_CLOSECANDIDATE             = 0x0004;
            public const int IMN_OPENCANDIDATE              = 0x0005;
            public const int IMN_SETCONVERSIONMODE          = 0x0006;
            public const int IMN_SETSENTENCEMODE            = 0x0007;
            public const int IMN_SETOPENSTATUS              = 0x0008;
            public const int IMN_SETCANDIDATEPOS            = 0x0009;
            public const int IMN_SETCOMPOSITIONFONT         = 0x000A;
            public const int IMN_SETCOMPOSITIONWINDOW       = 0x000B;
            public const int IMN_SETSTATUSWINDOWPOS         = 0x000C;
            public const int IMN_GUIDELINE                  = 0x000D;
            public const int IMN_PRIVATE                    = 0x000E;


        public const int WM_IME_CONTROL                     = 0x0283;
        public const int WM_IME_COMPOSITIONFULL             = 0x0284;
        public const int WM_IME_SELECT                      = 0x0285;
        public const int WM_IME_CHAR                        = 0x0286;

        public const int WM_CHAR                            = 0x0102;
        public const int WM_UNICHAR                         = 0x0109;
        public const int UNICODE_NOCHAR                     = 0xFFFF;

        public const int VK_PROCESSKEY                      = 0xE5;

        // parameter of ImmGetCompositionString
        public const int GCS_COMPREADSTR                    = 0x0001;
        public const int GCS_COMPREADATTR                   = 0x0002;
        public const int GCS_COMPREADCLAUSE                 = 0x0004;
        public const int GCS_COMPSTR                        = 0x0008;
        public const int GCS_COMPATTR                       = 0x0010;
        public const int GCS_COMPCLAUSE                     = 0x0020;
        public const int GCS_CURSORPOS                      = 0x0080;
        public const int GCS_DELTASTART                     = 0x0100;
        public const int GCS_RESULTREADSTR                  = 0x0200;
        public const int GCS_RESULTREADCLAUSE               = 0x0400;
        public const int GCS_RESULTSTR                      = 0x0800;
        public const int GCS_RESULTCLAUSE                   = 0x1000;

        // dwIndex for ImmSetCompositionString API
        public const int SCS_SETSTR                         = (GCS_COMPREADSTR|GCS_COMPSTR);
        public const int SCS_CHANGEATTR                     = (GCS_COMPREADATTR|GCS_COMPATTR);
        public const int SCS_CHANGECLAUSE                   = (GCS_COMPREADCLAUSE|GCS_COMPCLAUSE);
        public const int SCS_SETRECONVERTSTRING             = 0x00010000;
        public const int SCS_QUERYRECONVERTSTRING           = 0x00020000;

        // dwAction for ImmNotifyIME
        public const int NI_OPENCANDIDATE                   = 0x0010;
        public const int NI_CLOSECANDIDATE                  = 0x0011;
        public const int NI_SELECTCANDIDATESTR              = 0x0012;
        public const int NI_CHANGECANDIDATELIST             = 0x0013;
        public const int NI_FINALIZECONVERSIONRESULT        = 0x0014;
        public const int NI_COMPOSITIONSTR                  = 0x0015;
        public const int NI_SETCANDIDATE_PAGESTART          = 0x0016;
        public const int NI_SETCANDIDATE_PAGESIZE           = 0x0017;
        public const int NI_IMEMENUSELECTED                 = 0x0018;

        // dwIndex for ImmNotifyIME/NI_COMPOSITIONSTR
        public const int CPS_COMPLETE                       = 0x0001;
        public const int CPS_CONVERT                        = 0x0002;
        public const int CPS_REVERT                         = 0x0003;
        public const int CPS_CANCEL                         = 0x0004;
        #endregion

        public bool AllowAltF4 = true;

        internal bool IsResizing { get; set; }

        #region Events

        public event EventHandler<HorizontalMouseWheelEventArgs> MouseHorizontalWheel;
        public event EventHandler<EventArgs> SettingChanged;

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
                case WM_SETFOCUS:
                    m.Result = IntPtr.Zero;
                    return;
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
                case WM_KEYDOWN:
                    HandleKeyMessage(ref m);
                    switch (m.WParam.ToInt32())
                    {
                        case 0x5B:  // Left Windows Key
                        case 0x5C: // Right Windows Key

                            if (_window.IsFullScreen && _window.HardwareModeSwitch)
                                this.WindowState = FormWindowState.Minimized;

                            break;
                    }
                    break;

                case WM_INPUTLANGCHANGE:
                    HandleIMELanguageChange();
                    break;
                case WM_SYSKEYDOWN:
                    HandleKeyMessage(ref m);
                    break;
                case WM_KEYUP:
                case WM_SYSKEYUP:
                    HandleKeyMessage(ref m);
                    break;
                case WM_CHAR:
                case WM_UNICHAR:
                case WM_IME_STARTCOMPOSITION:
                case WM_IME_ENDCOMPOSITION:
                case WM_IME_COMPOSITION:
                    HandleCharMessage(ref m);
                    break;
                case WM_DROPFILES:
                    HandleDropMessage(ref m);
                    break;

                case WM_SETTING­CHANGE:
                    HandleSettingChange();
                    break;
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

        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern IntPtr GetKeyboardLayout(uint idThread); //HKL

        [DllImport("Imm32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern uint ImmGetContext(IntPtr hWnd); //HIMC
        [DllImport("Imm32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern bool ImmReleaseContext(IntPtr hWnd, uint himc);
        [DllImport("Imm32.dll", CallingConvention = CallingConvention.Winapi)]
        private static extern bool ImmNotifyIME(uint HIMC, uint dwAction, uint dwIndex, uint dwValue);

        [DllImport("Imm32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        private static extern bool ImmSetCompositionStringW(uint himc, uint dwIndex, string lpComp, uint dwCompLen, string lpRead, uint dwReadLen);

        [DllImport("Imm32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Winapi)]
        private static extern uint ImmGetCompositionStringW(uint himc, uint wd, char[] lpBuf, uint dwBufLen);

        private void HandleIMELanguageChange()
        {
            var hkl = GetKeyboardLayout(0);
            var prveLang = lang & 0xFFFF;
            var nextLang = hkl & 0xFFFF;

            if (prveLang != nextLang)
            {
                lang = hkl;
                uint himc = ImmGetContext(Handle);
                ImmNotifyIME(himc, NI_COMPOSITIONSTR, CPS_CANCEL, 0);
                ImmSetCompositionStringW(himc, SCS_SETSTR, "\0", 2, "\0", 2);
                ImmNotifyIME(himc, NI_CLOSECANDIDATE, 0, 0);
                ImmReleaseContext(Handle, himc);
                getReadingString = null;
                showReadingWindow = null;
                EventHelpers.Raise(this, SettingChanged, EventArgs.Empty);
            }
        }

        private void HandleSettingChange()
        {
            EventHelpers.Raise(this, SettingChanged, EventArgs.Empty);
        }

        void HandleKeyMessage(ref Message m)
        {
            long virtualKeyCode = m.WParam.ToInt64();
            bool extended = (m.LParam.ToInt64() & 0x01000000) != 0;
            long scancode = (m.LParam.ToInt64() & 0x00ff0000) >> 16;
            var key = KeyCodeTranslate(
                (System.Windows.Forms.Keys)virtualKeyCode,
                extended,
                scancode);
            if (Input.KeysHelper.IsKey((int)key))
            {
                switch (m.Msg)
                {
                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                        _window.OnKeyDown(new InputKeyEventArgs(key));
                        break;
                    case WM_KEYUP:
                    case WM_SYSKEYUP:
                        _window.OnKeyUp(new InputKeyEventArgs(key));
                        break;
                    default:
                        break;
                }
            }

        }

        private char hs = '\0';
        void HandleCharMessage(ref Message m)
        {
            if (m.Msg == WM_CHAR || m.Msg == WM_UNICHAR)
            {
                char c = (char)m.WParam;
                if (c >= 0xD800 && c < 0xE000)
                {
                    if (c < 0xDC00)
                    {
                        hs = c;
                    }
                    else
                    {
                        if (hs != 0)
                        {
                            _window.OnTextInput(new TextInputEventArgs((uint)char.ConvertToUtf32(hs, c), 0));
                        }
                        hs = '\0';
                    }
                }
                else
                {
                    _window.OnTextInput(new TextInputEventArgs(c, 0));
                    hs = '\0';
                }
            }
            else
            {
                if ((((uint)m.LParam) & GCS_COMPSTR) != 0)
                {
                    uint himc = ImmGetContext(Handle);
                    uint length = ImmGetCompositionStringW(himc, GCS_COMPSTR, null, 0);
                    if (length != 0)
                    {
                        char[] lpBuf = new char[(int)length >> 1];
                        ImmGetCompositionStringW(himc, GCS_COMPSTR, lpBuf, length);
                        int[] codePoints = StringInfo.ParseCombiningCharacters(new string(lpBuf));
                        uint index = 0;
                        for (int i = 0; i < codePoints.Length; i++)
                        {
                            uint codePoint = (uint)codePoints[i];
                            _window.OnTextEditing(new TextInputEventArgs(codePoint, index));
                            index += (uint)char.ConvertFromUtf32((int)codePoint).Length;
                        }
                    }
                    ImmReleaseContext(Handle, himc);
                }
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern uint DragQueryFileW(IntPtr hDrop, uint iFile,
            char[] lpszFile, uint cch);

        void HandleDropMessage(ref Message m)
        {
            IntPtr hdrop = m.WParam;

            uint count = DragQueryFileW(hdrop, uint.MaxValue, null, 0);

            string[] files = new string[count];
            for (uint i = 0; i < count; i++)
            {
                uint buffSize = DragQueryFileW(hdrop, i, null, int.MaxValue);
                char[] buffer = new char[(int)buffSize];
                DragQueryFileW(hdrop, i, buffer, buffSize);
                files[i] = new string(buffer);
            }

            _window.OnFileDrop(new FileDropEventArgs(files));
            m.Result = IntPtr.Zero;
        }

        private static Microsoft.Xna.Framework.Input.Keys KeyCodeTranslate(
            System.Windows.Forms.Keys keyCode, bool extended, long scancode)
        {
            switch (keyCode)
            {
                // WinForms does not distinguish between left/right keys
                // We have to check for special keys such as control/shift/alt/ etc
                case System.Windows.Forms.Keys.ControlKey:
                    return extended
                        ? Microsoft.Xna.Framework.Input.Keys.RightControl
                        : Microsoft.Xna.Framework.Input.Keys.LeftControl;
                case System.Windows.Forms.Keys.ShiftKey:
                    // left shift is 0x2A, right shift is 0x36. IsExtendedKey is always false.
                    return ((scancode & 0x1FF) == 0x36)
                                ? Microsoft.Xna.Framework.Input.Keys.RightShift
                                : Microsoft.Xna.Framework.Input.Keys.LeftShift;
                // Note that the Alt key is now refered to as Menu.
                case System.Windows.Forms.Keys.Menu:
                case System.Windows.Forms.Keys.Alt:
                    return extended
                        ? Microsoft.Xna.Framework.Input.Keys.RightAlt
                        : Microsoft.Xna.Framework.Input.Keys.LeftAlt;

                default:
                    return (Microsoft.Xna.Framework.Input.Keys)keyCode;
            }
        }
    }
}
