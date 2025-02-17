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
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
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