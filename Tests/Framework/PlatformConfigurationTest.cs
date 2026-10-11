// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Linq;
using MonoGame.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    public class PlatformConfigurationTest
    {
#if VULKAN || DIRECTX12 || DESKTOPGL4
        [Test]
        public void PlatformConfiguration_SetWithValidValue_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                PlatformConfiguration.Set(PlatformConfiguration.Key.VulkanUniformRingbufferSize, 64 * 1024 * 1024);
                PlatformConfiguration.Set(PlatformConfiguration.Key.VulkanDescriptorPoolSize, 8192);
                PlatformConfiguration.Set(PlatformConfiguration.Key.VulkanInstanceExtensions, "VK_KHR_external_memory_capabilities");
                PlatformConfiguration.Set(PlatformConfiguration.Key.VulkanDeviceExtensions, "VK_KHR_external_memory");
                PlatformConfiguration.Set(PlatformConfiguration.Key.Dx12PreferredBlockSize, 8 * 1024 * 1024);
                PlatformConfiguration.Set(PlatformConfiguration.Key.Dx12MaxUploadBufferPoolSize, 64);
            });
        }
#endif

        [Test]
        public void PlatformConfiguration_SetWithInvalidValue_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                PlatformConfiguration.Set((PlatformConfiguration.Key)int.MaxValue, 64 * 1024 * 1024);
            });
        }

#if VULKAN || DIRECTX12 || DESKTOPGL4
        [Test]
        public void PlatformConfiguration_EnumValues_HaveMatchingValuesInInterop()
        {
            var managedKeys = Enum.GetValues<PlatformConfiguration.Key>();
            var interopKeys = Enum.GetValues<Interop.PlatformConfigKey>();

            CollectionAssert.AreEqual(
                managedKeys.Select(k => k.ToString()),
                interopKeys.Select(k => k.ToString()));

            CollectionAssert.AreEqual(
                managedKeys.Select(k => (int)k),
                interopKeys.Select(k => (int)k));
        }
#endif
    }
}
