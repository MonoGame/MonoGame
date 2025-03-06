using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework;
using System.ComponentModel.Design;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    public class StorageDeviceTests
    {
        const string MY_GAME = "MyGame";

        [Test]
        public void IsConnected_ShouldReturnFalse_OnException()
        {
            var device = new StorageDevice(null);

            // We haven't opened a container yet, so should be false
            Assert.IsFalse(device.IsConnected);
        }

        [Test]
        public void OpenContainer_ShouldReturnContainer_OnSuccess()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME);
            Assert.IsNotNull(container);
            Assert.AreEqual(MY_GAME, container.ContainterName);
            Assert.AreEqual(device, container.StorageDevice);
            Assert.AreEqual(true, device.IsConnected);
            Assert.AreNotEqual(0, device.TotalSpace);
            Assert.AreNotEqual(0, device.FreeSpace);
        }

        [Test]
        public void OpenContainer_ShouldThrowException_OnInvalidContainerName()
        {
            var device = new StorageDevice(PlayerIndex.One);
            Assert.Throws<ArgumentNullException>(() => device.OpenContainer(null), "Value cannot be null. (Parameter 'ContainerName')");
            Assert.Throws<ArgumentException>(() => device.OpenContainer(string.Empty), "The value cannot be an empty string. (Parameter 'ContainerName')");
        }

        [Test]
        public async Task OpenContainerAsync_ShouldReturnContainer_OnSuccess()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = await device.OpenContainerAsync(MY_GAME);
            Assert.IsNotNull(container);
            Assert.AreEqual(MY_GAME, container.ContainterName);
        }

        [Test]
        public void OpenContainerAsync_ShouldThrowException_OnInvalidContainerName()
        {
            var device = new StorageDevice(PlayerIndex.One);
            Assert.ThrowsAsync<ArgumentNullException>(() => device.OpenContainerAsync(null), "Value cannot be null. (Parameter 'ContainerName')");
            Assert.ThrowsAsync<ArgumentException>(() => device.OpenContainerAsync(string.Empty), "The value cannot be an empty string. (Parameter 'ContainerName')");
        }
    }
}
