using System;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Content.Pipeline.Interop;

internal enum TextureType
{
    Rgba8 = 0,
    Rgba16,
    RgbaF,
}

internal enum TextureFormat
{
    Unknown = 0,
    Jpeg,
    Png,
    Tga,
    Bmp,
    Psd,
    Gif,
    Hdr,
    Pic,
    Pnm,
};

[StructLayout(LayoutKind.Sequential)]
internal struct MGCP_Bitmap
{
    public int width;
    public int height;
    public TextureType type;
    public TextureFormat format;
    public IntPtr data;
}

internal static unsafe partial class MGCP
{
    private const string PipelineNativeDLL = "mgpipeline";

    [DllImport(PipelineNativeDLL, EntryPoint = "MP_ImportBitmap", ExactSpelling = true)]
    public static extern IntPtr MP_ImportBitmap([MarshalAs(UnmanagedType.LPStr)] string importPath, ref MGCP_Bitmap bitmap);

    [DllImport(PipelineNativeDLL, EntryPoint = "MP_FreeBitmap", ExactSpelling = true)]
    public static extern void MP_FreeBitmap(ref MGCP_Bitmap bitmap);

    [DllImport(PipelineNativeDLL, EntryPoint = "MP_ResizeBitmap", ExactSpelling = true)]
    public static extern IntPtr MP_ResizeBitmap(ref MGCP_Bitmap srcBitmap, ref MGCP_Bitmap dstBitmap);

    [DllImport(PipelineNativeDLL, EntryPoint = "MP_ExportBitmap", ExactSpelling = true)]
    public static extern IntPtr MP_ExportBitmap(ref MGCP_Bitmap bitmap, [MarshalAs(UnmanagedType.LPStr)] string exportPath);
}
