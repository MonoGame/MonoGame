// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace MonoGame.InteractiveTests.TestUI
{
    /// <summary>Specified padding within a container <see cref="View"/></summary>
    public struct PaddingF
    {
        public float Left;
        public float Top;
        public float Right;
        public float Bottom;

        public float Horizontal { get { return Left + Right; } }

        public float Vertical { get { return Top + Bottom; } }

        public PaddingF(float all)
        {
            Left = Top = Right = Bottom = all;
        }

        public PaddingF(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public static bool operator ==(PaddingF a, PaddingF b)
        {
            return
                a.Left == b.Left &&
                a.Top == b.Top &&
                a.Right == b.Right &&
                a.Bottom == b.Bottom;
        }

        public static bool operator !=(PaddingF a, PaddingF b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PaddingF))
                return false;

            return this == (PaddingF)obj;
        }

        public override int GetHashCode()
        {
            return
                Left.GetHashCode() ^
                (Top.GetHashCode() << 8) ^
                (Right.GetHashCode() << 16) ^
                (Bottom.GetHashCode() << 24);
        }
    }
}
