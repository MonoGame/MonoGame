// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if NATIVE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Microsoft.Xna.Framework.Storage.StorageContainer;

namespace Microsoft.Xna.Framework.Storage
{
    internal class StorageStream : Stream
    {
        private readonly Blob _blob;

        private readonly bool _readable;
        private readonly bool _writeable;

        public override bool CanRead => _readable;

        public override bool CanSeek => true;

        public override bool CanWrite => _writeable;

        public override long Length => _blob.content.Length;

        public override long Position
        {
            get => _blob.content.Position;
            set
            {
                _blob.content.Position = value;
            }
        }

        public StorageStream(Blob blob, bool readable, bool writeable)
        {
            _blob = blob;
            _readable = readable;
            _writeable = writeable;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_readable)
                return _blob.content.Read(buffer, offset, count);

            throw new NotSupportedException("The stream is not readable.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _blob.content.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _blob.content.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_writeable)
            {
                _blob.content.Write(buffer, offset, count);
                _blob.dirty = true;
                return;
            }

            throw new NotSupportedException("The stream is not writable.");
        }

        protected unsafe override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}

#endif
