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
    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGI_ReadRGBA", ExactSpelling = true)]
    public static extern void ReadRGBA(
        byte* data,
        int dataBytes,
        bool zeroTransparentPixels,
        out int width,
        out int height,
        out byte* rgba);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGI_FreeRGBA", ExactSpelling = true)]
    public static extern void FreeRGBA(
        byte* rgba);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGI_WriteJpg", ExactSpelling = true)]
    public static extern void WriteJpg(
        byte* data,
        int dataBytes,
        int width,
        int height,
        int quality,
        out byte* jpg,
        out int jpgBytes);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGI_WritePng", ExactSpelling = true)]
    public static extern void WritePng(
        byte* data,
        int dataBytes,
        int width,
        int height,
        out byte* png,
        out int pngBytes);
}


