// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Framework;
using System.Runtime.InteropServices;

namespace MonoGame.Interop;

internal static unsafe partial class MGC
{

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGC_Config_SetInt", ExactSpelling = true)]
    public static extern void Config_SetInt(PlatformConfigKey key, int value);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGC_Config_SetString", ExactSpelling = true)]
    public static extern void Config_SetString(PlatformConfigKey key, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
}
