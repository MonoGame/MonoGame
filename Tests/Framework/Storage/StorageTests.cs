using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    public class StorageDeviceTests
    {
        const string MY_GAME = "MyGame";

        [Test]
        public void OpenContainer_ShouldReturnContainer_OnSuccess()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME);
            Assert.IsNotNull(container);
            Assert.AreEqual(MY_GAME, container.ContainerName);
            Assert.AreEqual(device, container.StorageDevice);
            Assert.AreNotEqual(0, device.TotalSpace);
            Assert.AreNotEqual(0, device.FreeSpace);
        }

        [Test]
        public void OpenContainer_ShouldThrowException_OnInvalidContainerName()
        {
            var device = new StorageDevice(PlayerIndex.One);
            Assert.Throws<ArgumentNullException>(() => device.OpenContainer(null), "A container name must be provided. (Parameter 'containerName')");
            Assert.Throws<ArgumentNullException>(() => device.OpenContainer(string.Empty), "A container name must be provided. (Parameter 'containerName')");
        }

        [Test]
        public async Task OpenContainerAsync_ShouldReturnContainer_OnSuccess()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = await device.OpenContainerAsync(MY_GAME);
            Assert.IsNotNull(container);
            Assert.AreEqual(MY_GAME, container.ContainerName);
        }

        [Test]
        public void OpenContainerAsync_ShouldThrowException_OnInvalidContainerName()
        {
            var device = new StorageDevice(PlayerIndex.One);
            Assert.ThrowsAsync<ArgumentNullException>(() => device.OpenContainerAsync(null), "A container name must be provided. (Parameter 'containerName')");
            Assert.ThrowsAsync<ArgumentNullException>(() => device.OpenContainerAsync(string.Empty), "A container name must be provided. (Parameter 'containerName')");
        }
    }
}
