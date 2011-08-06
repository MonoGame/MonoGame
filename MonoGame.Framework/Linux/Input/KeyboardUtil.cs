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
}

