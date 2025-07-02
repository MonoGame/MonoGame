using System;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Content.Pipeline.Interop;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct MGCP_Bitmap
{
    public int width;
    public int height;
    public bool is_16_bit;
    public IntPtr data;
}

internal static unsafe partial class MGCP
{
    private const string PipelineNativeDLL = "mgpipeline";

    [DllImport(PipelineNativeDLL, EntryPoint = "MP_ImportBitmap", ExactSpelling = true)]
    public static extern IntPtr MP_ImportBitmap([MarshalAs(UnmanagedType.LPStr)] string fullpathToFile, ref MGCP_Bitmap bitmap);

    [DllImport(PipelineNativeDLL, EntryPoint = "MP_FreeBitmap", ExactSpelling = true)]
    public static extern void MP_FreeBitmap(ref MGCP_Bitmap bitmap);
}
