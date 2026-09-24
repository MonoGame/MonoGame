// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
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
            var managedNames = Enum.GetNames<PlatformConfiguration.Key>();
            var interopNames = Enum.GetNames<Interop.PlatformConfigKey>();
            CollectionAssert.AreEqual(managedNames, interopNames);

            var managedValues = Array.ConvertAll(Enum.GetValues<PlatformConfiguration.Key>(), k => (int)k);
            var interopValues = Array.ConvertAll(Enum.GetValues<Interop.PlatformConfigKey>(), k => (int)k);
            CollectionAssert.AreEqual(managedValues, interopValues);
        }
#endif
    }
}
