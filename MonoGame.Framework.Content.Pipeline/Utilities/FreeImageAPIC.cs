// The following code is part of FreeImage.NET
// https://freeimagenet.codeplex.com/SourceControl/latest#FreeImage.NET/FreeImage.NET/3.17.0.4/FreeImageWrapper.cs
//
// COVERED CODE IS PROVIDED UNDER THIS LICENSE ON AN "AS IS" BASIS, WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING, WITHOUT LIMITATION, WARRANTIES
// THAT THE COVERED CODE IS FREE OF DEFECTS, MERCHANTABLE, FIT FOR A PARTICULAR PURPOSE
// OR NON-INFRINGING. THE ENTIRE RISK AS TO THE QUALITY AND PERFORMANCE OF THE COVERED
// CODE IS WITH YOU. SHOULD ANY COVERED CODE PROVE DEFECTIVE IN ANY RESPECT, YOU (NOT
// THE INITIAL DEVELOPER OR ANY OTHER CONTRIBUTOR) ASSUME THE COST OF ANY NECESSARY
// SERVICING, REPAIR OR CORRECTION. THIS DISCLAIMER OF WARRANTY CONSTITUTES AN ESSENTIAL
// PART OF THIS LICENSE. NO USE OF ANY COVERED CODE IS AUTHORIZED HEREUNDER EXCEPT UNDER
// THIS DISCLAIMER.

using System;

namespace FreeImageAPI
{
    partial class FreeImage
    {
        public static unsafe void CopyMemory(byte* dest, byte* src, int len)
        {
            if (len >= 0x10)
            {
                do
                {
                    *((int*)dest) = *((int*)src);
                    *((int*)(dest + 4)) = *((int*)(src + 4));
                    *((int*)(dest + 8)) = *((int*)(src + 8));
                    *((int*)(dest + 12)) = *((int*)(src + 12));
                    dest += 0x10;
                    src += 0x10;
                }
                while ((len -= 0x10) >= 0x10);
            }
            if (len > 0)
            {
                if ((len & 8) != 0)
                {
                    *((int*)dest) = *((int*)src);
                    *((int*)(dest + 4)) = *((int*)(src + 4));
                    dest += 8;
                    src += 8;
                }
                if ((len & 4) != 0)
                {
                    *((int*)dest) = *((int*)src);
                    dest += 4;
                    src += 4;
                }
                if ((len & 2) != 0)
                {
                    *((short*)dest) = *((short*)src);
                    dest += 2;
                    src += 2;
                }
                if ((len & 1) != 0)
                {
                    *dest = *src;
                }
            }
        }

        public static unsafe IntPtr ConvertFromRawBits(
            byte[] bits,
            FREE_IMAGE_TYPE type,
            int width,
            int height,
            int pitch,
            uint bpp,
            uint red_mask,
            uint green_mask,
            uint blue_mask,
            bool topdown)
        {
            fixed (byte* ptr = bits)
            {
                return ConvertFromRawBits(
                    (IntPtr)ptr,
                    type,
                    width,
                    height,
                    pitch,
                    bpp,
                    red_mask,
                    green_mask,
                    blue_mask,
                    topdown);
            }
        }

        public static unsafe IntPtr ConvertFromRawBits(
            IntPtr bits,
            FREE_IMAGE_TYPE type,
            int width,
            int height,
            int pitch,
            uint bpp,
            uint red_mask,
            uint green_mask,
            uint blue_mask,
            bool topdown)
        {
            byte* addr = (byte*)bits;
            if ((addr == null) || (width <= 0) || (height <= 0))
            {
                return IntPtr.Zero;
            }

            IntPtr dib = AllocateT(type, width, height, (int)bpp, red_mask, green_mask, blue_mask);
            if (dib != IntPtr.Zero)
            {
                if (topdown)
                {
                    for (int i = height - 1; i >= 0; --i)
                    {
                        CopyMemory((byte*)GetScanLine(dib, i), addr, (int)GetLine(dib));
                        addr += pitch;
                    }
                }
                else
                {
                    for (int i = 0; i < height; ++i)
                    {
                        CopyMemory((byte*)GetScanLine(dib, i), addr, (int)GetLine(dib));
                        addr += pitch;
                    }
                }
            }
            return dib;
        }
    }
}
