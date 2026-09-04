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
    internal class StorageStream : Stream
    {
        private readonly MemoryStream _stream;

        private readonly bool _readable;
        private readonly bool _writeable;

        public override bool CanRead => _readable;

        public override bool CanSeek => true;

        public override bool CanWrite => _writeable;

        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set
            {
                _stream.Position = value;
            }
        }

        public StorageStream(MemoryStream stream, bool readable, bool writeable)
        {
            _stream = stream;
            _readable = readable;
            _writeable = writeable;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_readable)
                return _stream.Read(buffer, offset, count);

            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_writeable)
            {
                _stream.Write(buffer, offset, count);
                return;
            }

            throw new NotSupportedException();
        }

        protected unsafe override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
