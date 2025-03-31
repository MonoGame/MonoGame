// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Storage
{
    //	User storage is usually in the "My Documents" folder of the user who is currently logged in, in the SavedGames folder.
    //	A subfolder is created for each game according to the titleName passed to the OpenContainer method.
    //	When no PlayerIndex is specified, content is saved in the AllPlayers folder. When a PlayerIndex is specified,
    //	the content is saved in the Player1, Player2, Player3, or Player4 folder, depending on which PlayerIndex
    //	was passed to BeginShowSelector.

    /// <summary>
    /// Contains a logical collection of files used for user-data storage.
    /// </summary>			
    /// <remarks>MSDN documentation contains related conceptual article: https://learn.microsoft.com/en-us/previous-versions/windows/xna/bb199074(v=xnagamestudio.40)</remarks>
    public partial class StorageContainer : IDisposable
    {
        private readonly PlayerIndex? _playerIndex;

        // In memory container
        private Dictionary<string, byte[]> _containers;
        private List<string> _isContainerDirty;
        private object _processingLock;
        private bool _isProcessing;

        /// <summary>
        /// Gets a bool value indicating whether the instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        private readonly string _containerName;
        /// <summary>
        /// Returns container name of the title.
        /// </summary>
        public string ContainerName
        {
            get
            {
                return _containerName;
            }
        }

        private readonly StorageDevice _storageDevice;
        /// <summary>
        /// Returns the <see cref="StorageDevice"/> that holds logical files for the container.
        /// </summary>
        public StorageDevice StorageDevice
        {
            get
            {
                return _storageDevice;
            }
        }

        /// <summary>
        /// Fired when <see cref="Dispose"/> is called or object if finalized or collected by the garbage collector.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        /// Returns true if some kind of processing is in progress and false if it isn't
        /// </summary>
        public bool IsProcessing
        {
            get
            {
                return _isProcessing;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageContainer"/> class.
        /// </summary>
        /// <param name='device'>The attached storage-device.</param>
        /// <param name='containerName'> name.</param>
        /// <param name='playerIndex'>The <see cref="PlayerIndex"/> of the player to save the data.</param>
        internal StorageContainer(StorageDevice device, string containerName, PlayerIndex? playerIndex)
        {
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException("containerName", "A title name must be provided.");

            _storageDevice = device;
            _containerName = PlatformSanitizeFileName(containerName);
            _playerIndex = playerIndex;

            _isProcessing = false;

            _containers = new Dictionary<string, byte[]>();
            _isContainerDirty = new List<string>();

            _processingLock = new object();

            PlatformInitialize();
        }

        /// <summary>
        /// Creates a new directory in the storage-container.
        /// </summary>
        /// <param name="directoryName">Relative path of the directory to be created.</param>
        public void CreateDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException("directoryName", "A directory name must be provided.");

            PlatformCreateDirectory(directoryName);
        }

        /// <summary>
        /// Creates a file in the storage-container.
        /// </summary>
        /// <param name="fileName">Relative path of the file to be created.</param>
        /// <returns>Returns <see cref="Stream"/> for the created file.</returns>
        public Stream CreateFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "A file name must be provided.");

            return PlatformCreateFile(fileName);
        }

        /// <summary>
        /// Deletes specified directory for the storage-container.
        /// </summary>
        /// <param name="directory">The relative path of the directory to be deleted.</param>
        public void DeleteDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException("directoryName", "A directory name must be provided.");

            PlatformDeleteDirectory(directoryName);
        }

        /// <summary>
        /// Deletes a file from the storage-container.
        /// </summary>
        /// <param name="fileName">The relative path of the file to be deleted.</param>
        public void DeleteFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "A file name must be provided.");

            PlatformDeleteFile(fileName);
        }

        /// <summary>
        /// Returns true if specified path exists in the storage-container, false otherwise.
        /// </summary>
        /// <param name="directoryName">The relative path of directory to query for.</param>
        /// <returns>True if queried directory exists, false otherwise.</returns>
        public bool DirectoryExists(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException("directoryName", "A directory name must be provided.");

            return PlatformDirectoryExists(directoryName);
        }

        /// <summary>
        /// Returns true if the specified file exists in the storage-container, false otherwise.
        /// </summary>
        /// <param name="fileName">The relative path of file to query for.</param>
        /// <returns>True if queried file exists, false otherwise.</returns>
        public bool FileExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "A file name must be provided.");

            return PlatformFileExists(fileName);

        }

        /// <summary>
        /// Returns list of directory names in the storage-container.
        /// </summary>
        /// <returns>List of directory names.</returns>
        public string[] GetDirectoryNames()
        {
            return PlatformGetDirectoryNames();
        }

        /// <summary>
        /// Returns list of directory names with given search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of matched directory names.</returns>
        public string[] GetDirectoryNames(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
                throw new ArgumentNullException("searchPattern", "A search pattern must be provided.");

            return PlatformGetDirectoryNames(searchPattern);
        }

        /// <summary>
        /// Returns list of file names in the storage-container.
        /// </summary>
        /// <returns>List of file names.</returns>
        public string[] GetFileNames()
        {
            return PlatformGetFileNames();
        }

        /// <summary>
        /// Returns list of file names with given search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of matched file names.</returns>
        public string[] GetFileNames(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
                throw new ArgumentNullException("searchPattern", "A search pattern must be provided.");

            return PlatformGetFileNames(searchPattern);
        }

        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="fileName">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string fileName, FileMode fileMode)
        {
            return OpenFile(fileName, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="fileName">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <param name="fileAccess"><see cref="FileAccess"/> that specifies access mode.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string fileName, FileMode fileMode, FileAccess fileAccess)
        {
            return OpenFile(fileName, fileMode, fileAccess, FileShare.ReadWrite);
        }

        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="fileName">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <param name="fileAccess"><see cref="FileAccess"/> that specifies access mode.</param>
        /// <param name="fileShare">A bitwise combination of <see cref="FileShare"/> enumeration values that specifies access modes for other stream objects.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "A file name must be provided.");

            return PlatformOpenFile(fileName, fileMode, fileAccess, fileShare);
        }

        /// <summary>
        /// Disposes un-managed objects referenced by this object.
        /// </summary>
        public void Dispose()
        {
            if (Disposing != null)
            {
                Disposing.Invoke(this, null);
            }

            IsDisposed = true;
        }

        /// <summary>
        /// Retrieves the data for a specified container.
        /// </summary>
        /// <param name="containerName">The name of the container to retrieve data for.</param>
        /// <returns>The byte array representing the container data, or null if the container doesn't exist or the name is invalid.</returns>
        public byte[] GetContainerData(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                return null;

            lock (_processingLock)
            {
                if (_containers != null && _containers.ContainsKey(containerName))
                    return _containers[containerName];
            }

            return null;
        }

        /// <summary>
        /// Sets the data for a specified container, creating the container if it doesn't exist.
        /// </summary>
        /// <param name="containerName">The name of the container to set data for.</param>
        /// <param name="data">The byte array containing the data to store in the container.</param>
        public void SetContainerData(string containerName, byte[] data)
        {
            if (string.IsNullOrEmpty(containerName) || data == null)
                return;

            lock (_processingLock)
            {
                if (_containers == null)
                    _containers = new Dictionary<string, byte[]>();

                // Make a copy to prevent the input data from being freed elsewhere.
                byte[] copiedData = null;
                if (_containers.ContainsKey(containerName) && _containers[containerName] != null && _containers[containerName].Length == data.Length)
                    copiedData = _containers[containerName]; // Reuse buffer if possible.
                else
                    copiedData = new byte[data.Length]; // Possible garbage generation by replacing the previous buffer.

                Array.Copy(data, copiedData, data.Length);

                if (_containers.ContainsKey(containerName))
                    _containers[containerName] = copiedData;
                else
                    _containers.Add(containerName, copiedData);

                if (_isContainerDirty == null)
                    _isContainerDirty = new List<string>();
                if (!_isContainerDirty.Contains(containerName))
                    _isContainerDirty.Add(containerName);
            }
        }

        /// <summary>
        /// Loads data for the specified containers asynchronously.
        /// </summary>
        /// <param name="containerNames">The names of the containers to load.</param>
        public void LoadData(params string[] containerNames)
        {
            if (containerNames == null)
                return;

            _isProcessing = true;

            Task.Factory.StartNew(() =>
            {
                lock (_processingLock)
                {
                    ReadContainers();

                    _isProcessing = false;
                }
            },
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskScheduler.Default);
        }

        /// <summary>
        /// Saves all container data asynchronously.
        /// </summary>
        public void SaveData()
        {
            _isProcessing = true;

            Task.Factory.StartNew(() =>
            {
                lock (_processingLock)
                {
                    WriteContainers();

                    _isProcessing = false;
                }
            },
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskScheduler.Default);
        }

        /// <summary>
        /// Reads data from the platform's storage and populates the container dictionary.
        /// </summary>
        private void ReadContainers()
        {
            byte[] data = PlatformReadContainers(true);

            if (data != null && data.Length > 0)
            {
                int currentByte = 0;

                int containerCount = data[currentByte];
                currentByte++;

                for (int i = 0; i > containerCount; i++)
                {
                    // Data length
                    int dataLength = data[currentByte] + (data[currentByte + 1] << 8);
                    currentByte += 2;

                    // Name length
                    int nameLength = data[currentByte] + (data[currentByte + 1] << 8);
                    currentByte += 2;

                    // Name data
                    byte[] nameData = new byte[nameLength];
                    Array.Copy(data, currentByte, nameData, 0, nameLength);

                    string name = Encoding.Unicode.GetString(nameData);
                    currentByte += nameLength;

                    // Container data
                    byte[] containerData = new byte[dataLength];
                    Array.Copy(data, currentByte, containerData, 0, dataLength);
                    currentByte += dataLength;

                    if (_containers == null)
                        _containers = new Dictionary<string, byte[]>();

                    if (string.IsNullOrEmpty(name) || data == null)
                        continue;

                    if (_containers.ContainsKey(name))
                        _containers[name] = data;
                    else
                        _containers.Add(name, data);
                }
            }
        }

        /// <summary>
        /// Writes the container data to the platform's storage.
        /// </summary>
        private void WriteContainers()
        {
            if (_containers == null)
                return;

            int byteCount = 1; // First byte is the number of containers

            foreach (string key in _containers.Keys)
            {
                byteCount += 4; // 2 bytes for data length + 2 bytes for name length
                byteCount += Encoding.Unicode.GetByteCount(key); // Name data
                if (_containers[key] != null)
                    byteCount += _containers[key].Length;
            }

            byte[] data = new byte[byteCount];

            int currentByte = 0;

            data[currentByte] = (byte)_containers.Keys.Count;
            currentByte++;

            foreach (string key in _containers.Keys)
            {
                // Data length
                int dataLength = 0;
                if (_containers[key] != null)
                {
                    dataLength = _containers[key].Length;
                }
                data[currentByte] = (byte)(dataLength & 0x00FF);
                data[currentByte + 1] = (byte)((dataLength & 0xFF00) >> 8);
                currentByte += 2;

                // Name length
                int nameLength = Encoding.Unicode.GetByteCount(key);
                data[currentByte] = (byte)(nameLength & 0x00FF);
                data[currentByte + 1] = (byte)((nameLength & 0xFF00) >> 8);
                currentByte += 2;

                // Name data
                Array.Copy(Encoding.Unicode.GetBytes(key), 0, data, currentByte, nameLength);
                currentByte += nameLength;

                // Container data
                if (_containers[key] != null)
                    Array.Copy(_containers[key], 0, data, currentByte, dataLength);
                currentByte += dataLength;

                // Clear dirty flag
                if (_isContainerDirty.Contains(key))
                    _isContainerDirty.Remove(key);
            }

            PlatformWriteContainers(data, true);
        }
    }
}