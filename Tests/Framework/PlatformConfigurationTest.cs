// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    public class PlatformConfigurationTest
    {
        [Test]
        public void TestPlatformConfigurationSetDoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                PlatformConfiguration.Set(PlatformConfigKey.VulkanUniformRingbufferSize, 64 * 1024 * 1024);
                PlatformConfiguration.Set(PlatformConfigKey.VulkanDescriptorPoolSize, 8192);
                PlatformConfiguration.Set(PlatformConfigKey.VulkanInstanceExtensions, "VK_KHR_external_memory_capabilities");
                PlatformConfiguration.Set(PlatformConfigKey.VulkanDeviceExtensions, "VK_KHR_external_memory");
                PlatformConfiguration.Set(PlatformConfigKey.Dx12PreferredBlockSize, 8 * 1024 * 1024);
                PlatformConfiguration.Set(PlatformConfigKey.Dx12MaxUploadBufferPoolSize, 64);
            });
        }
    }
}
