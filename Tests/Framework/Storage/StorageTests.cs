using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using NUnit.Framework;
using System;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    public class StorageDeviceTests
    {
        const string MY_GAME = "MyGame";
        const string ALL_PLAYERS = "AllPlayers";

        [Test]
        public void OpenContainer_WhenPlayerIndexIsNotNull_ShouldReturnContainer_OnSuccess()
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
        public void OpenContainer_WhenPlayerIndexIsNotNull_ShouldThrowException_OnInvalidContainerName()
        {
            var device = new StorageDevice(PlayerIndex.One);
            Assert.Throws<ArgumentNullException>(() => device.OpenContainer(null), "A container name must be provided. (Parameter 'containerName')");
            Assert.Throws<ArgumentNullException>(() => device.OpenContainer(string.Empty), "A container name must be provided. (Parameter 'containerName')");
        }

        [Test]
        public void StoragePath_WhenPlayerIndexIsNull_ShouldContainAllPlayers_OnSuccess()
        {
            var device = new StorageDevice(null);
            var container = device.OpenContainer(MY_GAME);
            Assert.IsNotNull(container._storagePath);
            Assert.IsNotEmpty(container._storagePath);
            Assert.AreEqual(true, container._storagePath.EndsWith(ALL_PLAYERS));
        }
    }
}