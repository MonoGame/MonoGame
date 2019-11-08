// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input
{
    internal static class KeyboardUtil
    {
        static Dictionary<ushort, Keys> _map;

        static KeyboardUtil()
        {
            _map = new Dictionary<ushort, Keys>();

            if (MonoGame.Utilities.CurrentPlatform.OS == MonoGame.Utilities.OS.Windows)
            {
                _map.Add(27, Keys.Escape);

                _map.Add(49, Keys.D1);
                _map.Add(50, Keys.D2);
                _map.Add(51, Keys.D3);
                _map.Add(52, Keys.D4);
                _map.Add(53, Keys.D5);
                _map.Add(54, Keys.D6);
                _map.Add(55, Keys.D7);
                _map.Add(56, Keys.D8);
                _map.Add(57, Keys.D9);
                _map.Add(48, Keys.D0);
                _map.Add(189, Keys.OemMinus);
                _map.Add(187, Keys.OemPlus);

                _map.Add(9, Keys.Tab);
                _map.Add(81, Keys.Q);
                _map.Add(87, Keys.W);
                _map.Add(69, Keys.E);
                _map.Add(82, Keys.R);
                _map.Add(84, Keys.T);
                _map.Add(89, Keys.Y);
                _map.Add(85, Keys.U);
                _map.Add(73, Keys.I);
                _map.Add(79, Keys.O);
                _map.Add(80, Keys.P);
                _map.Add(219, Keys.OemOpenBrackets);
                _map.Add(221, Keys.OemCloseBrackets);
                _map.Add(13, Keys.Enter);
                _map.Add(17, Keys.LeftControl);
                _map.Add(65, Keys.A);
                _map.Add(83, Keys.S);
                _map.Add(68, Keys.D);
                _map.Add(70, Keys.F);
                _map.Add(71, Keys.G);
                _map.Add(72, Keys.H);
                _map.Add(74, Keys.J);
                _map.Add(75, Keys.K);
                _map.Add(76, Keys.L);
                _map.Add(186, Keys.OemSemicolon);
                _map.Add(222, Keys.OemQuotes);
                _map.Add(192, Keys.OemTilde);
                _map.Add(16, Keys.LeftShift);
                _map.Add(220, Keys.OemBackslash);
                _map.Add(90, Keys.Z);
                _map.Add(88, Keys.X);
                _map.Add(67, Keys.C);
                _map.Add(86, Keys.V);
                _map.Add(66, Keys.B);
                _map.Add(78, Keys.N);
                _map.Add(77, Keys.M);
                _map.Add(188, Keys.OemComma);
                _map.Add(190, Keys.OemPeriod);
                _map.Add(191, Keys.OemQuestion);
                _map.Add(161, Keys.RightShift);
                _map.Add(106, Keys.Multiply);
                _map.Add(18, Keys.LeftAlt);
                _map.Add(32, Keys.Space);
                _map.Add(20, Keys.CapsLock);
                _map.Add(112, Keys.F1);
                _map.Add(113, Keys.F2);
                _map.Add(114, Keys.F3);
                _map.Add(115, Keys.F4);
                _map.Add(116, Keys.F5);
                _map.Add(117, Keys.F6);
                _map.Add(118, Keys.F7);
                _map.Add(119, Keys.F8);
                _map.Add(120, Keys.F9);
                _map.Add(121, Keys.F10);
                _map.Add(144, Keys.NumLock);
                _map.Add(145, Keys.Scroll);
                _map.Add(103, Keys.NumPad7);
                _map.Add(104, Keys.NumPad8);
                _map.Add(105, Keys.NumPad9);
                _map.Add(109, Keys.Subtract);
                _map.Add(100, Keys.NumPad4);
                _map.Add(101, Keys.NumPad5);
                _map.Add(102, Keys.NumPad6);
                _map.Add(107, Keys.Add);
                _map.Add(97, Keys.NumPad1);
                _map.Add(98, Keys.NumPad2);
                _map.Add(99, Keys.NumPad3);
                _map.Add(96, Keys.NumPad0);
                _map.Add(110, Keys.Decimal);

                _map.Add(122, Keys.F11);
                _map.Add(123, Keys.F12);

                _map.Add(163, Keys.RightControl);
                _map.Add(111, Keys.Divide);

                _map.Add(165, Keys.RightAlt);

                _map.Add(36, Keys.Home);
                _map.Add(38, Keys.Up);
                _map.Add(33, Keys.PageUp);
                _map.Add(37, Keys.Left);
                _map.Add(39, Keys.Right);
                _map.Add(35, Keys.End);
                _map.Add(40, Keys.Down);
                _map.Add(34, Keys.PageDown);
                _map.Add(45, Keys.Insert);
                _map.Add(46, Keys.Delete);

                _map.Add(19, Keys.Pause);
            }
            else
            {
                _map.Add(9, Keys.Escape);

                _map.Add(10, Keys.D1);
                _map.Add(11, Keys.D2);
                _map.Add(12, Keys.D3);
                _map.Add(13, Keys.D4);
                _map.Add(14, Keys.D5);
                _map.Add(15, Keys.D6);
                _map.Add(16, Keys.D7);
                _map.Add(17, Keys.D8);
                _map.Add(18, Keys.D9);
                _map.Add(19, Keys.D0);
                _map.Add(20, Keys.OemMinus);
                _map.Add(21, Keys.OemPlus);

                _map.Add(23, Keys.Tab);
                _map.Add(24, Keys.Q);
                _map.Add(25, Keys.W);
                _map.Add(26, Keys.E);
                _map.Add(27, Keys.R);
                _map.Add(28, Keys.T);
                _map.Add(29, Keys.Y);
                _map.Add(30, Keys.U);
                _map.Add(31, Keys.I);
                _map.Add(32, Keys.O);
                _map.Add(33, Keys.P);
                _map.Add(34, Keys.OemOpenBrackets);
                _map.Add(35, Keys.OemCloseBrackets);
                _map.Add(36, Keys.Enter);
                _map.Add(37, Keys.LeftControl);
                _map.Add(38, Keys.A);
                _map.Add(39, Keys.S);
                _map.Add(40, Keys.D);
                _map.Add(41, Keys.F);
                _map.Add(42, Keys.G);
                _map.Add(43, Keys.H);
                _map.Add(44, Keys.J);
                _map.Add(45, Keys.K);
                _map.Add(46, Keys.L);
                _map.Add(47, Keys.OemSemicolon);
                _map.Add(48, Keys.OemQuotes);
                _map.Add(49, Keys.OemTilde);
                _map.Add(50, Keys.LeftShift);
                _map.Add(51, Keys.OemBackslash);
                _map.Add(52, Keys.Z);
                _map.Add(53, Keys.X);
                _map.Add(54, Keys.C);
                _map.Add(55, Keys.V);
                _map.Add(56, Keys.B);
                _map.Add(57, Keys.N);
                _map.Add(58, Keys.M);
                _map.Add(59, Keys.OemComma);
                _map.Add(60, Keys.OemPeriod);
                _map.Add(61, Keys.OemQuestion);
                _map.Add(62, Keys.RightShift);
                _map.Add(63, Keys.Multiply);
                _map.Add(64, Keys.LeftAlt);
                _map.Add(65, Keys.Space);
                _map.Add(66, Keys.CapsLock);
                _map.Add(67, Keys.F1);
                _map.Add(68, Keys.F2);
                _map.Add(69, Keys.F3);
                _map.Add(70, Keys.F4);
                _map.Add(71, Keys.F5);
                _map.Add(72, Keys.F6);
                _map.Add(73, Keys.F7);
                _map.Add(74, Keys.F8);
                _map.Add(75, Keys.F9);
                _map.Add(76, Keys.F10);
                _map.Add(77, Keys.NumLock);
                _map.Add(78, Keys.Scroll);
                _map.Add(79, Keys.NumPad7);
                _map.Add(80, Keys.NumPad8);
                _map.Add(81, Keys.NumPad9);
                _map.Add(82, Keys.Subtract);
                _map.Add(83, Keys.NumPad4);
                _map.Add(84, Keys.NumPad5);
                _map.Add(85, Keys.NumPad6);
                _map.Add(86, Keys.Add);
                _map.Add(87, Keys.NumPad1);
                _map.Add(88, Keys.NumPad2);
                _map.Add(89, Keys.NumPad3);
                _map.Add(90, Keys.NumPad0);
                _map.Add(91, Keys.Decimal);

                _map.Add(94, Keys.OemBackslash);
                _map.Add(95, Keys.F11);
                _map.Add(96, Keys.F12);

                _map.Add(104, Keys.Enter);
                _map.Add(105, Keys.RightControl);
                _map.Add(106, Keys.Divide);

                _map.Add(108, Keys.RightAlt);

                _map.Add(110, Keys.Home);
                _map.Add(111, Keys.Up);
                _map.Add(112, Keys.PageUp);
                _map.Add(113, Keys.Left);
                _map.Add(114, Keys.Right);
                _map.Add(115, Keys.End);
                _map.Add(116, Keys.Down);
                _map.Add(117, Keys.PageDown);
                _map.Add(118, Keys.Insert);
                _map.Add(119, Keys.Delete);

                _map.Add(127, Keys.Pause);
            }
        }

        public static Keys ToXna(ushort key)
        {
            if (_map.ContainsKey(key))
                return _map[key];

            return Keys.None;
        }
    }
}

