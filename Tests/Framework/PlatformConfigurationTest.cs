// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Framework.Utilities;
using MonoGame.Interop;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    public class PlatformConfigurationTest
    {
#if VULKAN || DIRECTX12
        [Test]
        public void TestPlatformConfigurationSetDoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
            {
                PlatformConfiguration.Set(PlatformConfiguration.PlatformConfigKey.VulkanUniformRingbufferSize, 64 * 1024 * 1024);
                PlatformConfiguration.Set(PlatformConfiguration.PlatformConfigKey.VulkanDescriptorPoolSize, 8192);
                PlatformConfiguration.Set(PlatformConfiguration.PlatformConfigKey.VulkanInstanceExtensions, "VK_KHR_external_memory_capabilities");
                PlatformConfiguration.Set(PlatformConfiguration.PlatformConfigKey.VulkanDeviceExtensions, "VK_KHR_external_memory");
                PlatformConfiguration.Set(PlatformConfiguration.PlatformConfigKey.Dx12PreferredBlockSize, 8 * 1024 * 1024);
                PlatformConfiguration.Set(PlatformConfiguration.PlatformConfigKey.Dx12MaxUploadBufferPoolSize, 64);
            });
        }

        [Test]
        public void TestNativeAndManagedConfigKeysMatch()
        {
            var keyCount = MGG.Config_GetKeyCount();
            var nativeKeys = new Dictionary<string, int>(keyCount);

            for (var i = 0; i < keyCount; i++)
            {
                MGG.Config_GetKeyDetails(i, out nint configKeyPtr, out int configValue);
                var configKey = Marshal.PtrToStringUTF8(configKeyPtr)!;

                nativeKeys[configKey] = configValue;
            }

            var managedKeys = Enum
                .GetValues<PlatformConfiguration.PlatformConfigKey>()
                .ToDictionary(k => k.ToString(), k => (int)k);

            CollectionAssert.AreEquivalent(managedKeys, nativeKeys);
        }
#endif
    }
}
