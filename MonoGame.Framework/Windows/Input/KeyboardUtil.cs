using System;

namespace Microsoft.Xna.Framework.Input
{
	internal static class KeyboardUtil
	{
		// TODO make a dictionary for fast mapping
        public static Keys ToXna(OpenTK.Input.Key key)
        {
            switch (key)
            {
                case OpenTK.Input.Key.A:
                    return Keys.A;
                    
                case OpenTK.Input.Key.AltLeft:
                    return Keys.LeftAlt;
                    
                case OpenTK.Input.Key.AltRight:
                    return Keys.RightAlt;
                    
                case OpenTK.Input.Key.B:
                    return Keys.B;
                    
                case OpenTK.Input.Key.Back:
                    return Keys.Back;
                    
                case OpenTK.Input.Key.BackSlash:
                    return Keys.OemBackslash;
                    
                case OpenTK.Input.Key.BracketLeft:
                    return Keys.OemOpenBrackets;
                    
                case OpenTK.Input.Key.BracketRight:
                    return Keys.OemCloseBrackets;
                    
                case OpenTK.Input.Key.C:
                    return Keys.C;
                    
                case OpenTK.Input.Key.CapsLock:
                    return Keys.CapsLock;
                    
                case OpenTK.Input.Key.Clear:
                    return Keys.OemClear;
                    
                case OpenTK.Input.Key.Comma:
                    return Keys.OemComma;
                    
                case OpenTK.Input.Key.ControlLeft:
                    return Keys.LeftControl;
                    
                case OpenTK.Input.Key.ControlRight:
                    return Keys.RightControl;
                    
                case OpenTK.Input.Key.D:
                    return Keys.D;
                    
                case OpenTK.Input.Key.Delete:
                    return Keys.Delete;
                    
                case OpenTK.Input.Key.Down:
                    return Keys.Down;
                    
                case OpenTK.Input.Key.E:
                    return Keys.E;
                    
                case OpenTK.Input.Key.End:
                    return Keys.End;
                    
                case OpenTK.Input.Key.Enter:
                    return Keys.Enter;
                    
                case OpenTK.Input.Key.Escape:
                    return Keys.Escape;
                    
                case OpenTK.Input.Key.F:
                    return Keys.F;
                    
                case OpenTK.Input.Key.F1:
                    return Keys.F1;
                    
                case OpenTK.Input.Key.F10:
                    return Keys.F10;
                    
                case OpenTK.Input.Key.F11:
                    return Keys.F11;
                    
                case OpenTK.Input.Key.F12:
                    return Keys.F12;
                    
                case OpenTK.Input.Key.F13:
                    return Keys.F13;
                    
                case OpenTK.Input.Key.F14:
                    return Keys.F14;
                    
                case OpenTK.Input.Key.F15:
                    return Keys.F15;
                    
                case OpenTK.Input.Key.F16:
                    return Keys.F16;
                    
                case OpenTK.Input.Key.F17:
                    return Keys.F17;
                    
                case OpenTK.Input.Key.F18:
                    return Keys.F18;
                    
                case OpenTK.Input.Key.F19:
                    return Keys.F19;
                    
                case OpenTK.Input.Key.F2:
                    return Keys.F2;
                    
                case OpenTK.Input.Key.F20:
                    return Keys.F20;
                    
                case OpenTK.Input.Key.F21:
                    return Keys.F21;
                    
                case OpenTK.Input.Key.F22:
                    return Keys.F22;
                    
                case OpenTK.Input.Key.F23:
                    return Keys.F23;
                    
                case OpenTK.Input.Key.F24:
                    return Keys.F24;
                    
                case OpenTK.Input.Key.F25:
                    return Keys.None;
                    
                case OpenTK.Input.Key.F26:
                    return Keys.None;
                    
                case OpenTK.Input.Key.F27:
                    return Keys.None;
                    
                case OpenTK.Input.Key.F28:
                    return Keys.None;
                    
                case OpenTK.Input.Key.F29:
                    return Keys.None;
                    
                case OpenTK.Input.Key.F3:
                    return Keys.F3;
                    
                case OpenTK.Input.Key.F30:
                    return Keys.None;
                    
                case OpenTK.Input.Key.F31:
                    return Keys.None;
                    
                case OpenTK.Input.Key.F32:
                    return Keys.None;
                    
                case OpenTK.Input.Key.F33:
                    return Keys.None;
                    
                case OpenTK.Input.Key.F34:
                    return Keys.None;
                    
                case OpenTK.Input.Key.F35:
                    return Keys.None;
                    
                case OpenTK.Input.Key.F4:
                    return Keys.F4;
                    
                case OpenTK.Input.Key.F5:
                    return Keys.F5;
                    
                case OpenTK.Input.Key.F6:
                    return Keys.F6;
                    
                case OpenTK.Input.Key.F7:
                    return Keys.F7;
                    
                case OpenTK.Input.Key.F8:
                    return Keys.F8;
                    
                case OpenTK.Input.Key.F9:
                    return Keys.F9;
                    
                case OpenTK.Input.Key.G:
                    return Keys.G;
                    
                case OpenTK.Input.Key.H:
                    return Keys.H;
                    
                case OpenTK.Input.Key.Home:
                    return Keys.Home;
                    
                case OpenTK.Input.Key.I:
                    return Keys.I;
                    
                case OpenTK.Input.Key.Insert:
                    return Keys.Insert;
                    
                case OpenTK.Input.Key.J:
                    return Keys.J;
                    
                case OpenTK.Input.Key.K:
                    return Keys.K;
                    
                case OpenTK.Input.Key.Keypad0:
                    return Keys.NumPad0;
                    
                case OpenTK.Input.Key.Keypad1:
                    return Keys.NumPad1;
                    
                case OpenTK.Input.Key.Keypad2:
                    return Keys.NumPad2;
                    
                case OpenTK.Input.Key.Keypad3:
                    return Keys.NumPad3;
                    
                case OpenTK.Input.Key.Keypad4:
                    return Keys.NumPad4;
                    
                case OpenTK.Input.Key.Keypad5:
                    return Keys.NumPad5;
                    
                case OpenTK.Input.Key.Keypad6:
                    return Keys.NumPad6;
                    
                case OpenTK.Input.Key.Keypad7:
                    return Keys.NumPad7;
                    
                case OpenTK.Input.Key.Keypad8:
                    return Keys.NumPad8;
                    
                case OpenTK.Input.Key.Keypad9:
                    return Keys.NumPad9;
                    
                case OpenTK.Input.Key.KeypadAdd:
                    return Keys.Add;
                    
                case OpenTK.Input.Key.KeypadDecimal:
                    return Keys.Decimal;
                    
                case OpenTK.Input.Key.KeypadDivide:
                    return Keys.Divide;
                    
                case OpenTK.Input.Key.KeypadEnter:
                    return Keys.Enter;
                    
                case OpenTK.Input.Key.KeypadMinus:
                    return Keys.OemMinus;
                    
                case OpenTK.Input.Key.KeypadMultiply:
                    return Keys.Multiply;
                    
                case OpenTK.Input.Key.L:
                    return Keys.L;
                    
                case OpenTK.Input.Key.LShift:
                    return Keys.LeftShift;
                    
                case OpenTK.Input.Key.LWin:
                    return Keys.LeftWindows;
                    
                case OpenTK.Input.Key.Left:
                    return Keys.Left;
                    
                case OpenTK.Input.Key.M:
                    return Keys.M;
                    
                case OpenTK.Input.Key.Minus:
                    return Keys.OemMinus;
                    
                case OpenTK.Input.Key.N:
                    return Keys.N;
                    
                case OpenTK.Input.Key.NumLock:
                    return Keys.NumLock;
                    
                case OpenTK.Input.Key.Number0:
                    return Keys.D0;
                    
                case OpenTK.Input.Key.Number1:
                    return Keys.D1;
                    
                case OpenTK.Input.Key.Number2:
                    return Keys.D2;
                    
                case OpenTK.Input.Key.Number3:
                    return Keys.D3;
                    
                case OpenTK.Input.Key.Number4:
                    return Keys.D4;
                    
                case OpenTK.Input.Key.Number5:
                    return Keys.D5;
                    
                case OpenTK.Input.Key.Number6:
                    return Keys.D6;
                    
                case OpenTK.Input.Key.Number7:
                    return Keys.D7;
                    
                case OpenTK.Input.Key.Number8:
                    return Keys.D8;
                    
                case OpenTK.Input.Key.Number9:
                    return Keys.D9;
                    
                case OpenTK.Input.Key.O:
                    return Keys.O;
                    
                case OpenTK.Input.Key.P:
                    return Keys.P;
                    
                case OpenTK.Input.Key.PageDown:
                    return Keys.PageDown;
                    
                case OpenTK.Input.Key.PageUp:
                    return Keys.PageUp;
                    
                case OpenTK.Input.Key.Pause:
                    return Keys.Pause;
                    
                case OpenTK.Input.Key.Period:
                    return Keys.OemPeriod;
                    
                case OpenTK.Input.Key.Plus:
                    return Keys.OemPlus;
                    
                case OpenTK.Input.Key.PrintScreen:
                    return Keys.PrintScreen;
                    
                case OpenTK.Input.Key.Q:
                    return Keys.Q;
                    
                case OpenTK.Input.Key.Quote:
                    return Keys.OemQuotes;
                    
                case OpenTK.Input.Key.R:
                    return Keys.R;
                    
                case OpenTK.Input.Key.Right:
                    return Keys.Right;

                case OpenTK.Input.Key.RShift:
                    return Keys.RightShift;

                case OpenTK.Input.Key.RWin:
                    return Keys.RightWindows;
                    
                case OpenTK.Input.Key.S:
                    return Keys.S;
                    
                case OpenTK.Input.Key.ScrollLock:
                    return Keys.Scroll;
                    
                case OpenTK.Input.Key.Semicolon:
                    return Keys.OemSemicolon;
                    
                case OpenTK.Input.Key.Slash:
                    return Keys.OemQuestion;
                    
                case OpenTK.Input.Key.Sleep:
                    return Keys.Sleep;
                    
                case OpenTK.Input.Key.Space:
                    return Keys.Space;
                    
                case OpenTK.Input.Key.T:
                    return Keys.T;
                    
                case OpenTK.Input.Key.Tab:
                    return Keys.Tab;
                    
                case OpenTK.Input.Key.Tilde:
                    return Keys.OemTilde;
                    
                case OpenTK.Input.Key.U:
                    return Keys.U;
                    
                case OpenTK.Input.Key.Unknown:
                    return Keys.None;
                    
                case OpenTK.Input.Key.Up:
                    return Keys.Up;
                    
                case OpenTK.Input.Key.V:
                    return Keys.V;
                    
                case OpenTK.Input.Key.W:
                    return Keys.W;
                    
                case OpenTK.Input.Key.X:
                    return Keys.X;
                    
                case OpenTK.Input.Key.Y:
                    return Keys.Y;
                    
                case OpenTK.Input.Key.Z:
                    return Keys.Z;
                    
                default:
                    return Keys.None;
                    
            }
        }
    }
}

