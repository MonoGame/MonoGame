#region License
/*
Microsoft Public License (Ms-PL)
XnaTouch - Copyright © 2009 The XnaTouch Team

All rights reserved.

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
accept the license, do not use the software.

1. Definitions
The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.
A "contributor" is any person that distributes its contribution under this license.
"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights
(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations
(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
your patent license from such contributor to the software ends automatically.
(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
notices that are present in the software.
(D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
code form, you may only do so under a license that complies with this license.
(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
purpose and non-infringement.
*/
#endregion License

#region Using Statements
using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input;
using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;


#endregion Using Statements

namespace Microsoft.Xna.Framework
{
    public static class KeyExtensions
    {
        public static Microsoft.Xna.Framework.Input.Keys ToXna(this OpenTK.Input.Key key)
        {
            switch (key)
            {
                case OpenTK.Input.Key.A:
                    return Input.Keys.A;
                    break;
                case OpenTK.Input.Key.AltLeft:
                    return Input.Keys.LeftAlt;
                    break;
                case OpenTK.Input.Key.AltRight:
                    return Input.Keys.RightAlt;
                    break;
                case OpenTK.Input.Key.B:
                    return Input.Keys.B;
                    break;
                case OpenTK.Input.Key.Back:
                    return Input.Keys.Back;
                    break;
                case OpenTK.Input.Key.BackSlash:
                    return Input.Keys.OemBackslash;
                    break;
                case OpenTK.Input.Key.BracketLeft:
                    return Input.Keys.OemOpenBrackets;
                    break;
                case OpenTK.Input.Key.BracketRight:
                    return Input.Keys.OemCloseBrackets;
                    break;
                case OpenTK.Input.Key.C:
                    return Input.Keys.C;
                    break;
                case OpenTK.Input.Key.CapsLock:
                    return Input.Keys.CapsLock;
                    break;
                case OpenTK.Input.Key.Clear:
                    return Input.Keys.OemClear;
                    break;
                case OpenTK.Input.Key.Comma:
                    return Input.Keys.OemComma;
                    break;
                case OpenTK.Input.Key.ControlLeft:
                    return Input.Keys.LeftControl;
                    break;
                case OpenTK.Input.Key.ControlRight:
                    return Input.Keys.RightControl;
                    break;
                case OpenTK.Input.Key.D:
                    return Input.Keys.D;
                    break;
                case OpenTK.Input.Key.Delete:
                    return Input.Keys.Delete;
                    break;
                case OpenTK.Input.Key.Down:
                    return Input.Keys.Down;
                    break;
                case OpenTK.Input.Key.E:
                    return Input.Keys.E;
                    break;
                case OpenTK.Input.Key.End:
                    return Input.Keys.End;
                    break;
                case OpenTK.Input.Key.Enter:
                    return Input.Keys.Enter;
                    break;
                case OpenTK.Input.Key.Escape:
                    return Input.Keys.Escape;
                    break;
                case OpenTK.Input.Key.F:
                    return Input.Keys.F;
                    break;
                case OpenTK.Input.Key.F1:
                    return Input.Keys.F1;
                    break;
                case OpenTK.Input.Key.F10:
                    return Input.Keys.F10;
                    break;
                case OpenTK.Input.Key.F11:
                    return Input.Keys.F11;
                    break;
                case OpenTK.Input.Key.F12:
                    return Input.Keys.F12;
                    break;
                case OpenTK.Input.Key.F13:
                    return Input.Keys.F13;
                    break;
                case OpenTK.Input.Key.F14:
                    return Input.Keys.F14;
                    break;
                case OpenTK.Input.Key.F15:
                    return Input.Keys.F15;
                    break;
                case OpenTK.Input.Key.F16:
                    return Input.Keys.F16;
                    break;
                case OpenTK.Input.Key.F17:
                    return Input.Keys.F17;
                    break;
                case OpenTK.Input.Key.F18:
                    return Input.Keys.F18;
                    break;
                case OpenTK.Input.Key.F19:
                    return Input.Keys.F19;
                    break;
                case OpenTK.Input.Key.F2:
                    return Input.Keys.F2;
                    break;
                case OpenTK.Input.Key.F20:
                    return Input.Keys.F20;
                    break;
                case OpenTK.Input.Key.F21:
                    return Input.Keys.F21;
                    break;
                case OpenTK.Input.Key.F22:
                    return Input.Keys.F22;
                    break;
                case OpenTK.Input.Key.F23:
                    return Input.Keys.F23;
                    break;
                case OpenTK.Input.Key.F24:
                    return Input.Keys.F24;
                    break;
                case OpenTK.Input.Key.F25:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.F26:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.F27:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.F28:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.F29:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.F3:
                    return Input.Keys.F3;
                    break;
                case OpenTK.Input.Key.F30:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.F31:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.F32:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.F33:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.F34:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.F35:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.F4:
                    return Input.Keys.F4;
                    break;
                case OpenTK.Input.Key.F5:
                    return Input.Keys.F5;
                    break;
                case OpenTK.Input.Key.F6:
                    return Input.Keys.F6;
                    break;
                case OpenTK.Input.Key.F7:
                    return Input.Keys.F7;
                    break;
                case OpenTK.Input.Key.F8:
                    return Input.Keys.F8;
                    break;
                case OpenTK.Input.Key.F9:
                    return Input.Keys.F9;
                    break;
                case OpenTK.Input.Key.G:
                    return Input.Keys.G;
                    break;
                case OpenTK.Input.Key.H:
                    return Input.Keys.H;
                    break;
                case OpenTK.Input.Key.Home:
                    return Input.Keys.Home;
                    break;
                case OpenTK.Input.Key.I:
                    return Input.Keys.I;
                    break;
                case OpenTK.Input.Key.Insert:
                    return Input.Keys.Insert;
                    break;
                case OpenTK.Input.Key.J:
                    return Input.Keys.J;
                    break;
                case OpenTK.Input.Key.K:
                    return Input.Keys.K;
                    break;
                case OpenTK.Input.Key.Keypad0:
                    return Input.Keys.NumPad0;
                    break;
                case OpenTK.Input.Key.Keypad1:
                    return Input.Keys.NumPad1;
                    break;
                case OpenTK.Input.Key.Keypad2:
                    return Input.Keys.NumPad2;
                    break;
                case OpenTK.Input.Key.Keypad3:
                    return Input.Keys.NumPad3;
                    break;
                case OpenTK.Input.Key.Keypad4:
                    return Input.Keys.NumPad4;
                    break;
                case OpenTK.Input.Key.Keypad5:
                    return Input.Keys.NumPad5;
                    break;
                case OpenTK.Input.Key.Keypad6:
                    return Input.Keys.NumPad6;
                    break;
                case OpenTK.Input.Key.Keypad7:
                    return Input.Keys.NumPad7;
                    break;
                case OpenTK.Input.Key.Keypad8:
                    return Input.Keys.NumPad8;
                    break;
                case OpenTK.Input.Key.Keypad9:
                    return Input.Keys.NumPad9;
                    break;
                case OpenTK.Input.Key.KeypadAdd:
                    return Input.Keys.Add;
                    break;
                case OpenTK.Input.Key.KeypadDecimal:
                    return Input.Keys.Decimal;
                    break;
                case OpenTK.Input.Key.KeypadDivide:
                    return Input.Keys.Divide;
                    break;
                case OpenTK.Input.Key.KeypadEnter:
                    return Input.Keys.Enter;
                    break;
                case OpenTK.Input.Key.KeypadMinus:
                    return Input.Keys.OemMinus;
                    break;
                case OpenTK.Input.Key.KeypadMultiply:
                    return Input.Keys.Multiply;
                    break;
                case OpenTK.Input.Key.L:
                    return Input.Keys.L;
                    break;
                case OpenTK.Input.Key.LShift:
                    return Input.Keys.LeftShift;
                    break;
                case OpenTK.Input.Key.LWin:
                    return Input.Keys.LeftWindows;
                    break;
                case OpenTK.Input.Key.Left:
                    return Input.Keys.Left;
                    break;
                case OpenTK.Input.Key.M:
                    return Input.Keys.M;
                    break;
                case OpenTK.Input.Key.Minus:
                    return Input.Keys.OemMinus;
                    break;
                case OpenTK.Input.Key.N:
                    return Input.Keys.N;
                    break;
                case OpenTK.Input.Key.NumLock:
                    return Input.Keys.NumLock;
                    break;
                case OpenTK.Input.Key.Number0:
                    return Input.Keys.D1;
                    break;
                case OpenTK.Input.Key.Number1:
                    return Input.Keys.D1;
                    break;
                case OpenTK.Input.Key.Number2:
                    return Input.Keys.D2;
                    break;
                case OpenTK.Input.Key.Number3:
                    return Input.Keys.D3;
                    break;
                case OpenTK.Input.Key.Number4:
                    return Input.Keys.D4;
                    break;
                case OpenTK.Input.Key.Number5:
                    return Input.Keys.D5;
                    break;
                case OpenTK.Input.Key.Number6:
                    return Input.Keys.D6;
                    break;
                case OpenTK.Input.Key.Number7:
                    return Input.Keys.D7;
                    break;
                case OpenTK.Input.Key.Number8:
                    return Input.Keys.D8;
                    break;
                case OpenTK.Input.Key.Number9:
                    return Input.Keys.D9;
                    break;
                case OpenTK.Input.Key.O:
                    return Input.Keys.O;
                    break;
                case OpenTK.Input.Key.P:
                    return Input.Keys.P;
                    break;
                case OpenTK.Input.Key.PageDown:
                    return Input.Keys.PageDown;
                    break;
                case OpenTK.Input.Key.PageUp:
                    return Input.Keys.PageUp;
                    break;
                case OpenTK.Input.Key.Pause:
                    return Input.Keys.Pause;
                    break;
                case OpenTK.Input.Key.Period:
                    return Input.Keys.OemPeriod;
                    break;
                case OpenTK.Input.Key.Plus:
                    return Input.Keys.OemPlus;
                    break;
                case OpenTK.Input.Key.PrintScreen:
                    return Input.Keys.PrintScreen;
                    break;
                case OpenTK.Input.Key.Q:
                    return Input.Keys.Q;
                    break;
                case OpenTK.Input.Key.Quote:
                    return Input.Keys.OemQuotes;
                    break;
                case OpenTK.Input.Key.R:
                    return Input.Keys.R;
                    break;
                case OpenTK.Input.Key.Right:
                    return Input.Keys.Right;
                    break;
                case OpenTK.Input.Key.S:
                    return Input.Keys.S;
                    break;
                case OpenTK.Input.Key.ScrollLock:
                    return Input.Keys.Scroll;
                    break;
                case OpenTK.Input.Key.Semicolon:
                    return Input.Keys.OemSemicolon;
                    break;
                case OpenTK.Input.Key.Slash:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.Sleep:
                    return Input.Keys.Sleep;
                    break;
                case OpenTK.Input.Key.Space:
                    return Input.Keys.Space;
                    break;
                case OpenTK.Input.Key.T:
                    return Input.Keys.T;
                    break;
                case OpenTK.Input.Key.Tab:
                    return Input.Keys.Tab;
                    break;
                case OpenTK.Input.Key.Tilde:
                    return Input.Keys.OemTilde;
                    break;
                case OpenTK.Input.Key.U:
                    return Input.Keys.U;
                    break;
                case OpenTK.Input.Key.Unknown:
                    return Input.Keys.None;
                    break;
                case OpenTK.Input.Key.Up:
                    return Input.Keys.Up;
                    break;
                case OpenTK.Input.Key.V:
                    return Input.Keys.V;
                    break;
                case OpenTK.Input.Key.W:
                    return Input.Keys.W;
                    break;
                case OpenTK.Input.Key.X:
                    return Input.Keys.X;
                    break;
                case OpenTK.Input.Key.Y:
                    return Input.Keys.Y;
                    break;
                case OpenTK.Input.Key.Z:
                    return Input.Keys.Z;
                    break;
                default:
                    return Input.Keys.None;
                    break;
            }
        }
    }

    internal class WindowsGameWindow : GameWindow
    {
		private Rectangle clientBounds;
		private GameTime _updateGameTime;
        private GameTime _drawGameTime;
        private DateTime _lastUpdate;
		private DateTime _now;
        private static List<Microsoft.Xna.Framework.Input.Keys> keys = new List<Input.Keys>();

        public static List<Microsoft.Xna.Framework.Input.Keys> Keys
        {
            get
            {
                return keys;
            }
        }

        internal Game Game;
        internal OpenTK.GameWindow OpenTkGameWindow { get; private set; }

        public WindowsGameWindow() 
        {
            Initialize();
        }

        private void Initialize()
        {
            OpenTkGameWindow = new OpenTK.GameWindow();
            OpenTkGameWindow.RenderFrame += OnRenderFrame;
            OpenTkGameWindow.UpdateFrame += OnUpdateFrame;
            OpenTkGameWindow.Resize += OnResize;
            OpenTkGameWindow.Keyboard.KeyDown += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyDown);
            OpenTkGameWindow.Keyboard.KeyUp += new EventHandler<OpenTK.Input.KeyboardKeyEventArgs>(Keyboard_KeyUp);
            clientBounds = new Rectangle(0, 0, OpenTkGameWindow.Width, OpenTkGameWindow.Height);

            // Initialize GameTime
            _updateGameTime = new GameTime();
            _drawGameTime = new GameTime();

            // Initialize _lastUpdate
            _lastUpdate = DateTime.Now;

            //Default no resizing
            AllowUserResizing = false;
        }

        void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (keys.Contains(e.Key.ToXna())) keys.Remove(e.Key.ToXna());
        }

        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (!keys.Contains(e.Key.ToXna())) keys.Add(e.Key.ToXna());
        }

        
        #region GameWindow Methods

        private void OnResize(object sender, EventArgs e)
        {
            Game.GraphicsDevice.SizeChanged(OpenTkGameWindow.ClientRectangle.Width, OpenTkGameWindow.ClientRectangle.Height);
            OnClientSizeChanged();
        }

        private void OnRenderFrame(object sender, FrameEventArgs e)
        {
            if (GraphicsContext.CurrentContext == null || GraphicsContext.CurrentContext.IsDisposed)
                return;

            //Should not happen at all..
            if (!GraphicsContext.CurrentContext.IsCurrent)
                OpenTkGameWindow.MakeCurrent();

            if (Game != null) {
                _drawGameTime.Update(_now - _lastUpdate);
                _lastUpdate = _now;
                Game.DoDraw(_drawGameTime);
            }

            OpenTkGameWindow.SwapBuffers();
        }

        private void OnUpdateFrame(object sender, FrameEventArgs e)
		{			
			if (Game != null ) {
			  
                HandleInput();

                _now = DateTime.Now;
				_updateGameTime.Update(_now - _lastUpdate);
            	Game.DoUpdate(_updateGameTime);
			}
		}

        private void HandleInput()
        {
            Mouse.SetPosition(OpenTkGameWindow.Mouse.X, OpenTkGameWindow.Mouse.Y);                
        }
		
		#endregion

        public override IntPtr Handle
        {
            get { return IntPtr.Zero; }
        }

        public override string ScreenDeviceName 
		{
			get { return string.Empty; }
		}

		public override Rectangle ClientBounds 
		{
			get 
			{
				return clientBounds;
			}
		}

        protected override void SetTitle(string title)
        {
            OpenTkGameWindow.Title = title;
        }

        public new string Title
        {
            get { return OpenTkGameWindow.Title; }
            set { SetTitle(value); }
        }

        private bool _allowUserResizing;
        public override bool AllowUserResizing
        {
            get { return _allowUserResizing; }
            set
            {
                _allowUserResizing = value;

                if (_allowUserResizing)
                    OpenTkGameWindow.WindowBorder = WindowBorder.Resizable;
                else OpenTkGameWindow.WindowBorder = WindowBorder.Fixed;
            }
        }

        private DisplayOrientation _currentOrientation;
		public override DisplayOrientation CurrentOrientation 
		{
            get
            {
                return _currentOrientation;
            }
            internal set
            {
                if (value != _currentOrientation)
                {
                    _currentOrientation = value;
                    OnOrientationChanged();
                }
            }
		}

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
           
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {
           
        }
    }
}

