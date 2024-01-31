// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;

namespace FreeImageAPI
{
    internal enum FREE_IMAGE_TYPE
    {
        FIT_UNKNOWN,
        FIT_BITMAP,
        FIT_UINT16,
        FIT_INT16,
        FIT_UINT32,
        FIT_INT32,
        FIT_FLOAT,
        FIT_DOUBLE,
        FIT_COMPLEX,
        FIT_RGB16,
        FIT_RGBA16,
        FIT_RGBF,
        FIT_RGBAF
    }

    internal enum FREE_IMAGE_FILTER
    {
        FILTER_BOX,
        FILTER_BICUBIC,
        FILTER_BILINEAR,
        FILTER_BSPLINE,
        FILTER_CATMULLROM,
        FILTER_LANCZOS3
    }

    internal enum FREE_IMAGE_FORMAT
    {
        FIF_UNKNOWN = -1,
        FIF_BMP,
        FIF_ICO,
        FIF_JPEG,
        FIF_JNG,
        FIF_KOALA,
        FIF_LBM,
        FIF_IFF,
        FIF_MNG,
        FIF_PBM,
        FIF_PBMRAW,
        FIF_PCD,
        FIF_PCX,
        FIF_PGM,
        FIF_PGMRAW,
        FIF_PNG,
        FIF_PPM,
        FIF_PPMRAW,
        FIF_RAS,
        FIF_TARGA,
        FIF_TIFF,
        FIF_WBMP,
        FIF_PSD,
        FIF_CUT,
        FIF_XBM,
        FIF_XPM,
        FIF_DDS,
        FIF_GIF,
        FIF_HDR,
        FIF_FAXG3,
        FIF_SGI,
        FIF_EXR,
        FIF_J2K,
        FIF_JP2,
        FIF_PFM,
        FIF_PICT,
        FIF_RAW,
        FIF_WEBP,
        FIF_JXR
    }

    internal enum FREE_IMAGE_COLOR_CHANNEL
    {
        FICC_RGB,
        FICC_RED,
        FICC_GREEN,
        FICC_BLUE,
        FICC_ALPHA,
        FICC_BLACK,
        FICC_REAL,
        FICC_IMAG,
        FICC_MAG,
        FICC_PHASE
    }

    partial class FreeImage
    {
        private const string NativeLibName = "FreeImage";

        [DllImport(NativeLibName, EntryPoint = "FreeImage_ConvertFromRawBits")]
        public static extern IntPtr ConvertFromRawBits(byte[] bits, int width, int height, int pitch, uint bpp, uint red_mask, uint green_mask, uint blue_mask, bool topdown);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_Rescale")]
        public static extern IntPtr Rescale(IntPtr dib, int dst_width, int dst_height, FREE_IMAGE_FILTER filter);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_Unload")]
        public static extern void Unload(IntPtr dib);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_ConvertToRawBits")]
        public static extern void ConvertToRawBits(byte[] bits, IntPtr dib, int pitch, uint bpp, uint red_mask, uint green_mask, uint blue_mask, bool topdown);

        [DllImport(NativeLibName, CharSet = CharSet.Unicode, EntryPoint = "FreeImage_GetFileTypeU")]
        public static extern FREE_IMAGE_FORMAT GetFileTypeU(string filename, int size);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_GetFileType")]
        public static extern FREE_IMAGE_FORMAT GetFileTypeS(string filename, int size);

        public static FREE_IMAGE_FORMAT GetFileType(string filename, int size)
        {
            if (CurrentPlatform.OS == OS.Windows)
                return GetFileTypeU(filename, size);
            else
                return GetFileTypeS(filename, size);
        }

        [DllImport(NativeLibName, CharSet = CharSet.Unicode, EntryPoint = "FreeImage_LoadU")]
        public static extern IntPtr LoadU(FREE_IMAGE_FORMAT fif, string filename, int flags);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_Load")]
        public static extern IntPtr LoadS(FREE_IMAGE_FORMAT fif, string filename, int flags);

        public static IntPtr Load(FREE_IMAGE_FORMAT fif, string filename, int flags)
        {
            if (CurrentPlatform.OS == OS.Windows)
                return LoadU(fif, filename, flags);
            else
                return LoadS(fif, filename, flags);
        }

        [DllImport(NativeLibName, EntryPoint = "FreeImage_GetHeight")]
        public static extern uint GetHeight(IntPtr dib);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_GetWidth")]
        public static extern uint GetWidth(IntPtr dib);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_GetImageType")]
        public static extern FREE_IMAGE_TYPE GetImageType(IntPtr dib);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_GetChannel")]
        public static extern IntPtr GetChannel(IntPtr dib, FREE_IMAGE_COLOR_CHANNEL channel);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_SetChannel")]
        public static extern bool SetChannel(IntPtr dib, IntPtr dib8, FREE_IMAGE_COLOR_CHANNEL channel);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_ConvertToType")]
        public static extern IntPtr ConvertToType(IntPtr src, FREE_IMAGE_TYPE dst_type, bool scale_linear);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_ConvertTo32Bits")]
        public static extern IntPtr ConvertTo32Bits(IntPtr dib);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_GetBPP")]
        public static extern uint GetBPP(IntPtr dib);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_GetPitch")]
        public static extern uint GetPitch(IntPtr dib);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_GetRedMask")]
        public static extern uint GetRedMask(IntPtr dib);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_GetGreenMask")]
        public static extern uint GetGreenMask(IntPtr dib);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_GetBlueMask")]
        public static extern uint GetBlueMask(IntPtr dib);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_AllocateT")]
        public static extern IntPtr AllocateT(FREE_IMAGE_TYPE type, int width, int height, int bpp, uint red_mask, uint green_mask, uint blue_mask);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_GetScanLine")]
        public static extern IntPtr GetScanLine(IntPtr dib, int scanline);

        [DllImport(NativeLibName, EntryPoint = "FreeImage_GetLine")]
        public static extern uint GetLine(IntPtr dib);
    }
}
