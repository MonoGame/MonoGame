// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace FreeTypeAPI
{
    enum FT_Pixel_Mode
    {
        FT_PIXEL_MODE_NONE = 0,
        FT_PIXEL_MODE_MONO,
        FT_PIXEL_MODE_GRAY,
        FT_PIXEL_MODE_GRAY2,
        FT_PIXEL_MODE_GRAY4,
        FT_PIXEL_MODE_LCD,
        FT_PIXEL_MODE_LCD_V,
        FT_PIXEL_MODE_BGRA,

        FT_PIXEL_MODE_MAX
    };

    enum FT_Render_Mode
    {
        FT_RENDER_MODE_NORMAL = 0,
        FT_RENDER_MODE_LIGHT,
        FT_RENDER_MODE_MONO,
        FT_RENDER_MODE_LCD,
        FT_RENDER_MODE_LCD_V,
        FT_RENDER_MODE_SDF,

        FT_RENDER_MODE_MAX
    }

    enum FT_Glyph_Format
    {
        None = 0,
        Composite = ('c' << 24 | 'o' << 16 | 'm' << 8 | 'p'),
        Bitmap = ('b' << 24 | 'i' << 16 | 't' << 8 | 's'),
        Outline = ('o' << 24 | 'u' << 16 | 't' << 8 | 'l'),
        Plotter = ('p' << 24 | 'l' << 16 | 'o' << 8 | 't')
    }

    [UnsafeValueType]
    unsafe struct FT_Library
    {
        public nint memory;

        public int version_major;
        public int version_minor;
        public int version_patch;

        public uint num_modules;
    };

    struct FT_Generic
    {
        public nint data;
        public nint finalizer;
    }

    struct FT_BBox
    {
        public CLong xMin, yMin, xMax, yMax;
    }

    struct FT_ListRec
    {
        public nint head, tail;
    }

    struct FT_Vector
    {
        public CLong x, y;
    }

    struct FT_Matrix
    {
        public CLong xx, xy, yx, yy;
    }

    struct FT_Glyph_Metrics
    {
        public CLong width, height, horiBearingX, horiBearingY, horiAdvance, vertBearingX, vertBearingY, vertAdvance;
    }

    struct FT_Size_Metrics
    {
        public ushort x_ppem;
        public ushort y_ppem;

        public CLong x_scale;
        public CLong y_scale;

        public CLong ascender;
        public CLong descender;
        public CLong height;
        public CLong max_advance;
    }

    unsafe struct FT_Size
    {
        public FT_Face* face;
        public FT_Generic generic;
        public FT_Size_Metrics metrics;
        nint inter;
    }

    unsafe struct FT_Bitmap
    {
        public uint rows;
        public uint width;
        public int pitch;
        public byte* buffer;
        public ushort num_grays;
        public byte pixel_mode;
        public byte palette_mode;
        public nint palette;
    }

    unsafe struct  FT_Outline
    {
        public ushort n_contours;
        public ushort n_points;

        public FT_Vector*  points;
        public byte* tags;
        public ushort*  contours;

        public int flags;
    }

    struct  FT_SubGlyph
    {
        public int index;
        public ushort flags;
        public int arg1;
        public int arg2;
        FT_Matrix transform;
    }

    unsafe struct FT_Face
    {
        public CLong num_faces;
        public CLong face_index;

        public CLong face_flags;
        public CLong style_flags;

        public CLong num_glyphs;

        public nint family_name;
        public nint style_name;

        public int num_fixed_sizes;
        public nint available_sizes;

        public int num_charmaps;
        public nint charmaps;

        public FT_Generic generic;

        public FT_BBox bbox;

        public ushort units_per_EM;
        public short ascender;
        public short descender;
        public short height;

        public short max_advance_width;
        public short max_advance_height;

        public short underline_position;
        public short underline_thickness;

        public FT_GlyphSlot* glyph;
        public FT_Size* size;
        public nint charmap;

        public nint driver;
        public nint memory;
        public nint stream;

        public FT_ListRec sizes_list;

        public FT_Generic autohint;
        public nint extensions;

        nint intern;
    };

    unsafe struct FT_GlyphSlot
    {
        public FT_Library* library;
        public FT_Face* face;
        public FT_GlyphSlot* next;
        public uint glyph_index; /* new in 2.10; was reserved previously */
        public FT_Generic generic;

        public FT_Glyph_Metrics metrics;
        public CLong linearHoriAdvance;
        public CLong linearVertAdvance;
        public FT_Vector advance;

        public FT_Glyph_Format format;

        public FT_Bitmap bitmap;
        public int bitmap_left;
        public int bitmap_top;

        public FT_Outline outline;

        public uint num_subglyphs;
        public FT_SubGlyph* subglyphs;

        public nint control_data;
        public CLong control_len;

        public CLong lsb_delta;
        public CLong rsb_delta;

        public nint other;

        nint intern;
    }

    unsafe partial class FreeType
    {
        private const string Library = "freetype";

        [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int FT_Init_FreeType(out FT_Library* library);

        [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int FT_Done_FreeType(FT_Library* library);

        [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int FT_New_Face(FT_Library* library, string filepathname, CLong face_index, out FT_Face* aface);

        [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int FT_Done_Face(FT_Face* aface);

        [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial uint FT_Get_Char_Index(FT_Face* face, CULong charcode);

        [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int FT_Load_Glyph(FT_Face* face, uint glyph_index, int load_flags = 0);

        [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int FT_Render_Glyph(FT_GlyphSlot* slot, FT_Render_Mode render_mode = FT_Render_Mode.FT_RENDER_MODE_NORMAL);

        [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial int FT_Set_Char_Size(FT_Face* face, CLong char_width, CLong char_height,  uint horz_resolution, uint vert_resolution);
    }
}
