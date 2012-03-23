namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    using System;

    internal static class HalfUtils
    {
        private const uint BiasDiffo = 0xc8000000;
        private const int cExpBias = 15;
        private const int cExpBits = 5;
        private const int cFracBits = 10;
        private const int cFracBitsDiff = 13;
        private const uint cFracMask = 0x3ff;
        private const uint cRoundBit = 0x1000;
        private const int cSignBit = 15;
        private const uint cSignMask = 0x8000;
        private const uint eMax = 0x10;
        private const int eMin = -14;
        private const uint wMaxNormal = 0x47ffefff;
        private const uint wMinNormal = 0x38800000;

        public static unsafe ushort Pack(float value)
        {
            uint num5 = *((uint*)&value);
            uint num3 = (uint)((num5 & -2147483648) >> 0x10);
            uint num = num5 & 0x7fffffff;
            if (num > 0x47ffefff)
            {
                return (ushort)(num3 | 0x7fff);
            }
            if (num < 0x38800000)
            {
                uint num6 = (num & 0x7fffff) | 0x800000;
                int num4 = 0x71 - ((int)(num >> 0x17));
                num = (num4 > 0x1f) ? 0 : (num6 >> num4);
                return (ushort)(num3 | (((num + 0xfff) + ((num >> 13) & 1)) >> 13));
            }
            return (ushort)(num3 | ((((num + -939524096) + 0xfff) + ((num >> 13) & 1)) >> 13));
        }

        public static unsafe float Unpack(ushort value)
        {
            uint num3;
            if ((value & -33792) == 0)
            {
                if ((value & 0x3ff) != 0)
                {
                    uint num2 = 0xfffffff2;
                    uint num = (uint)(value & 0x3ff);
                    while ((num & 0x400) == 0)
                    {
                        num2--;
                        num = num << 1;
                    }
                    num &= 0xfffffbff;
                    num3 = ((uint)(((value & 0x8000) << 0x10) | ((num2 + 0x7f) << 0x17))) | (num << 13);
                }
                else
                {
                    num3 = (uint)((value & 0x8000) << 0x10);
                }
            }
            else
            {
                num3 = (uint)((((value & 0x8000) << 0x10) | (((((value >> 10) & 0x1f) - 15) + 0x7f) << 0x17)) | ((value & 0x3ff) << 13));
            }
            return *(((float*)&num3));
        }
    }
}