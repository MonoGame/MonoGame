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
using System.ComponentModel;
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
                    
                case OpenTK.Input.Key.AltLeft:
                    return Input.Keys.LeftAlt;
                    
                case OpenTK.Input.Key.AltRight:
                    return Input.Keys.RightAlt;
                    
                case OpenTK.Input.Key.B:
                    return Input.Keys.B;
                    
                case OpenTK.Input.Key.Back:
                    return Input.Keys.Back;
                    
                case OpenTK.Input.Key.BackSlash:
                    return Input.Keys.OemBackslash;
                    
                case OpenTK.Input.Key.BracketLeft:
                    return Input.Keys.OemOpenBrackets;
                    
                case OpenTK.Input.Key.BracketRight:
                    return Input.Keys.OemCloseBrackets;
                    
                case OpenTK.Input.Key.C:
                    return Input.Keys.C;
                    
                case OpenTK.Input.Key.CapsLock:
                    return Input.Keys.CapsLock;
                    
                case OpenTK.Input.Key.Clear:
                    return Input.Keys.OemClear;
                    
                case OpenTK.Input.Key.Comma:
                    return Input.Keys.OemComma;
                    
                case OpenTK.Input.Key.ControlLeft:
                    return Input.Keys.LeftControl;
                    
                case OpenTK.Input.Key.ControlRight:
                    return Input.Keys.RightControl;
                    
                case OpenTK.Input.Key.D:
                    return Input.Keys.D;
                    
                case OpenTK.Input.Key.Delete:
                    return Input.Keys.Delete;
                    
                case OpenTK.Input.Key.Down:
                    return Input.Keys.Down;
                    
                case OpenTK.Input.Key.E:
                    return Input.Keys.E;
                    
                case OpenTK.Input.Key.End:
                    return Input.Keys.End;
                    
                case OpenTK.Input.Key.Enter:
                    return Input.Keys.Enter;
                    
                case OpenTK.Input.Key.Escape:
                    return Input.Keys.Escape;
                    
                case OpenTK.Input.Key.F:
                    return Input.Keys.F;
                    
                case OpenTK.Input.Key.F1:
                    return Input.Keys.F1;
                    
                case OpenTK.Input.Key.F10:
                    return Input.Keys.F10;
                    
                case OpenTK.Input.Key.F11:
                    return Input.Keys.F11;
                    
                case OpenTK.Input.Key.F12:
                    return Input.Keys.F12;
                    
                case OpenTK.Input.Key.F13:
                    return Input.Keys.F13;
                    
                case OpenTK.Input.Key.F14:
                    return Input.Keys.F14;
                    
                case OpenTK.Input.Key.F15:
                    return Input.Keys.F15;
                    
                case OpenTK.Input.Key.F16:
                    return Input.Keys.F16;
                    
                case OpenTK.Input.Key.F17:
                    return Input.Keys.F17;
                    
                case OpenTK.Input.Key.F18:
                    return Input.Keys.F18;
                    
                case OpenTK.Input.Key.F19:
                    return Input.Keys.F19;
                    
                case OpenTK.Input.Key.F2:
                    return Input.Keys.F2;
                    
                case OpenTK.Input.Key.F20:
                    return Input.Keys.F20;
                    
                case OpenTK.Input.Key.F21:
                    return Input.Keys.F21;
                    
                case OpenTK.Input.Key.F22:
                    return Input.Keys.F22;
                    
                case OpenTK.Input.Key.F23:
                    return Input.Keys.F23;
                    
                case OpenTK.Input.Key.F24:
                    return Input.Keys.F24;
                    
                case OpenTK.Input.Key.F25:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.F26:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.F27:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.F28:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.F29:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.F3:
                    return Input.Keys.F3;
                    
                case OpenTK.Input.Key.F30:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.F31:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.F32:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.F33:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.F34:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.F35:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.F4:
                    return Input.Keys.F4;
                    
                case OpenTK.Input.Key.F5:
                    return Input.Keys.F5;
                    
                case OpenTK.Input.Key.F6:
                    return Input.Keys.F6;
                    
                case OpenTK.Input.Key.F7:
                    return Input.Keys.F7;
                    
                case OpenTK.Input.Key.F8:
                    return Input.Keys.F8;
                    
                case OpenTK.Input.Key.F9:
                    return Input.Keys.F9;
                    
                case OpenTK.Input.Key.G:
                    return Input.Keys.G;
                    
                case OpenTK.Input.Key.H:
                    return Input.Keys.H;
                    
                case OpenTK.Input.Key.Home:
                    return Input.Keys.Home;
                    
                case OpenTK.Input.Key.I:
                    return Input.Keys.I;
                    
                case OpenTK.Input.Key.Insert:
                    return Input.Keys.Insert;
                    
                case OpenTK.Input.Key.J:
                    return Input.Keys.J;
                    
                case OpenTK.Input.Key.K:
                    return Input.Keys.K;
                    
                case OpenTK.Input.Key.Keypad0:
                    return Input.Keys.NumPad0;
                    
                case OpenTK.Input.Key.Keypad1:
                    return Input.Keys.NumPad1;
                    
                case OpenTK.Input.Key.Keypad2:
                    return Input.Keys.NumPad2;
                    
                case OpenTK.Input.Key.Keypad3:
                    return Input.Keys.NumPad3;
                    
                case OpenTK.Input.Key.Keypad4:
                    return Input.Keys.NumPad4;
                    
                case OpenTK.Input.Key.Keypad5:
                    return Input.Keys.NumPad5;
                    
                case OpenTK.Input.Key.Keypad6:
                    return Input.Keys.NumPad6;
                    
                case OpenTK.Input.Key.Keypad7:
                    return Input.Keys.NumPad7;
                    
                case OpenTK.Input.Key.Keypad8:
                    return Input.Keys.NumPad8;
                    
                case OpenTK.Input.Key.Keypad9:
                    return Input.Keys.NumPad9;
                    
                case OpenTK.Input.Key.KeypadAdd:
                    return Input.Keys.Add;
                    
                case OpenTK.Input.Key.KeypadDecimal:
                    return Input.Keys.Decimal;
                    
                case OpenTK.Input.Key.KeypadDivide:
                    return Input.Keys.Divide;
                    
                case OpenTK.Input.Key.KeypadEnter:
                    return Input.Keys.Enter;
                    
                case OpenTK.Input.Key.KeypadMinus:
                    return Input.Keys.OemMinus;
                    
                case OpenTK.Input.Key.KeypadMultiply:
                    return Input.Keys.Multiply;
                    
                case OpenTK.Input.Key.L:
                    return Input.Keys.L;
                    
                case OpenTK.Input.Key.LShift:
                    return Input.Keys.LeftShift;
                    
                case OpenTK.Input.Key.LWin:
                    return Input.Keys.LeftWindows;
                    
                case OpenTK.Input.Key.Left:
                    return Input.Keys.Left;
                    
                case OpenTK.Input.Key.M:
                    return Input.Keys.M;
                    
                case OpenTK.Input.Key.Minus:
                    return Input.Keys.OemMinus;
                    
                case OpenTK.Input.Key.N:
                    return Input.Keys.N;
                    
                case OpenTK.Input.Key.NumLock:
                    return Input.Keys.NumLock;
                    
                case OpenTK.Input.Key.Number0:
                    return Input.Keys.D1;
                    
                case OpenTK.Input.Key.Number1:
                    return Input.Keys.D1;
                    
                case OpenTK.Input.Key.Number2:
                    return Input.Keys.D2;
                    
                case OpenTK.Input.Key.Number3:
                    return Input.Keys.D3;
                    
                case OpenTK.Input.Key.Number4:
                    return Input.Keys.D4;
                    
                case OpenTK.Input.Key.Number5:
                    return Input.Keys.D5;
                    
                case OpenTK.Input.Key.Number6:
                    return Input.Keys.D6;
                    
                case OpenTK.Input.Key.Number7:
                    return Input.Keys.D7;
                    
                case OpenTK.Input.Key.Number8:
                    return Input.Keys.D8;
                    
                case OpenTK.Input.Key.Number9:
                    return Input.Keys.D9;
                    
                case OpenTK.Input.Key.O:
                    return Input.Keys.O;
                    
                case OpenTK.Input.Key.P:
                    return Input.Keys.P;
                    
                case OpenTK.Input.Key.PageDown:
                    return Input.Keys.PageDown;
                    
                case OpenTK.Input.Key.PageUp:
                    return Input.Keys.PageUp;
                    
                case OpenTK.Input.Key.Pause:
                    return Input.Keys.Pause;
                    
                case OpenTK.Input.Key.Period:
                    return Input.Keys.OemPeriod;
                    
                case OpenTK.Input.Key.Plus:
                    return Input.Keys.OemPlus;
                    
                case OpenTK.Input.Key.PrintScreen:
                    return Input.Keys.PrintScreen;
                    
                case OpenTK.Input.Key.Q:
                    return Input.Keys.Q;
                    
                case OpenTK.Input.Key.Quote:
                    return Input.Keys.OemQuotes;
                    
                case OpenTK.Input.Key.R:
                    return Input.Keys.R;
                    
                case OpenTK.Input.Key.Right:
                    return Input.Keys.Right;
                    
                case OpenTK.Input.Key.S:
                    return Input.Keys.S;
                    
                case OpenTK.Input.Key.ScrollLock:
                    return Input.Keys.Scroll;
                    
                case OpenTK.Input.Key.Semicolon:
                    return Input.Keys.OemSemicolon;
                    
                case OpenTK.Input.Key.Slash:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.Sleep:
                    return Input.Keys.Sleep;
                    
                case OpenTK.Input.Key.Space:
                    return Input.Keys.Space;
                    
                case OpenTK.Input.Key.T:
                    return Input.Keys.T;
                    
                case OpenTK.Input.Key.Tab:
                    return Input.Keys.Tab;
                    
                case OpenTK.Input.Key.Tilde:
                    return Input.Keys.OemTilde;
                    
                case OpenTK.Input.Key.U:
                    return Input.Keys.U;
                    
                case OpenTK.Input.Key.Unknown:
                    return Input.Keys.None;
                    
                case OpenTK.Input.Key.Up:
                    return Input.Keys.Up;
                    
                case OpenTK.Input.Key.V:
                    return Input.Keys.V;
                    
                case OpenTK.Input.Key.W:
                    return Input.Keys.W;
                    
                case OpenTK.Input.Key.X:
                    return Input.Keys.X;
                    
                case OpenTK.Input.Key.Y:
                    return Input.Keys.Y;
                    
                case OpenTK.Input.Key.Z:
                    return Input.Keys.Z;
                    
                default:
                    return Input.Keys.None;
                    
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
            OpenTkGameWindow.Closing += new EventHandler<CancelEventArgs>(OpenTkGameWindow_Closing);
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

        void OpenTkGameWindow_Closing(object sender,CancelEventArgs e)
        {        	
        	Game.Exit();
        }

        void Keyboard_KeyUp(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (keys.Contains(e.Key.ToXna())) keys.Remove(e.Key.ToXna());
        }

        void Keyboard_KeyDown(object sender, OpenTK.Input.KeyboardKeyEventArgs e)
        {
            if (!keys.Contains(e.Key.ToXna())) keys.Add(e.Key.ToXna());
        }

        // This method should only be called when necessary like when the Guide is displayed
        internal void ClearKeyCacheState()
        {
            keys.Clear();
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

