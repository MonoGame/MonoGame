// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Contains native graphics API handles for XR or other external interop.
/// <see cref="PhysicalDevice"/>, <see cref="LogicalDevice"/> and <see cref="Queue"/> are always populated for the native backends (Vulkan and DX12).
/// Other fields depend on <see cref="Backend"/>.
/// </summary>
public readonly struct NativeGraphicsHandles
{
    /// <summary>
    /// The active graphics backend.
    /// </summary>
    public readonly GraphicsBackend Backend;

    /// <summary>
    /// Handle for the session between the application and the graphics API.
    /// </summary>
    /// <remarks>
    /// Vulkan: <c>VkInstance</c> handle.
    /// Others: Zero.
    /// </remarks>
    public readonly nint Instance;

    /// <summary>
    /// The physical adapter handle.
    /// </summary>
    /// <remarks>
    /// Vulkan: <c>VkPhysicalDevice</c> handle.
    /// DX12: <c>IDXGIAdapter1*</c>.
    /// Others: Zero.
    /// </remarks>
    public readonly nint PhysicalDevice;

    /// <summary>
    /// The logical graphics device handle.
    /// </summary>
    /// <remarks>
    /// Vulkan: <c>VkDevice</c> handle.
    /// DX12: <c>ID3D12Device*</c>.
    /// Others: Zero.
    /// </remarks>
    public readonly nint LogicalDevice;

    /// <summary>
    /// The graphics queue handle.
    /// </summary>
    /// <remarks>
    /// Vulkan: <c>VkQueue</c> handle.
    /// DX12: <c>ID3D12CommandQueue*</c>.
    /// Others: Zero.
    /// </remarks>
    public readonly nint Queue;

    /// <summary>
    /// Index of the queue family/type.
    /// </summary>
    /// <remarks>
    /// Vulkan: queue family index.
    /// Others: Zero.
    /// </remarks>
    public readonly int QueueFamilyIndex;

    /// <summary>
    /// Index of the queue within its family/type.
    /// </summary>
    /// <remarks>
    /// Vulkan: queue index within family.
    /// Others: Zero.
    /// </remarks>
    public readonly int QueueIndex;

    /// <summary>
    /// Creates a new <see cref="NativeGraphicsHandles"/> instance that holds the provided handles (pointers).
    /// </summary>
    public NativeGraphicsHandles(
        GraphicsBackend backend,
        nint instance,
        nint physicalDevice,
        nint device,
        nint queue,
        int queueFamilyIndex,
        int queueIndex)
    {
        Backend = backend;
        Instance = instance;
        PhysicalDevice = physicalDevice;
        LogicalDevice = device;
        Queue = queue;
        QueueFamilyIndex = queueFamilyIndex;
        QueueIndex = queueIndex;
    }
}
