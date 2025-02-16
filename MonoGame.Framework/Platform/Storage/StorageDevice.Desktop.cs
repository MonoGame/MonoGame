using System;
using System.IO;

namespace Microsoft.Xna.Framework.Storage
{
    partial class StorageDevice
    {
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

        internal static string StorageRoot
        {
            get
            {
#if LINUX
                string osConfigDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (string.IsNullOrEmpty(osConfigDir))
                {
                    osConfigDir = Environment.GetEnvironmentVariable("HOME");
                    if (string.IsNullOrEmpty(osConfigDir))
                    {
                        return "."; // Oh well.
                    }
                    osConfigDir += "/.local/share";
                }
                return osConfigDir;
#elif MAC
                string osConfigDir = Environment.GetEnvironmentVariable("HOME");
                if (string.IsNullOrEmpty(osConfigDir))
                {
                    return "."; // Oh well.
                }
                osConfigDir += "/Library/Application Support";
                return osConfigDir;
#else
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
            }
        }

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