// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Runtime.InteropServices;
using MonoGame.Interop;

namespace MonoGame.Framework.Content.Pipeline.Interop;


[MGHandle] internal readonly struct MGCP_EffectCompiler { }


[StructLayout(LayoutKind.Sequential)]
internal unsafe struct MGCP_EffectOptions
{
    public nint EffectFilePath;
    public nint Defines;
    public nint ProfileOrPlatform;

    [MarshalAs(UnmanagedType.I1)]
    public bool Debug;

    public nint DebugSymbolPath;

    public void Create(string effectFilePath, string? defines, string profileOrPlatform, bool debug, string outputPath)
    {
        EffectFilePath = Marshal.StringToCoTaskMemUTF8(effectFilePath);
        Defines = Marshal.StringToCoTaskMemUTF8(defines);
        ProfileOrPlatform = Marshal.StringToCoTaskMemUTF8(profileOrPlatform);
        Debug = debug;

        if (!debug)
            DebugSymbolPath = 0;
        else
        {
            // The shader debug symbols if needed will be written here.
            string debugSymbolPath = Path.Combine(outputPath, "ShaderSymbols");
            if (!Directory.Exists(debugSymbolPath))
                Directory.CreateDirectory(debugSymbolPath);

            DebugSymbolPath = Marshal.StringToCoTaskMemUTF8(debugSymbolPath);
        }
        
    }

    public void Destroy()
    {

    }
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct MGCP_Effect
{
    public int Size;
    public int Version;

    private nint bytecode;
    private nint errorsAndWarnings;
    private nint handle;

    public string? GetErrorsAndWarnings()
    {
        if (errorsAndWarnings == 0)
            return null;

        return Marshal.PtrToStringUTF8(errorsAndWarnings);
    }

    public byte[] GetBytecode()
    {
        if (this.bytecode == 0 || Size == 0)
            return Array.Empty<byte>();

        var bytecode = new byte[Size];
        Marshal.Copy(this.bytecode, bytecode, 0, Size);
        return bytecode;
    }
}


internal static unsafe partial class MGCP
{
    [DllImport("mgpipeline", EntryPoint = "MP_EffectCompiler_Compile", ExactSpelling = true)]
    public static extern int EffectCompiler_Compile(ref MGCP_EffectOptions options, out MGCP_Effect result);

    [DllImport("mgpipeline", EntryPoint = "MP_EffectCompiler_Free", ExactSpelling = true)]
    public static extern void EffectCompiler_Free(ref MGCP_Effect result);
}
