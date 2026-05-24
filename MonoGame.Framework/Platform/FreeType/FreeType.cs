// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;

internal unsafe static class FreeType
{
    public static readonly IntPtr NativeLibrary = GetNativeLibrary();

    private static IntPtr GetNativeLibrary()
    {
        if (CurrentPlatform.OS == OS.Windows)
        {
            return FuncLoader.LoadLibraryExt("freetype.dll");
        }
        else if (CurrentPlatform.OS == OS.Linux)
        {
            return FuncLoader.LoadLibraryExt("libfreetype.so");
        }
        else if (CurrentPlatform.OS == OS.MacOSX)
        {
            return FuncLoader.LoadLibraryExt("libfreetype.dylib");
        }
        else
        {
            return FuncLoader.LoadLibraryExt("libfreetype");
        }
    }

    internal static readonly int CLongSize = GetCLongSize();

    /// Determines the ABI size of C long for the current target.
    /// Using this instead of System.Runtime.InteropServices.CLong since we
    /// need to target netstandard2.1 for MonoGame.Framework.Native
    private static int GetCLongSize()
    {
#if WINDOWS
        // Windows C Long is always 32-bit (4 bytes)
        return 4;
#elif DESKTOPGL || NATIVE
        // DesktopGL and Native can both target Windows or *Nix systems
        // So the size depends on which runtime the platform is on
        return CurrentPlatform.OS == OS.Windows ? 4 : IntPtr.Size;
#else
        return IntPtr.Size;
#endif
    }

    public enum PixelMode : byte
    {
        None = 0,
        Mono = 1,
        Gray = 2,
        Gray2 = 3,
        Gray4 = 4,
        Lcd = 5,
        LcdVertical = 6,
        Bgra = 7
    }

    public enum RenderMode
    {
        Normal = 0,
        Light = 1,
        Mono = 2,
        Lcd = 3,
        LcdVertical = 4,
        Sdf = 5
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FT_FaceRec { }

    [StructLayout(LayoutKind.Sequential)]
    public struct FT_GlyphSlotRec { }

    [StructLayout(LayoutKind.Sequential)]
    public struct FT_SizeRec { }

    [StructLayout(LayoutKind.Sequential)]
    public struct FT_CharMapRec { }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FT_Generic
    {
        public IntPtr data;
        public IntPtr finalizer;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FT_LibraryRec
    {
        public IntPtr memory;
        public int version_major;
        public int version_minor;
        public int version_patch;
        public uint num_modules;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct FT_Bitmap
    {
        public uint rows;
        public uint width;
        public int pitch;
        public byte* buffer;
        public ushort num_grays;
        public byte pixel_mode;
        public byte palette_mode;
        public IntPtr palette;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FT_Bitmap_Size32
    {
        public short height;
        public short width;
        public int size;
        public int x_ppem;
        public int y_ppem;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FT_Bitmap_Size64
    {
        public short height;
        public short width;
        public long size;
        public long x_ppem;
        public long y_ppem;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FT_Size_Metrics32
    {
        public ushort x_ppem;
        public ushort y_ppem;
        public int x_scale;
        public int y_scale;
        public int ascender;
        public int descender;
        public int height;
        public int max_advance;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FT_Size_Metrics64
    {
        public ushort x_ppem;
        public ushort y_ppem;
        public long x_scale;
        public long y_scale;
        public long ascender;
        public long descender;
        public long height;
        public long max_advance;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FT_SizeRec32
    {
        public FT_FaceRec* face;
        public FT_Generic generic;
        public FT_Size_Metrics32 metrics;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FT_SizeRec64
    {
        public FT_FaceRec* face;
        public FT_Generic generic;
        public FT_Size_Metrics64 metrics;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FT_BBox32
    {
        public int xMin;
        public int yMin;
        public int xMax;
        public int yMax;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FT_BBox64
    {
        public long xMin;
        public long yMin;
        public long xMax;
        public long yMax;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FT_FaceRec32
    {
        public int num_faces;
        public int face_index;
        public int face_flags;
        public int style_flags;
        public int num_glyphs;
        public sbyte* family_name;
        public sbyte* style_name;
        public int num_fixed_sizes;
        public FT_Bitmap_Size32* available_sizes;
        public int num_charmaps;
        public FT_CharMapRec** charmaps;
        public FT_Generic generic;
        public FT_BBox32 bbox;
        public ushort units_per_EM;
        public short ascender;
        public short descender;
        public short height;
        public short max_advance_width;
        public short max_advance_height;
        public short underline_position;
        public short underline_thickness;
        public FT_GlyphSlotRec* glyph;
        public FT_SizeRec* size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FT_FaceRec64
    {
        public long num_faces;
        public long face_index;
        public long face_flags;
        public long style_flags;
        public long num_glyphs;
        public sbyte* family_name;
        public sbyte* style_name;
        public int num_fixed_sizes;
        public FT_Bitmap_Size64* available_sizes;
        public int num_charmaps;
        public FT_CharMapRec** charmaps;
        public FT_Generic generic;
        public FT_BBox64 bbox;
        public ushort units_per_EM;
        public short ascender;
        public short descender;
        public short height;
        public short max_advance_width;
        public short max_advance_height;
        public short underline_position;
        public short underline_thickness;
        public FT_GlyphSlotRec* glyph;
        public FT_SizeRec* size;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FT_Glyph_Metrics32
    {
        public int width;
        public int height;
        public int horiBearingX;
        public int horiBearingY;
        public int horiAdvance;
        public int vertBearingX;
        public int vertBearingY;
        public int vertAdvance;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FT_Glyph_Metrics64
    {
        public long width;
        public long height;
        public long horiBearingX;
        public long horiBearingY;
        public long horiAdvance;
        public long vertBearingX;
        public long vertBearingY;
        public long vertAdvance;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FT_GlyphSlotRec32
    {
        public FT_LibraryRec* library;
        public FT_FaceRec* face;
        public FT_GlyphSlotRec* next;
        public uint glyph_index;
        public FT_Generic generic;
        public FT_Glyph_Metrics32 metrics;
        public int linearHoriAdvance;
        public int linearVertAdvance;
        public int advance_x;
        public int advance_y;
        public int format;
        public FT_Bitmap bitmap;
        public int bitmap_left;
        public int bitmap_top;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FT_GlyphSlotRec64
    {
        public FT_LibraryRec* library;
        public FT_FaceRec* face;
        public FT_GlyphSlotRec* next;
        public uint glyph_index;
        public FT_Generic generic;
        public FT_Glyph_Metrics64 metrics;
        public long linearHoriAdvance;
        public long linearVertAdvance;
        public long advance_x;
        public long advance_y;
        public int format;
        public FT_Bitmap bitmap;
        public int bitmap_left;
        public int bitmap_top;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_ft_init_freetype(out FT_LibraryRec* library);
    public static readonly d_ft_init_freetype FT_Init_FreeType = FuncLoader.LoadFunction<d_ft_init_freetype>(NativeLibrary, "FT_Init_FreeType");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_ft_done_freetype(FT_LibraryRec* library);
    public static readonly d_ft_done_freetype FT_Done_FreeType = FuncLoader.LoadFunction<d_ft_done_freetype>(NativeLibrary, "FT_Done_FreeType");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int d_ft_new_memory_face_32(FT_LibraryRec* library, byte* fileBase, int fileSize, int faceIndex, out FT_FaceRec* face);
    private static readonly d_ft_new_memory_face_32 FT_New_Memory_Face_32 = FuncLoader.LoadFunction<d_ft_new_memory_face_32>(NativeLibrary, "FT_New_Memory_Face");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int d_ft_new_memory_face_64(FT_LibraryRec* library, byte* fileBase, long fileSize, long faceIndex, out FT_FaceRec* face);
    private static readonly d_ft_new_memory_face_64 FT_New_Memory_Face_64 = FuncLoader.LoadFunction<d_ft_new_memory_face_64>(NativeLibrary, "FT_New_Memory_Face");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_ft_done_face(FT_FaceRec* face);
    public static readonly d_ft_done_face FT_Done_Face = FuncLoader.LoadFunction<d_ft_done_face>(NativeLibrary, nameof(FT_Done_Face));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint d_ft_get_char_index_32(FT_FaceRec* face, uint charCode);
    private static readonly d_ft_get_char_index_32 FT_Get_Char_Index_32 = FuncLoader.LoadFunction<d_ft_get_char_index_32>(NativeLibrary, "FT_Get_Char_Index");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint d_ft_get_char_index_64(FT_FaceRec* face, ulong charCode);
    private static readonly d_ft_get_char_index_64 FT_Get_Char_Index_64 = FuncLoader.LoadFunction<d_ft_get_char_index_64>(NativeLibrary, "FT_Get_Char_Index");

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_ft_set_pixel_sizes(FT_FaceRec* face, uint pixelWidth, uint pixelHeight);
    public static readonly d_ft_set_pixel_sizes FT_Set_Pixel_Sizes = FuncLoader.LoadFunction<d_ft_set_pixel_sizes>(NativeLibrary, nameof(FT_Set_Pixel_Sizes));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_ft_load_glyph(FT_FaceRec* face, uint glyphIndex, int loadFlags);
    public static readonly d_ft_load_glyph FT_Load_Glyph = FuncLoader.LoadFunction<d_ft_load_glyph>(NativeLibrary, nameof(FT_Load_Glyph));

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_ft_render_glyph(FT_GlyphSlotRec* slot, RenderMode renderMode);
    public static readonly d_ft_render_glyph FT_Render_Glyph = FuncLoader.LoadFunction<d_ft_render_glyph>(NativeLibrary, nameof(FT_Render_Glyph));

    public static int FT_New_Memory_Face(FT_LibraryRec* library, byte* fileBase, int fileSize, int faceIndex, out FT_FaceRec* face)
    {
        if (CLongSize == 4)
        {
            return FT_New_Memory_Face_32(library, fileBase, fileSize, faceIndex, out face);
        }

        return FT_New_Memory_Face_64(library, fileBase, fileSize, faceIndex, out face);
    }

    public static uint FT_Get_Char_Index(FT_FaceRec* face, uint charCode)
    {
        if(CLongSize == 4)
        {
            return FT_Get_Char_Index_32(face, charCode);
        }

        return FT_Get_Char_Index_64(face, charCode);
    }

    public static void CheckError(int error)
    {
        if (error == 0)
        {
            return;
        }

        throw new InvalidOperationException($"A FreeType call failed with error code {error}");
    }
}
