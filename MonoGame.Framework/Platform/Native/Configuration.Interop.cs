// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Framework;
using System.Runtime.InteropServices;
using static MonoGame.Framework.PlatformConfiguration;

namespace MonoGame.Interop;

internal enum PlatformConfigKey
{
    VulkanInstanceExtensions = 1,
    VulkanDeviceExtensions = 2,
    VulkanUniformRingbufferSize = 3,
    VulkanDescriptorPoolSize = 4,
    Dx12PreferredBlockSize = 5,
    Dx12MaxUploadBufferPoolSize = 6,
}

internal static unsafe partial class MGC
{

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGC_Config_SetInt", ExactSpelling = true)]
    public static extern void Config_SetInt(PlatformConfigKey key, int value);

    [DllImport(MGP.MonoGameNativeDLL, EntryPoint = "MGC_Config_SetString", ExactSpelling = true)]
    public static extern void Config_SetString(PlatformConfigKey key, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
}
