using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Native window class that handles IME.
    /// Related Microsfot IME documentation links:
    /// - https://docs.microsoft.com/en-us/windows/win32/dxtecharts/using-an-input-method-editor-in-a-game
    /// - https://docs.microsoft.com/zh-cn/windows/win32/intl/about-input-method-manager
    /// </summary>
    internal sealed class WinFormsImeHandler : IImmService, IDisposable
    {
        private ImeCompositionString
            _compstr, _compclause, _compattr,
            _compread, _compreadclause, _compreadattr,
            _resstr, _resclause,
            _resread, _resreadclause;
        private ImeCompositionInt _compcurpos;

        private IntPtr _windowHandle;
        private IntPtr _context;
        private bool _disposed;

        private bool _compositionEnded;

        public bool IsTextInputActive { get; private set; }
        public event EventHandler<TextCompositionEventArgs> TextComposition;
        public event EventHandler<TextInputEventArgs> TextInput;

        public void StartTextInput()
        {
            if (IsTextInputActive)
                return;

            IsTextInputActive = true;
            EnableIME();
        }

        public void StopTextInput()
        {
            if (!IsTextInputActive)
                return;

            IsTextInputActive = false;
            DisableIME();
        }

        public bool ShowOSImeWindow { get; set; }

        public int VirtualKeyboardHeight { get { return 0; } }

        public void Update(GameTime gameTime)
        {

        }

        public bool IsIMEOpen { get; private set; }

        /// <summary>
        /// Composition String
        /// </summary>
        public string CompositionString { get { return _compstr.ToString(); } }

        /// <summary>
        /// Composition Clause
        /// </summary>
        public string CompositionClause { get { return _compclause.ToString(); } }

        /// <summary>
        /// Composition String Reads
        /// </summary>
        public string CompositionReadString { get { return _compread.ToString(); } }

        /// <summary>
        /// Composition Clause Reads
        /// </summary>
        public string CompositionReadClause { get { return _compreadclause.ToString(); } }

        /// <summary>
        /// Result String
        /// </summary>
        public string ResultString { get { return _resstr.ToString(); } }

        /// <summary>
        /// Result Clause
        /// </summary>
        public string ResultClause { get { return _resclause.ToString(); } }

        /// <summary>
        /// Result String Reads
        /// </summary>
        public string ResultReadString { get { return _resread.ToString(); } }

        /// <summary>
        /// Result Clause Reads
        /// </summary>
        public string ResultReadClause { get { return _resreadclause.ToString(); } }

        /// <summary>
        /// Caret position of the composition
        /// </summary>
        public int CompositionCursorPos { get { return _compcurpos.Value; } }

        /// <summary>
        /// Array of the candidates
        /// </summary>
        public string[] Candidates { get; private set; }

        /// <summary>
        /// First candidate index of current page
        /// </summary>
        public uint CandidatesPageStart { get; private set; }

        /// <summary>
        /// How many candidates should display per page
        /// </summary>
        public uint CandidatesPageSize { get; private set; }

        /// <summary>
        /// The selected canddiate index
        /// </summary>
        public uint CandidatesSelection { get; private set; }

        /// <summary>
        /// Get the composition attribute at character index.
        /// </summary>
        /// <param name="index">Character Index</param>
        /// <returns>Composition Attribute</returns>
        public ImeCompositionAttributes GetCompositionAttr(int index)
        {
            return (ImeCompositionAttributes)_compattr[index];
        }

        /// <summary>
        /// Get the composition read attribute at character index.
        /// </summary>
        /// <param name="index">Character Index</param>
        /// <returns>Composition Attribute</returns>
        public ImeCompositionAttributes GetCompositionReadAttr(int index)
        {
            return (ImeCompositionAttributes)_compreadattr[index];
        }

        /// <summary>
        /// Constructor, must be called when the window create.
        /// </summary>
        /// <param name="windowHandle">Handle of the window</param>
        internal WinFormsImeHandler(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            _context = IMM.ImmCreateContext();
            IMM.ImmAssociateContext(_windowHandle, _context);

            Candidates = null;

            _compcurpos = new ImeCompositionInt(IMM.GCSCursorPos);
            _compstr = new ImeCompositionString(IMM.GCSCompStr);
            _compclause = new ImeCompositionString(IMM.GCSCompClause);
            _compattr = new ImeCompositionString(IMM.GCSCompAttr);
            _compread = new ImeCompositionString(IMM.GCSCompReadStr);
            _compreadclause = new ImeCompositionString(IMM.GCSCompReadClause);
            _compreadattr = new ImeCompositionString(IMM.GCSCompReadAttr);
            _resstr = new ImeCompositionString(IMM.GCSResultStr);
            _resclause = new ImeCompositionString(IMM.GCSResultClause);
            _resread = new ImeCompositionString(IMM.GCSResultReadStr);
            _resreadclause = new ImeCompositionString(IMM.GCSResultReadClause);

            _compcurpos.ImeContext = _context;
            _compstr.ImeContext = _context;
            _compclause.ImeContext = _context;
            _compattr.ImeContext = _context;
            _compread.ImeContext = _context;
            _compreadclause.ImeContext = _context;
            _compreadattr.ImeContext = _context;
            _resstr.ImeContext = _context;
            _resclause.ImeContext = _context;
            _resread.ImeContext = _context;
            _resreadclause.ImeContext = _context;
        }

        /// <summary>
        /// Enable the IME
        /// </summary>
        public void EnableIME()
        {
            IMM.DestroyCaret();
            IMM.CreateCaret(_windowHandle, IntPtr.Zero, 1, 1);

            IMM.ImmAssociateContext(_windowHandle, _context);

            // This fix the bug that _context is 0 on fullscreen mode.
            //ImeContext.Enable(_windowHandle);
        }

        /// <summary>
        /// Disable the IME
        /// </summary>
        public void DisableIME()
        {
            IMM.DestroyCaret();

            IMM.ImmAssociateContext(_windowHandle, IntPtr.Zero);
        }

        public void SetTextInputRect(Rectangle rect)
        {
            if (!IsTextInputActive)
                return;

            var candidateForm = new IMM.CandidateForm(new IMM.Point(rect.X, rect.Y));
            IMM.ImmSetCandidateWindow(_context, ref candidateForm);
            IMM.SetCaretPos(rect.X, rect.Y);
        }

        internal bool WndProc(ref Message msg)
        {
            switch (msg.Msg)
            {
                case IMM.ImeSetContext:
                    if (IsTextInputActive)
                        IMM.ImmAssociateContext(_windowHandle, _context);
                    else
                        IMM.ImmAssociateContext(_windowHandle, IntPtr.Zero);

                    if (!ShowOSImeWindow)
                        msg.LParam = IntPtr.Zero;
                    break;
                case IMM.InputLanguageChange:
                    return true;
                case IMM.ImeNotify:
                    IMENotify(msg.WParam.ToInt32());
                    if (!ShowOSImeWindow) return true;
                    break;
                case IMM.ImeStartCompostition:
                    _compositionEnded = false;
                    IMEStartComposion(msg.LParam.ToInt32());
                    return true;
                case IMM.ImeComposition:
                    IMEComposition(msg.LParam.ToInt32());
                    break;
                case IMM.ImeEndComposition:
                    _compositionEnded = true;
                    IMEEndComposition(msg.LParam.ToInt32());
                    if (!ShowOSImeWindow) return true;
                    break;
                case IMM.Char:
                    if (IsTextInputActive)
                        CharEvent(msg.WParam.ToInt32());
                    break;
            }

            return false;
        }

        private void ClearComposition()
        {
            _compstr.Clear();
            _compclause.Clear();
            _compattr.Clear();
            _compread.Clear();
            _compreadclause.Clear();
            _compreadattr.Clear();
        }

        private void ClearResult()
        {
            _resstr.Clear();
            _resclause.Clear();
            _resread.Clear();
            _resreadclause.Clear();
        }

        #region IME Message Handlers

        private void IMENotify(int WParam)
        {
            switch (WParam)
            {
                case IMM.ImnSetOpenStatus:
                    IsIMEOpen = IMM.ImmGetOpenStatus(_context);
                    break;
                case IMM.ImnOpenCandidate:
                case IMM.ImnChangeCandidate:
                    IMEChangeCandidate();
                    break;
                case IMM.ImnCloseCandidate:
                    IMECloseCandidate();
                    break;
                case IMM.ImnPrivate:
                    break;
                default:
                    break;
            }
        }

        private void IMEChangeCandidate()
        {
            if (_compositionEnded)
                IMECloseCandidate();
            else
                UpdateCandidates();

            if (TextComposition != null)
                TextComposition.Invoke(this, new TextCompositionEventArgs(CompositionString, CompositionCursorPos,
                    Candidates, CandidatesPageStart, CandidatesPageSize, CandidatesSelection));
        }

        private void UpdateCandidates()
        {
            uint length = IMM.ImmGetCandidateList(_context, 0, IntPtr.Zero, 0);
            if (length > 0)
            {
                IntPtr pointer = Marshal.AllocHGlobal((int)length);
                length = IMM.ImmGetCandidateList(_context, 0, pointer, length);
                IMM.CandidateList cList = (IMM.CandidateList)Marshal.PtrToStructure(pointer, typeof(IMM.CandidateList));

                CandidatesSelection = cList.dwSelection;
                CandidatesPageStart = cList.dwPageStart;
                CandidatesPageSize = cList.dwPageSize;

                if (cList.dwCount > 1)
                {
                    Candidates = new string[cList.dwCount];
                    for (int i = 0; i < cList.dwCount; i++)
                    {
                        int sOffset = Marshal.ReadInt32(pointer, 24 + 4 * i);
                        Candidates[i] = Marshal.PtrToStringUni(pointer + sOffset);
                    }
                }

                Marshal.FreeHGlobal(pointer);
            }
            else
                IMECloseCandidate();
        }

        private void IMECloseCandidate()
        {
            CandidatesSelection = CandidatesPageStart = CandidatesPageSize = 0;
            Candidates = null;
        }

        private void IMEStartComposion(int lParam)
        {
            ClearComposition();
            ClearResult();
        }

        private void IMEComposition(int lParam)
        {
            if (_compstr.Update(lParam))
            {
                _compclause.Update();
                _compattr.Update();
                _compread.Update();
                _compreadclause.Update();
                _compreadattr.Update();
                _compcurpos.Update();

                if (TextComposition != null)
                    TextComposition.Invoke(this, new TextCompositionEventArgs(CompositionString, CompositionCursorPos,
                        Candidates, CandidatesPageStart, CandidatesPageSize, CandidatesSelection));
            }
        }

        private void IMEEndComposition(int lParam)
        {
            ClearComposition();
            IMECloseCandidate();

            if (_resstr.Update(lParam))
            {
                _resclause.Update();
                _resread.Update();
                _resreadclause.Update();
            }

            if (TextComposition != null)
                TextComposition.Invoke(this, new TextCompositionEventArgs(null, 0));
        }

        private void CharEvent(int wParam)
        {
            var charInput = (char)wParam;

            var key = Keys.None;
            if (!char.IsSurrogate(charInput))
                key = (Keys)(IMM.VkKeyScanEx(charInput, InputLanguage.CurrentInputLanguage.Handle) & 0xff);

            if (TextInput != null)
                TextInput.Invoke(this, new TextInputEventArgs(charInput, key));
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                IMM.ImmAssociateContext(_windowHandle, IntPtr.Zero);
                IMM.ImmDestroyContext(_context);

                _disposed = true;
            }
        }

        #endregion
    }
}
