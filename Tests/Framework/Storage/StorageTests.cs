// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if NATIVE

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Storage;

namespace MonoGame.Tests.Framework
{
    [TestFixture]
    [Category("Storage")]
    public class StorageDeviceTests
    {
        const string MY_GAME_TITLE = "MyGame";
        const string MY_GAME_CONTAINER = "Save1";

        const long ContainerSize = 256 * 1024;

        [Test]
        public void OpenContainer_WhenPlayerIndexIsNotNull_ShouldReturnContainer_OnSuccess()
        {
            var device = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME_CONTAINER, ContainerSize);
            Assert.IsNotNull(container);
            Assert.AreEqual(MY_GAME_CONTAINER, container.ContainerName);
            Assert.AreEqual(device, container.StorageDevice);
            Assert.AreNotEqual(0, device.TotalSpace);
            Assert.AreNotEqual(0, device.FreeSpace);

            container.Dispose();
            device.Dispose();
        }

        [Test]
        public void OpenContainer_WhenPlayerIndexIsNotNull_ShouldThrowException_OnInvalidContainerName()
        {
            var device = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
            Assert.Throws<ArgumentNullException>(() => device.OpenContainer(null,1), "A container name must be provided. (Parameter 'containerName')");
            Assert.Throws<ArgumentNullException>(() => device.OpenContainer(string.Empty,1), "A container name must be provided. (Parameter 'containerName')");
            Assert.Throws<ArgumentOutOfRangeException>(() => device.OpenContainer("foo", -1), "A container must provide a greater than 0 required file size.");
            Assert.Throws<ArgumentOutOfRangeException>(() => device.OpenContainer("foo", 0), "A container must provide a greater than 0 required file size.");
            device.Dispose();
        }

        /*
        [Test]
        public void StoragePath_WhenPlayerIndexIsNull_ShouldContainAllPlayers_OnSuccess()
        {
            var device = new StorageDevice(null);
            var container = device.OpenContainer(MY_GAME, ContainerSize);
            Assert.IsNotNull(container.StoragePath);
            Assert.IsNotEmpty(container.StoragePath);
            Assert.AreEqual(true, container.StoragePath.EndsWith(ALL_PLAYERS));
        }
        */

        [Test]
        public void CreateFile_ShouldCreateFileInMemory_AndPersistContents()
        {
            var device = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME_CONTAINER, ContainerSize);
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
            using (var readStream = container.OpenFile(fileName, FileMode.Open))
            using (var reader = new StreamReader(readStream))
            {
                string readContent = reader.ReadToEnd();
                Assert.AreEqual(fileContent, readContent);
            }

            container.Dispose();
            device.Dispose();
        }

        [Test]
        public void DeleteFile_ShouldRemoveFileFromMemory()
        {
            var device = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME_CONTAINER, ContainerSize);
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

            container.Dispose();
            device.Dispose();
        }

        [Test]
        public void CreateDirectory_ShouldAddDirectoryToMemory()
        {
            var device = new StorageDevice(MY_GAME_TITLE,PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME_CONTAINER, ContainerSize);
            string dirName = "TestDir";
            container.CreateDirectory(dirName);
            Assert.IsTrue(container.DirectoryExists(dirName));

            container.Dispose();
            device.Dispose();
        }

        [Test]
        public void DeleteDirectory_ShouldRemoveDirectoryFromMemory()
        {
            var device = new StorageDevice(MY_GAME_TITLE,PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME_CONTAINER, ContainerSize);
            string dirName = "DeleteDir";
            container.CreateDirectory(dirName);
            Assert.IsTrue(container.DirectoryExists(dirName));
            container.DeleteDirectory(dirName);
            Assert.IsFalse(container.DirectoryExists(dirName));

            container.Dispose();
            device.Dispose();
        }

        [Test]
        public void GetDirectoryNames_ShouldReturnAllCreatedDirectories()
        {
            var device = new StorageDevice(MY_GAME_TITLE,PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME_CONTAINER, ContainerSize);
            string[] dirs = { "DirA", "DirB", "DirC" };
            foreach (var d in dirs)
                container.CreateDirectory(d);
            var found = container.GetDirectoryNames();
            foreach (var d in dirs)
                Assert.IsTrue(found.Any(x => x.Equals(d, StringComparison.OrdinalIgnoreCase)));

            container.Dispose();
            device.Dispose();
        }

        [Test]
        public void DirectoryExists_ShouldReturnFalseForNonexistentDirectory()
        {
            var device = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME_CONTAINER, ContainerSize);
            Assert.IsFalse(container.DirectoryExists("NoSuchDir"));
            container.Dispose();
            device.Dispose();
        }

        [Test]
        public void GetDirectoryNames_WithPattern_ShouldFilterResults()
        {
            var device = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME_CONTAINER, ContainerSize);
            string[] dirs = { "DirA", "DirB", "DirC" };
            foreach (var d in dirs)
                container.CreateDirectory(d);

            var found = container.GetDirectoryNames("*");
            Assert.AreEqual(dirs.Length, found.Length);
            foreach (var d in dirs)
                Assert.IsTrue(found.Any(x => x.Equals(d, StringComparison.OrdinalIgnoreCase)));

            found = container.GetDirectoryNames("Dir*");
            Assert.AreEqual(dirs.Length, found.Length);
            foreach (var d in dirs)
                Assert.IsTrue(found.Any(x => x.Equals(d, StringComparison.OrdinalIgnoreCase)));

            found = container.GetDirectoryNames("D*ir?");
            Assert.AreEqual(dirs.Length, found.Length);
            foreach (var d in dirs)
                Assert.IsTrue(found.Any(x => x.Equals(d, StringComparison.OrdinalIgnoreCase)));

            found = container.GetDirectoryNames("Dir?");
            Assert.AreEqual(dirs.Length, found.Length);
            foreach (var d in dirs)
                Assert.IsTrue(found.Any(x => x.Equals(d, StringComparison.OrdinalIgnoreCase)));

            found = container.GetDirectoryNames("D?rA");
            Assert.AreEqual(1, found.Length);
            Assert.IsTrue(found[0].Equals(dirs[0], StringComparison.OrdinalIgnoreCase));

            found = container.GetDirectoryNames("dira");
            Assert.AreEqual(1, found.Length);
            Assert.IsTrue(found[0].Equals(dirs[0], StringComparison.OrdinalIgnoreCase));

            container.Dispose();
            device.Dispose();
        }

        [Test]
        public void GetFileNames_ShouldReturnAllCreatedFiles()
        {
            var device = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME_CONTAINER, ContainerSize);
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
                Assert.IsTrue(found.Any(x => x.Equals(f, StringComparison.OrdinalIgnoreCase)));

            container.Dispose();
            device.Dispose();
        }

        [Test]
        public void GetFileNames_WithPattern_ShouldFilterResults()
        {
            var device = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME_CONTAINER, ContainerSize);
            string[] files = { "fileA.txt", "fileB.txt", "fileC.txt" };
            foreach (var f in files)
            {
                using (var stream = container.CreateFile(f))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write("data");
                }
            }

            var found = container.GetFileNames("*");
            Assert.AreEqual(files.Length, found.Length);
            foreach (var f in files)
                Assert.IsTrue(found.Any(x => x.Equals(f, StringComparison.OrdinalIgnoreCase)));

            found = container.GetFileNames("*.txt");
            Assert.AreEqual(files.Length, found.Length);
            foreach (var f in files)
                Assert.IsTrue(found.Any(x => x.Equals(f, StringComparison.OrdinalIgnoreCase)));

            found = container.GetFileNames("file?.txt");
            Assert.AreEqual(files.Length, found.Length);
            foreach (var f in files)
                Assert.IsTrue(found.Any(x => x.Equals(f, StringComparison.OrdinalIgnoreCase)));

            found = container.GetFileNames("filea.txt");
            Assert.AreEqual(1, found.Length);
            Assert.IsTrue(found[0].Equals(files[0], StringComparison.OrdinalIgnoreCase));

            container.Dispose();
            device.Dispose();

        }

        [Test]
        public void FileOpenOrCreate_ThrowOnMissingDirectory()
        {
            var file_name = "folder/file1.txt";
            string fileContent = "Persisted!";

            var device = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
            var container = device.OpenContainer(MY_GAME_CONTAINER, ContainerSize);

            // These should throw.
            Assert.Throws(typeof(DirectoryNotFoundException), () => container.CreateFile(file_name));
            Assert.Throws(typeof(DirectoryNotFoundException), () => container.OpenFile(file_name, FileMode.Open));

            // Create the folder then it should work.
            container.CreateDirectory("folder");
            Assert.DoesNotThrow(() => container.CreateFile(file_name));
            Assert.DoesNotThrow(() => container.OpenFile(file_name, FileMode.Open));

            container.Dispose();
            device.Dispose();
        }

        private static string GetContainerPath(string title, string container)
        {
            string root;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            else
            {
                root = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (string.IsNullOrEmpty(root))
                {
                    root = Environment.GetEnvironmentVariable("HOME");
                    if (string.IsNullOrEmpty(root))
                        root = ".";
                    else
                        root = Path.Combine(root, ".local", "share");
                }
            }

            return Path.Combine(root, "SavedGames", title, container);
        }

        [Test]
        public void OpenContainer_CrashRecovery()
        {
            var container_name = "crash_test";
            var file_name = "file1.txt";
            string fileContent = "Persisted!";

            var container_path = GetContainerPath(MY_GAME_TITLE, container_name);
            if (Directory.Exists(container_path))
                Directory.Delete(container_path, true);

            // First write the new container to disk and commit it.
            {
                var device = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
                var container = device.OpenContainer(container_name, ContainerSize);
                using (var stream = container.CreateFile(file_name))
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(fileContent);
                }

                // Flush the save data changes to disk.
                container.Commit();
                container.Dispose();
                device.Dispose();
            }

            // Simulate a crash by renaming the container to ~name.
            {
                var crash_path = container_path.Replace(container_name, $"~{container_name}");
                if (Directory.Exists(crash_path))
                    Directory.Delete(crash_path, true);
                Directory.Move(container_path, crash_path);
            }

            // Check that the crash recovered the content.
            {
                var device = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
                var container = device.OpenContainer(container_name, ContainerSize);
                Assert.IsTrue(container.FileExists(file_name));

                using (var stream = container.OpenFile(file_name, FileMode.Open))
                using (var reader = new StreamReader(stream))
                {
                    var readContent = reader.ReadToEnd();
                    Assert.AreEqual(fileContent, readContent);
                }

                container.Dispose();
                device.Dispose();
            }
        }

        [Test]
        public void SaveData_And_Reload_ShouldRestoreFilesAndDirectories()
        {
            string[] dirs = { "Dir1", "Dir2" };
            string[] files = { "file1.txt", "file2.txt" };
            string fileContent = "Persisted!";

            // First write the new container to disk and commit it.
            {
                var device = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
                var container = device.OpenContainer(MY_GAME_CONTAINER, ContainerSize);

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

                // Flush the save data changes to disk.
                container.Commit();
                container.Dispose();
                device.Dispose();
            }

            // Now load the written container and verify its content.
            {
                var device2 = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
                var container2 = device2.OpenContainer(MY_GAME_CONTAINER, ContainerSize);

                // Check directories
                var foundDirs = container2.GetDirectoryNames();
                foreach (var d in dirs)
                    Assert.IsTrue(foundDirs.Any(x => x.Equals(d, StringComparison.OrdinalIgnoreCase)), $"Directory '{d}' missing after reload");

                // Check files and contents
                var foundFiles = container2.GetFileNames();
                foreach (var f in files)
                {
                    Assert.IsTrue(foundFiles.Any(x => x.Equals(f, StringComparison.OrdinalIgnoreCase)), $"File '{f}' missing after reload");
                    using (var stream = container2.OpenFile(f, FileMode.Open))
                    using (var reader = new StreamReader(stream))
                    {
                        string readContent = reader.ReadToEnd();
                        Assert.AreEqual(fileContent, readContent, $"File '{f}' content mismatch after reload");
                    }
                }

                container2.Dispose();
                device2.Dispose();
            }

            // Now test cleaning up the container content.
            {
                var device3 = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
                var container3 = device3.OpenContainer(MY_GAME_CONTAINER, ContainerSize);

                // Delete the directories.
                foreach (var d in dirs)
                    container3.DeleteDirectory(d);

                // Delete the files.
                foreach (var f in files)
                    container3.DeleteFile(f);

                container3.Commit();
                container3.Dispose();
                device3.Dispose();
            }

            // Final test to ensure the container doesn't include our stuff.
            {
                var device4 = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
                var container4 = device4.OpenContainer(MY_GAME_CONTAINER, ContainerSize);

                foreach (var d in dirs)
                    Assert.IsFalse(container4.DirectoryExists(d));
                foreach (var f in files)
                    Assert.IsFalse(container4.FileExists(f));

                container4.Dispose();
                device4.Dispose();
            }

            // One more... delete the container.
            {
                var device5 = new StorageDevice(MY_GAME_TITLE, PlayerIndex.One);
                device5.DeleteContainer(MY_GAME_CONTAINER);

                // TODO: Add ContainerExists!
            }
        }
    }
}

#endif
