// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Storage
{
    partial class StorageContainer
    {
        internal unsafe MG_StorageContainer* _handle;

        private unsafe void PlatformDispose()
        {
            MG_Storage.CloseContainer(_handle);
            _handle = null;
        }

        public unsafe MemoryStream PlatformLoadFile(string path)
        {
            byte* data = MG_Storage.FileLoad(_handle, path, out int size);
            var content = new MemoryStream(size);
            content.SetLength(size);
            Marshal.Copy((IntPtr)data, content.GetBuffer(), 0, size);
            return content;
        }

        public unsafe void PlatformUpdateCache()
        {
            _cache = new Dictionary<string, Blob>();

            byte** filesAndDirectories = null;
            int count = 0;

            MG_Storage.EnumerateContent(_handle, out filesAndDirectories, out count);

            for (int i=0; i < count; i++)
            {
                byte* utf8_string = filesAndDirectories[i];
                var path = Marshal.PtrToStringUTF8((nint)utf8_string);

                var blob = new Blob();
                if (path.EndsWith('/'))
                    blob.directory = true;

                _cache[path] = blob;
            }
        }

        public unsafe void PlatformCommit()
        {
            // TODO: How do we enforce an atomic commit
            // on platforms without that functionality?

            // First we process the directories.
            foreach (var pair in _cache)
            {
                var blob = pair.Value;
                if (!blob.directory)
                    continue;

                if (!blob.dirty)
                    continue;

                var path = pair.Key;

                if (blob.deleted)
                    MG_Storage.DirectoryDelete(_handle, path);
                else
                    MG_Storage.DirectoryCreate(_handle, path);
            }

            // Then we process the files.
            foreach (var pair in _cache)
            {
                var blob = pair.Value;
                if (blob.directory)
                    continue;

                if (!blob.dirty)
                    continue;

                var path = pair.Key;

                if (blob.deleted)
                    MG_Storage.FileDelete(_handle, path);
                else
                {
                    var data = blob.content.ToArray();
                    fixed(byte* ptr = data)
                        MG_Storage.FileSave(_handle, path, ptr, data.Length);
                }
            }

            // Let the native layer the changes are done.
            MG_Storage.CommitContainer(_handle);

            // Cleanup the deleted/dirty entries.
            var remove = new List<string>();
            foreach (var pair in _cache)
            {
                if (pair.Value.deleted)
                    remove.Add(pair.Key);
                else
                    pair.Value.dirty = false;
            }
            foreach (var key in remove)
                _cache.Remove(key);
        }
    }
}
