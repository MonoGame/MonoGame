using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Microsoft.Xna.Framework.Windows.Input
{
    /// <summary>
    /// Native window class that handles IME.
    /// </summary>
    internal sealed class WinFormsImeHandler : NativeWindow, IImmService, IDisposable
    {
        private ImeCompositionString
            _compstr, _compclause, _compattr,
            _compread, _compreadclause, _compreadattr,
            _resstr, _resclause,
            _resread, _resreadclause;
        private ImeCompositionInt _compcurpos;

        private bool _disposed;

        private IntPtr _context;

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
        /// <param name="imeHandler">WinForms ime handler instance</param>
        /// <param name="handle">Handle of the window</param>
        internal WinFormsImeHandler(Game game)
        {
            this._context = IntPtr.Zero;
            this.Candidates = new string[0];
            this._compcurpos = new ImeCompositionInt(IMM.GCSCursorPos);
            this._compstr = new ImeCompositionString(IMM.GCSCompStr);
            this._compclause = new ImeCompositionString(IMM.GCSCompClause);
            this._compattr = new ImeCompositionString(IMM.GCSCompAttr);
            this._compread = new ImeCompositionString(IMM.GCSCompReadStr);
            this._compreadclause = new ImeCompositionString(IMM.GCSCompReadClause);
            this._compreadattr = new ImeCompositionString(IMM.GCSCompReadAttr);
            this._resstr = new ImeCompositionString(IMM.GCSResultStr);
            this._resclause = new ImeCompositionString(IMM.GCSResultClause);
            this._resread = new ImeCompositionString(IMM.GCSResultReadStr);
            this._resreadclause = new ImeCompositionString(IMM.GCSResultReadClause);

            AssignHandle(game.Window.Handle);
            game.Exiting += (o, e) =>
            {
                this.Dispose();
            };
        }

        /// <summary>
        /// Enable the IME
        /// </summary>
        public void EnableIME()
        {
            IMM.DestroyCaret();
            IMM.CreateCaret(Handle, IntPtr.Zero, 1, 1);

            _context = IMM.ImmGetContext(Handle);
            if (_context != IntPtr.Zero)
            {
                IMM.ImmAssociateContext(Handle, _context);
                IMM.ImmReleaseContext(Handle, _context);
                return;
            }

            // This fix the bug that _context is 0 on fullscreen mode.
            ImeContext.Enable(Handle);
        }

        /// <summary>
        /// Disable the IME
        /// </summary>
        public void DisableIME()
        {
            IMM.DestroyCaret();

            IMM.ImmAssociateContext(Handle, IntPtr.Zero);
        }

        public void SetTextInputRect(Rectangle rect)
        {
            if (!IsTextInputActive)
                return;

            _context = IMM.ImmGetContext(Handle);

            var candidateForm = new IMM.CandidateForm(new IMM.Point(rect.X, rect.Y));
            IMM.ImmSetCandidateWindow(_context, ref candidateForm);
            IMM.SetCaretPos(rect.X, rect.Y);

            IMM.ImmReleaseContext(Handle, _context);
        }

        /// <summary>
        /// Dispose everything
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                ReleaseHandle();
                _disposed = true;
            }
        }

        protected override void WndProc(ref Message msg)
        {
            switch (msg.Msg)
            {
                case IMM.ImeSetContext:
                    if (msg.WParam.ToInt32() == 1)
                    {
                        if (IsTextInputActive)
                            EnableIME();
                        if (!ShowOSImeWindow)
                            msg.LParam = (IntPtr)0;
                    }
                    break;
                case IMM.InputLanguageChange:
                    return;
                case IMM.ImeNotify:
                    IMENotify(msg.WParam.ToInt32());
                    if (!ShowOSImeWindow) return;
                    break;
                case IMM.ImeStartCompostition:
                    IMEStartComposion(msg.LParam.ToInt32());
                    return;
                case IMM.ImeComposition:
                    IMESetContextForAll();
                    IMEComposition(msg.LParam.ToInt32());
                    IMM.ImmReleaseContext(Handle, _context);
                    break;
                case IMM.ImeEndComposition:
                    IMESetContextForAll();
                    IMEEndComposition(msg.LParam.ToInt32());
                    IMM.ImmReleaseContext(Handle, _context);
                    if (!ShowOSImeWindow) return;
                    break;
                case IMM.Char:
                    CharEvent(msg.WParam.ToInt32());
                    break;
            }
            base.WndProc(ref msg);
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

        private void IMESetContextForAll()
        {
            _context = IMM.ImmGetContext(Handle);

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

        private void IMENotify(int WParam)
        {
            switch (WParam)
            {
                case IMM.ImnSetOpenStatus:
                    _context = IMM.ImmGetContext(Handle);
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
            _context = IMM.ImmGetContext(Handle);

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

                    if (TextComposition != null)
                        TextComposition.Invoke(this, new TextCompositionEventArgs(CompositionString, CompositionCursorPos,
                            Candidates, CandidatesPageStart, CandidatesPageSize, CandidatesSelection));
                }
                else
                    IMECloseCandidate();

                Marshal.FreeHGlobal(pointer);
            }

            IMM.ImmReleaseContext(Handle, _context);
        }

        private void IMECloseCandidate()
        {
            CandidatesSelection = CandidatesPageStart = CandidatesPageSize = 0;
            Candidates = new string[0];

            if (TextComposition != null)
                TextComposition.Invoke(this, new TextCompositionEventArgs(CompositionString, CompositionCursorPos));
        }

        private void IMEStartComposion(int lParam)
        {
            ClearComposition();
            ClearResult();

            if (TextComposition != null)
                TextComposition.Invoke(this, new TextCompositionEventArgs(CompositionString, CompositionCursorPos));
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
                    TextComposition.Invoke(this, new TextCompositionEventArgs(CompositionString, CompositionCursorPos));
            }
        }

        private void IMEEndComposition(int lParam)
        {
            ClearComposition();

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

            if (IsTextInputActive)
                IMECloseCandidate();
        }

        #endregion
    }
}
