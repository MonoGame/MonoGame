using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Storage
{
    partial class StorageDevice
    {
        /// <summary>
        /// Gets the amount of free space on the device.
        /// </summary>
        public long FreeSpace
        {
            get
            {
                try
                {
                    return new DriveInfo(GetDevicePath).AvailableFreeSpace;
                }
                catch (Exception)
                {
                    /* TODO StorageDeviceHelper.Path = StorageRoot;
                    return StorageDeviceHelper.FreeSpace;*/
                    return -1;
                }
            }
        }

        /// <summary>
        /// Gets whether the device is connected.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                try
                {
                    return new DriveInfo(GetDevicePath).IsReady;
                }
                catch (Exception)
                {
                    return true;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        internal static string StorageRoot
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    string osConfigDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                    if (string.IsNullOrEmpty(osConfigDir))
                    {
                        string homeDir = Environment.GetEnvironmentVariable("HOME");
                        if (string.IsNullOrEmpty(homeDir))
                        {
                            return "."; // Fallback to current directory
                        }
                        osConfigDir = Path.Combine(homeDir, ".local", "share");
                    }
                    return osConfigDir;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    string osConfigDir = Environment.GetEnvironmentVariable("HOME");
                    if (string.IsNullOrEmpty(osConfigDir))
                    {
                        return "."; // Fallback to current directory
                    }
                    osConfigDir = Path.Combine(osConfigDir, "Library", "Application Support");
                    return osConfigDir;
                }
                else // Windows?
                {
                    return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
            }
        }

        /// <summary>
        /// Gets the total amount of space on the device.
        /// </summary>
        public long TotalSpace
        {
            get
            {
                try
                {
                    return new DriveInfo(GetDevicePath).TotalSize;
                }
                catch (Exception)
                {
                    /* TODO StorageDeviceHelper.Path = StorageRoot;
                    return StorageDeviceHelper.TotalSpace; */
                    return -1;
                }
            }
        }
    }
}