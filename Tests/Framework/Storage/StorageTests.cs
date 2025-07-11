using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

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

        [Test]
        public void CreateFile_ShouldCreateFileInMemory_AndPersistContents()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME);
            string fileName = "testfile.txt";
            string fileContent = "Hello, MonoGame!";

            // Create file and write content
            using (var stream = container.CreateFile(fileName))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(fileContent);
            } // stream disposed here

            // Now the file should exist
            Assert.IsTrue(container.FileExists(fileName));
            using (var readStream = container.OpenFile(fileName, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(readStream))
            {
                string readContent = reader.ReadToEnd();
                Assert.AreEqual(fileContent, readContent);
            }
        }

        [Test]
        public void DeleteFile_ShouldRemoveFileFromMemory()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME);
            string fileName = "deletefile.txt";
            string fileContent = "Delete me!";

            // Create file and write content
            using (var stream = container.CreateFile(fileName))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(fileContent);
            }

            // File should exist
            Assert.IsTrue(container.FileExists(fileName));

            // Delete the file
            container.DeleteFile(fileName);

            // File should not exist
            Assert.IsFalse(container.FileExists(fileName));
        }

        [Test]
        public void CreateDirectory_ShouldAddDirectoryToMemory()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME);
            string dirName = "TestDir";
            container.CreateDirectory(dirName);
            Assert.IsTrue(container.DirectoryExists(dirName));
        }

        [Test]
        public void DeleteDirectory_ShouldRemoveDirectoryFromMemory()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME);
            string dirName = "DeleteDir";
            container.CreateDirectory(dirName);
            Assert.IsTrue(container.DirectoryExists(dirName));
            container.DeleteDirectory(dirName);
            Assert.IsFalse(container.DirectoryExists(dirName));
        }

        [Test]
        public void GetDirectoryNames_ShouldReturnAllCreatedDirectories()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME);
            string[] dirs = { "DirA", "DirB", "DirC" };
            foreach (var d in dirs)
                container.CreateDirectory(d);
            var found = container.GetDirectoryNames();
            foreach (var d in dirs)
                Assert.IsTrue(found.Any(x => x == d));
        }

        [Test]
        public void DirectoryExists_ShouldReturnFalseForNonexistentDirectory()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME);
            Assert.IsFalse(container.DirectoryExists("NoSuchDir"));
        }

        [Test]
        public void GetDirectoryNames_WithPattern_ShouldFilterResults()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME);
            container.CreateDirectory("Alpha");
            container.CreateDirectory("Beta");
            container.CreateDirectory("Gamma");
            var filtered = container.GetDirectoryNames("A*");
            Assert.IsTrue(filtered.Any(x => x == "Alpha"));
            Assert.IsFalse(filtered.Any(x => x == "Beta"));
        }

        [Test]
        public void GetFileNames_ShouldReturnAllCreatedFiles()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME);
            string[] files = { "fileA.txt", "fileB.txt", "fileC.txt" };
            foreach (var f in files)
            {
                using (var stream = container.CreateFile(f))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("data");
                }
            }
            var found = container.GetFileNames();
            foreach (var f in files)
                Assert.IsTrue(found.Any(x => x == f));
        }

        [Test]
        public void GetFileNames_WithPattern_ShouldFilterResults()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME);
            container.CreateFile("Alpha.txt").Dispose();
            container.CreateFile("Beta.txt").Dispose();
            container.CreateFile("Gamma.txt").Dispose();
            var filtered = container.GetFileNames("A*");
            Assert.IsTrue(filtered.Any(x => x == "Alpha.txt"));
            Assert.IsFalse(filtered.Any(x => x == "Beta.txt"));
        }

        [Test]
        public void SaveData_And_Reload_ShouldRestoreFilesAndDirectories()
        {
            var device = new StorageDevice(PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME);
            string[] dirs = { "Dir1", "Dir2" };
            string[] files = { "file1.txt", "file2.txt" };
            string fileContent = "Persisted!";

            // Create directories
            foreach (var d in dirs)
                container.CreateDirectory(d);
            // Create files
            foreach (var f in files)
            {
                using (var stream = container.CreateFile(f))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(fileContent);
                }
            }

            // Save (flush to disk)
            container.SaveData();

            // Wait for async save to complete
            while (container.IsProcessing)
                System.Threading.Thread.Sleep(10);

            // Simulate reload: create a new container instance
            var device2 = new StorageDevice(PlayerIndex.One);
            var container2 = device2.OpenContainer(MY_GAME);
            container2.LoadData();

            // Wait for async load to complete
            while (container2.IsProcessing)
                System.Threading.Thread.Sleep(10);

            // Check directories
            var foundDirs = container2.GetDirectoryNames();
            foreach (var d in dirs)
                Assert.IsTrue(foundDirs.Any(x => x == d), $"Directory '{d}' missing after reload");

            // Check files and contents
            var foundFiles = container2.GetFileNames();
            foreach (var f in files)
            {
                Assert.IsTrue(foundFiles.Any(x => x == f), $"File '{f}' missing after reload");
                using (var stream = container2.OpenFile(f, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(stream))
                {
                    string readContent = reader.ReadToEnd();
                    Assert.AreEqual(fileContent, readContent, $"File '{f}' content mismatch after reload");
                }
            }
        }
    }
}