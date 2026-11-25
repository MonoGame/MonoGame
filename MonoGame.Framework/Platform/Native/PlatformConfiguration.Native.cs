// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;

namespace MonoGame.Framework;

public static partial class PlatformConfiguration
{
    /// <summary>
    /// Configuration keys for native platform-specific engine parameters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Keys prefixed with <c>Vulkan</c> apply only to the Vulkan backend.
    /// Keys prefixed with <c>Dx12</c> apply only to the DX12 backend.
    /// Using a key that does not apply to the current backend is a no-op.
    /// </para>
    /// <para>
    /// Settings should be applied in <c>Program.Main()</c> before creating the <see cref="Microsoft.Xna.Framework.Game"/> instance.
    /// Native platforms read these values before or during device initialization.
    /// </para>
    /// </remarks>
    public enum Key
    {
        #region Vulkan

        /// <summary>
        /// Space-separated Vulkan instance extension names to enable during <c>vkCreateInstance</c>.
        /// <para>
        /// <b>Type:</b> <see cref="string"/>. <b>Default:</b> <c>null</c>.
        /// </para>
        /// </summary>
        VulkanInstanceExtensions = 1,

        /// <summary>
        /// Space-separated Vulkan device extension names to enable during <c>vkCreateDevice</c>.
        /// <para>
        /// <b>Type:</b> <see cref="string"/>. <b>Default:</b> <c>null</c>.
        /// </para>
        /// </summary>
        VulkanDeviceExtensions = 2,

        /// <summary>
        /// Size in bytes of the per-frame uniform constant ring buffer.
        /// <para>
        /// <b>Type:</b> <see cref="int"/>. <b>Default:</b> 33554432 (32 MiB).
        /// </para>
        /// </summary>
        VulkanUniformRingbufferSize = 3,

        /// <summary>
        /// Maximum descriptor pool capacity per shader type.
        /// <para>
        /// <b>Type:</b> <see cref="int"/>. <b>Default:</b> 16384.
        /// </para>
        /// </summary>
        VulkanDescriptorPoolSize = 4,

        #endregion Vulkan

        #region DX12

        /// <summary>
        /// Preferred block size in bytes for the D3D12 Memory Allocator.
        /// <para>
        /// (Sets <c>D3D12MA::ALLOCATOR_DESC::PreferredBlockSize</c>.)
        /// </para>
        /// <para>
        /// <b>Type:</b> <see cref="int"/>. <b>Default:</b> 0. (Left at 0, the backend uses D3D12MA's default of 64 MiB.)
        /// </para>
        /// </summary>
        Dx12PreferredBlockSize = 5,

        /// <summary>
        /// Maximum number of pooled upload buffers before stalling until eviction or forcing new allocation.
        /// <para>
        /// <b>Type:</b> <see cref="int"/>. <b>Default:</b> 32.
        /// </para>
        /// </summary>
        Dx12MaxUploadBufferPoolSize = 6,

        #endregion DX12
    }

    /// <summary>
    /// Sets an <see cref="int"/> configuration value.
    /// </summary>
    public static void Set(Key key, int value)
    {
        MGC.Config_SetInt((PlatformConfigKey)key, value);
    }

    /// <summary>
    /// Sets a <see cref="string"/> configuration value.
    /// </summary>
    public static void Set(Key key, string value)
    {
        MGC.Config_SetString((PlatformConfigKey)key, value);
    }
}
