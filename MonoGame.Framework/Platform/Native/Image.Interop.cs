// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;


namespace MonoGame.Interop;


/// <summary>
/// MonoGame native calls for high performance reading and writing of images.
/// </summary>
internal static unsafe partial class MGI
{
    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGI_ReadRGBA", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void ReadRGBA(
        byte* data,
        int dataBytes,
        [MarshalAs(UnmanagedType.U1)] bool zeroTransparentPixels,
        out int width,
        out int height,
        out byte* rgba);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGI_WriteJpg", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void WriteJpg(
        byte* data,
        int dataBytes,
        int width,
        int height,
        int quality,
        out byte* jpg,
        out int jpgBytes);

    [LibraryImport(MGP.MonoGameNativeDLL, EntryPoint = "MGI_WritePng", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void WritePng(
        byte* data,
        int dataBytes,
        int width,
        int height,
        out byte* png,
        out int pngBytes);
}


