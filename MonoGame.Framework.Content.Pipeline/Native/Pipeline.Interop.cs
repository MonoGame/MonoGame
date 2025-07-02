using System;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Content.Pipeline.Interop;

[StructLayout(LayoutKind.Sequential)]
internal readonly unsafe struct MGCP_Bitmap
{
    public readonly int width;
    public readonly int height;
    public readonly bool is_16_bit;
    public readonly IntPtr data;
}

internal static unsafe partial class MGCP
{
    private const string PipelineNativeDLL = "monogame.native.pipeline";

    [DllImport(PipelineNativeDLL, EntryPoint = "MP_ImportBitmap", ExactSpelling = true)]
    public static extern MGCP_Bitmap* MP_ImportBitmap([MarshalAs(UnmanagedType.LPStr)] string fullpathToFile);

    [DllImport(PipelineNativeDLL, EntryPoint = "MP_FreeBitmap", ExactSpelling = true)]
    public static extern void MP_FreeBitmap(MGCP_Bitmap* bitmap);
}
